using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FishingBonus.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FishingBonus;

[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
static class ObjectDBAwakePatch
{
    public static bool Initialized = false;

    static void Postfix(ObjectDB __instance)
    {
        // Load existing config or create a new one if the file does not exist
        DropConfig dropConfig = File.Exists(FishingBonusPlugin.YamlFileFullPath)
            ? ConfigLoader.LoadConfig(FishingBonusPlugin.YamlFileFullPath)
            : new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };

        IEnumerable<GameObject> allFishItems = __instance.m_items.Where(i => i.GetComponent<ItemDrop>() != null && i.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Fish);

        bool configUpdated = false;
        foreach (GameObject fishItem in allFishItems)
        {
            Fish fishComponent = fishItem.GetComponent<Fish>();
            if (fishComponent != null)
            {
                string fishName = Utils.GetPrefabName(fishItem.name);
                // Check if this fish is already in the config
                if (!dropConfig.FishDrops.ContainsKey(fishName))
                {
                    // New fish found, log and create a new entry
                    FishingBonusPlugin.FishingBonusLogger.LogDebug($"New fish found: {fishName}, adding to YAML file.");
                    FishDropConfig fishDropConfig = new FishDropConfig { Drops = new List<ExtraDrop?>(), AddToDefaultDrops = false, DropMin = 1, DropMax = 1, DropChance = 1f, OneOfEach = false };
                    DropTable extraDrops = fishComponent.m_extraDrops;
                    
                    fishDropConfig.DropChance = fishComponent.m_extraDrops.m_dropChance;
                    fishDropConfig.DropMin = fishComponent.m_extraDrops.m_dropMin;
                    fishDropConfig.DropMax = fishComponent.m_extraDrops.m_dropMax;
                    fishDropConfig.OneOfEach = fishComponent.m_extraDrops.m_oneOfEach;

                    if (!extraDrops.IsEmpty())
                    {
                        foreach (DropTable.DropData drop in extraDrops.m_drops)
                        {
                            if (drop.m_item != null)
                            {
                                ExtraDrop? extraDrop = new ExtraDrop
                                {
                                    Item = drop.m_item.name,
                                    MinStack = drop.m_stackMin,
                                    MaxStack = drop.m_stackMax,
                                    RelativeWeight = drop.m_weight
                                };
                                fishDropConfig.Drops.Add(extraDrop);
                            }
                        }
                    }

                    dropConfig.FishDrops[fishName] = fishDropConfig;
                    configUpdated = true;
                }

                if (!Initialized)
                {
                    // Cache the original drops for later use
                    if (!FishingBonusPlugin.originalDropsCache.ContainsKey(fishName))
                    {
                        FishingBonusPlugin.originalDropsCache[fishName] = fishComponent.m_extraDrops.m_drops;
                    }

                    Initialized = true;
                }
            }
            else
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning($"Fish Item {fishItem.name} doesn't have a Fish component.");
            }
        }

        // Save the updated config back to the YAML file, if any changes were made
        if (configUpdated)
        {
            ConfigLoader.SaveConfig(dropConfig, FishingBonusPlugin.YamlFileFullPath);
            FishingBonusPlugin.FishingBonusLogger.LogDebug("YAML file updated with new fish.");
        }

        try
        {
            var yamlContent = File.ReadAllText(FishingBonusPlugin.YamlFileFullPath);
            FishingBonusPlugin.FishDropsData.AssignLocalValue(yamlContent);
            FishingBonusPlugin.FishingBonusLogger.LogInfo("Fish drops configuration loaded into memory.");
        }
        catch (Exception e)
        {
            FishingBonusPlugin.FishingBonusLogger.LogError($"Failed to read or assign the YAML content to FishDropsData: {e.Message}");
        }
    }
}

[HarmonyPatch(typeof(FishingFloat), nameof(FishingFloat.Catch))]
static class FishingFloatCatchPatch
{
    static void Prefix(FishingFloat __instance, Fish fish, Character owner)
    {
        try
        {
            if (!fish.m_extraDrops.IsEmpty())
            {
                var fishName = Utils.GetPrefabName(fish.name);
                if (FishingBonusPlugin.FishDropsData.Value.Contains(fishName))
                {
                    FishingBonusPlugin.FishingBonusLogger.LogDebug("Fish has extra drops, applying bonus.");
                    FishDropConfig fishDropConfig = ConfigLoader.LoadFromText(FishingBonusPlugin.FishDropsData.Value).FishDrops[fishName];
                    
                    fish.m_extraDrops.m_dropChance = fishDropConfig.DropChance;
                    fish.m_extraDrops.m_dropMin = fishDropConfig.DropMin;
                    fish.m_extraDrops.m_dropMax = fishDropConfig.DropMax;
                    fish.m_extraDrops.m_oneOfEach = fishDropConfig.OneOfEach;

                    
                    List<DropTable.DropData> combinedDrops = new List<DropTable.DropData>();

                    if (fishDropConfig.AddToDefaultDrops && FishingBonusPlugin.originalDropsCache.TryGetValue(fishName, out List<DropTable.DropData>? originalDrops))
                    {
                        combinedDrops.AddRange(originalDrops);
                    }

                    foreach (var extraDrop in fishDropConfig.Drops)
                    {
                        var itemPrefab = ObjectDB.instance.GetItemPrefab(extraDrop?.Item);
                        if (itemPrefab != null && extraDrop != null)
                        {
                            combinedDrops.Add(new DropTable.DropData
                            {
                                m_item = itemPrefab,
                                m_stackMin = extraDrop.MinStack,
                                m_stackMax = extraDrop.MaxStack,
                                m_weight = extraDrop.RelativeWeight
                            });
                        }
                    }

                    // Deduplicate based on item name
                    fish.m_extraDrops.m_drops = combinedDrops.GroupBy(d => d.m_item.name).Select(g => g.First()).ToList();
                }
            }
        }
        catch (Exception e)
        {
            FishingBonusPlugin.FishingBonusLogger.LogError($"Error applying fish drops for {fish.name}: {e.Message}");
        }
    }
}
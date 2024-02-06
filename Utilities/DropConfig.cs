using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FishingBonus.Utilities;

public class DropConfig
{
    public Dictionary<string, FishDropConfig> FishDrops { get; set; } = new Dictionary<string, FishDropConfig>();
}

public class FishDropConfig
{
    public bool AddToDefaultDrops { get; set; } = false;

    public int DropMin { get; set; } = 1;
    public int DropMax { get; set; } = 1;
    public float DropChance { get; set; } = 1f;
    public bool OneOfEach { get; set; } = false;
    public List<ExtraDrop?> Drops { get; set; } = new List<ExtraDrop?>();
}

public class ExtraDrop
{
    public string Item { get; set; } = null!;
    public int MinStack { get; set; }
    public int MaxStack { get; set; }

    public float RelativeWeight { get; set; }
}

public static class ConfigLoader
{
    public static DropConfig LoadConfig(string filePath)
    {
        try
        {
            var configText = File.ReadAllText(filePath).Trim();
            // Check if the file is empty or contains only whitespace
            if (string.IsNullOrEmpty(configText))
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning("YAML file is empty or contains only whitespace. Returning new DropConfig.");
                return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
            }

            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var dropConfig = deserializer.Deserialize<DropConfig>(configText);

            // Additional check to ensure deserialization didn't return null
            if (dropConfig == null || dropConfig.FishDrops == null)
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning("Failed to deserialize YAML content. Returning new DropConfig.");
                return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
            }

            return dropConfig;
        }
        catch (Exception e)
        {
            FishingBonusPlugin.FishingBonusLogger.LogError($"Exception caught while loading YAML file: {e.Message}");
            // Return a new DropConfig to ensure the caller always receives a valid object
            return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
        }
    }

    public static DropConfig LoadFromText(string yamlText)
    {
        try
        {
            var trimmedYamlText = yamlText.Trim();
            // Check if the text is empty or contains only whitespace
            if (string.IsNullOrEmpty(trimmedYamlText))
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning("YAML text is empty or contains only whitespace. Returning new DropConfig.");
                return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
            }

            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var dropConfig = deserializer.Deserialize<DropConfig>(trimmedYamlText);

            // Additional check to ensure deserialization didn't return null
            if (dropConfig == null || dropConfig.FishDrops == null)
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning("Failed to deserialize YAML text. Returning new DropConfig.");
                return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
            }

            return dropConfig;
        }
        catch (Exception e)
        {
            FishingBonusPlugin.FishingBonusLogger.LogError($"Exception caught while loading YAML text: {e.Message}");
            // Return a new DropConfig to ensure the caller always receives a valid object
            return new DropConfig { FishDrops = new Dictionary<string, FishDropConfig>() };
        }
    }


    public static void SaveConfig(DropConfig dropConfig, string filePath)
    {
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(dropConfig);
        File.WriteAllText(filePath, yaml);
    }

    internal static void ApplyFishDropsConfig()
    {
        if (!File.Exists(FishingBonusPlugin.YamlFileFullPath))
        {
            FishingBonusPlugin.FishingBonusLogger.LogWarning($"YAML file {FishingBonusPlugin.YamlFileName} does not exist. Skipping fish drops update.");
            return;
        }

        DropConfig dropConfig;
        try
        {
            dropConfig = LoadFromText(FishingBonusPlugin.FishDropsData.Value);
        }
        catch (Exception e)
        {
            FishingBonusPlugin.FishingBonusLogger.LogError($"Failed to load or parse {FishingBonusPlugin.YamlFileName}: {e.Message}");
            return;
        }

        ObjectDB objectDB = ObjectDB.instance;
        if (objectDB == null)
        {
            return;
        }

        foreach (var fishDropEntry in dropConfig.FishDrops)
        {
            var fishPrefab = objectDB.GetItemPrefab(fishDropEntry.Key);
            if (fishPrefab == null)
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning($"Fish prefab '{fishDropEntry.Key}' not found in ObjectDB.");
                continue;
            }

            var fishComponent = fishPrefab.GetComponent<Fish>();
            if (fishComponent == null)
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning($"Fish prefab '{fishDropEntry.Key}' does not have a Fish component.");
                continue;
            }

            UpdateFishDrops(fishComponent, fishDropEntry.Value);
        }
    }

    private static void UpdateFishDrops(Fish fishComponent, FishDropConfig config)
    {
        fishComponent.m_extraDrops.m_dropMin = config.DropMin;
        fishComponent.m_extraDrops.m_dropMax = config.DropMax;
        fishComponent.m_extraDrops.m_dropChance = config.DropChance;
        fishComponent.m_extraDrops.m_oneOfEach = config.OneOfEach;
        
        if (!config.AddToDefaultDrops)
        {
            fishComponent.m_extraDrops.m_drops.Clear();
        }

        foreach (var extraDrop in config.Drops)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(extraDrop?.Item);
            if (itemPrefab == null)
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning($"Item prefab '{extraDrop?.Item}' not found in ObjectDB for fish '{fishComponent.name}'. Skipping this drop.");
                continue;
            }

            if (fishComponent.m_extraDrops.m_drops.All(d => d.m_item != itemPrefab))
            {
                if (extraDrop != null)
                    fishComponent.m_extraDrops.m_drops.Add(new DropTable.DropData
                    {
                        m_item = itemPrefab,
                        m_stackMin = extraDrop.MinStack,
                        m_stackMax = extraDrop.MaxStack,
                        m_weight = extraDrop.RelativeWeight
                    });
            }
            else
            {
                FishingBonusPlugin.FishingBonusLogger.LogWarning($"Duplicate drop item '{extraDrop?.Item}' for fish '{fishComponent.name}' is not added.");
            }
        }
    }
}
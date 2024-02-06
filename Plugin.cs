using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FishingBonus.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FishingBonus
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class FishingBonusPlugin : BaseUnityPlugin
    {
        internal const string ModName = "FishingBonus";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = $"{Author}.{ModName}";
        private const string ConfigFileName = $"{ModGUID}.cfg";
        internal const string YamlFileName = $"{ModGUID}.yml";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static readonly string YamlFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + YamlFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource FishingBonusLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        private FileSystemWatcher _watcher = null!;
        private FileSystemWatcher _yamlwatcher = null!;
        internal static readonly CustomSyncedValue<string> FishDropsData = new(ConfigSync, "fishDropsData", "");
        internal static Dictionary<string, List<DropTable.DropData>> originalDropsCache = new();

        public void Awake()
        {
            // TODO: Clean up the code when I actually care to do so.
            
            ConfigSync.IsLocked = true;
            FishDropsData.ValueChanged += OnValChangedUpdate; // check for file changes
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            FishDropsData.ValueChanged -= OnValChangedUpdate;
            _watcher.Dispose();
            _yamlwatcher.Dispose();
        }

        private void SetupWatcher()
        {
            _watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
            _watcher.Changed += ReadConfigValues;
            _watcher.Created += ReadConfigValues;
            _watcher.Renamed += ReadConfigValues;
            _watcher.IncludeSubdirectories = true;
            _watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            _watcher.EnableRaisingEvents = true;

            _yamlwatcher = new FileSystemWatcher(Paths.ConfigPath, YamlFileName);
            _yamlwatcher.Changed += ReadYamlFiles;
            _yamlwatcher.Created += ReadYamlFiles;
            _yamlwatcher.Renamed += ReadYamlFiles;
            _yamlwatcher.IncludeSubdirectories = true;
            _yamlwatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            _yamlwatcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                FishingBonusLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                FishingBonusLogger.LogError($"There was an issue loading your {ConfigFileName}");
                FishingBonusLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        internal static void OnValChangedUpdate()
        {
            FishingBonusLogger.LogDebug("YAML file changed, updating fish drops based on new configuration.");
            try
            {
                ConfigLoader.ApplyFishDropsConfig();
            }
            catch (Exception e)
            {
                FishingBonusLogger.LogError($"Failed to deserialize {YamlFileName}: {e}");
            }
        }

        private void ReadYamlFiles(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(YamlFileFullPath)) return;
            try
            {
                FishingBonusLogger.LogDebug("ReadYamlFiles called");
                FishDropsData.AssignLocalValue(File.ReadAllText(YamlFileFullPath));
            }
            catch (Exception ex)
            {
                FishingBonusLogger.LogError($"There was an issue loading your {YamlFileName}");
                FishingBonusLogger.LogError("Please check your entries for spelling and format!\n" + ex);
            }
        }


        #region ConfigOptions

        //private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }

        #endregion
    }
}
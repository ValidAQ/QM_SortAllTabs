using HarmonyLib;
using MGSC;
using QM_SortAllTabs.LocalizationSupport;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QM_SortAllTabs
{
    /// <summary>
    /// Entry point for the mod. Exposes shared state and registers Harmony patches.
    /// </summary>
    public static class Plugin
    {
        public const string SortAllLocalizationKey = "mod.qm_sortalltabs.sort_all";

        /// <summary>Unique identifier used to register this mod's Harmony instance.</summary>
        public static string HarmonyId { get; } = "valid.QM_SortAllTabs";

        /// <summary>Shared logger for writing messages to the mod's log output.</summary>
        public static Logger Logger { get; } = new Logger();

        /// <summary>Game state provided by the mod context after configs are loaded.</summary>
        public static State State { get; private set; }

        /// <summary>
        /// Loads localization dictionary from embedded resources.
        /// </summary>
        private static void LoadLocalization()
        {
            LocalizationFileLoader.LoadFromEmbeddedJson(
                "QM_SortAllTabs.Localization.sortalltabs.localization.json",
                Assembly.GetExecutingAssembly(),
                Logger.LogError);
        }

        /// <summary>
        /// Called by the game after all configs have been loaded.
        /// Initializes mod state, loads config from disk, and applies Harmony patches.
        /// </summary>
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {
            State = context.State;

            try
            {
                // Call this if you have localization entries to load.
                LoadLocalization();
                new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
                Logger.Log("Harmony patches applied.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to apply Harmony patches.");
                Logger.LogException(ex);
            }
        }
    }
}

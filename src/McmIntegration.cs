#if MCM_PRESENT
using ModConfigMenu;
using ModConfigMenu.Contracts;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QM_SortAllTabs
{
    /// <summary>
    /// Handles optional registration with Mod Configuration Menu (MCM).
    /// This file is only compiled when MCM.dll is found in the workshop folder at build time
    /// (i.e. the MCM_PRESENT symbol is defined in QM_SortAllTabs.csproj).
    /// </summary>
    internal static class McmIntegration
    {
        /// <summary>
        /// Registers this mod with MCM if the MCM assembly is loaded at runtime.
        /// Safe to call even when MCM is not installed — the runtime check prevents any crash.
        /// Called from <see cref="Plugin.AfterConfig"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void RegisterIfPresent()
        {
            // Confirm MCM is actually loaded at runtime before touching any of its types.
            // (The mod may have been built with MCM present but the player hasn't installed it.)
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "MCM")
                {
                    Register();
                    return;
                }
            }
        }

        /// <summary>
        /// Performs the actual MCM registration.
        /// Kept in a separate method so the JIT never compiles it unless MCM is present at runtime.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Register()
        {
            // Build the list of config entries MCM will display in its UI.
            // Add one IConfigValue per field you want the player to be able to edit.
            //
            // Available types (all in ModConfigMenu.Objects / ModConfigMenu.Implementations):
            //   ConfigValue  — generic boolean / string value
            //   RangeConfig  — numeric slider with min / max
            //   DropdownConfig — fixed list of options
            //   TextBoxConfig  — free-text input
            //   StringConfig   — read-only label
            //
            // Example:
            //   new ConfigValue(
            //       key: "EnableFeature",
            //       value: Plugin.Config.EnableFeature,
            //       header: "General",
            //       defaultValue: true,
            //       tooltip: "Toggle the main feature on or off.",
            //       label: "Enable Feature"),
            var configValues = new List<IConfigValue>
            {
                // TODO: add your IConfigValue entries here.
            };

            ModConfigMenuAPI.RegisterModConfig(
                modName: "QM ModTemplate",  // Display name shown in the MCM mod list.
                configData: configValues,
                OnConfigSaved: (Dictionary<string, object> currentConfig, out string feedback) =>
                {
                    // Called when the player clicks Save in the MCM UI.
                    // `currentConfig` is a Dictionary<string, object> keyed by each entry's Key.
                    //
                    // Apply values to Plugin.Config and persist them, for example:
                    //   if (currentConfig.TryGetValue("EnableFeature", out object val))
                    //       Plugin.Config.EnableFeature = Convert.ToBoolean(val);
                    //   Plugin.Config.Save(Plugin.ConfigDirectories.ConfigPath);
                    //
                    // Set feedback to a non-null string to show an error message to the player.
                    feedback = null;
                    return true;
                });

            Plugin.Logger.Log("Registered with Mod Configuration Menu.");
        }
    }
}
#endif

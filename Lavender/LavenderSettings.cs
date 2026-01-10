using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BepInEx.Configuration;

namespace Lavender
{
    public class LavenderSettings(ConfigFile config)
    {
        public ConfigEntry<bool> DetailedLog = config.Bind<bool>("Log", "DetailedLog", false, "Enable Detailed Log Output");
        public ConfigEntry<bool> UseBepinexLog = config.Bind<bool>("Log", "UseBepinexLog", false, "Send logging through the BepinEx Plugin logger");
        public ConfigEntry<bool> SceneLoadingDoneNotification = config.Bind<bool>("Log", "SceneLoadingDoneNotification", true, "Enable 'Scene Loading Done' Notification");

        #region Data Only Mods
        public ConfigEntry<bool> EnableDataOnlyMods = config.Bind<bool>("Data Only Mods", "Enabled", true, "Enable Lavender searching for and loading data-only mods");
        public ConfigEntry<string> AdditionalDOMPath = config.Bind<string>("Data Only Mods", "DOMSearchPath", "", "Additional locations to search for data-only mods (comma-separated)");
        #endregion

        #region Plugin developer Tools
        public ConfigEntry<bool> DialoguePatcherVerboseLogging = config.Bind<bool>("Developer Tools", "DialoguePatcherVerbose", false, "Enable verbose logging when patching dialogues");
        public ConfigEntry<string> EnabledVerboseConversations = config.Bind<string>("Developer Tools", "EnabledVerboseConversations", "", 
            "Which conversations to verbosely log.  If empty, then all.  Provide a list of quote-surrounded names.  Ex: \"Tenement/Outside/Tatyana Gopnikova\" \"Steve\""); 
        public List<string> ListEnabledVerboseConversations = new List<string>();

        public bool IsDialogueVerboseLoggingAllowed(string conversationName)
        {
            if (DialoguePatcherVerboseLogging.Value)
            {
                return ListEnabledVerboseConversations.Count == 0 || ListEnabledVerboseConversations.Contains(conversationName);
            }

            return false;
        }

        #endregion

        #region UNSTABLE PATCH SETTINGS

        public ConfigEntry<bool> FurnitureShop_AddFurniture_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "FurnitureShop_AddFurniture_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");
        public ConfigEntry<bool> FurnitureShop_Restock_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "FurnitureShop_Restock_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");
        public ConfigEntry<bool> BuildingSystem_AddFurniture_Prefix_SkipOriginal = config.Bind<bool>("UNSTABLE!", "BuildingSystem_AddFurniture_Prefix_SkipOriginal", true, "Only set this to false if you know what your doing!");

        public void LogUnstableStatus()
        {
            if (!FurnitureShop_AddFurniture_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! FurnitureShop_AddFurniture_Prefix_SkipOriginal = FALSE");
            if (!FurnitureShop_Restock_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! FurnitureShop_Restock_Prefix_SkipOriginal = FALSE");
            if (!BuildingSystem_AddFurniture_Prefix_SkipOriginal.Value) LavenderLog.Log("WARNING! BuildingSystem_AddFurniture_Prefix_SkipOriginal = FALSE");
        }

        #endregion


        #region Handle custom settings
        public void SetupCustomSettingsHandling()
        {
            EnabledVerboseConversations.SettingChanged += EnabledVerboseConversations_SettingChanged;
            RegenListEnabledVerboseConversations();
        }

        private void EnabledVerboseConversations_SettingChanged(object sender, EventArgs e)
        {
            RegenListEnabledVerboseConversations();
        }

        private void RegenListEnabledVerboseConversations()
        {
            const string patternQuotedString = "\"([^\"]*)\"";

            ListEnabledVerboseConversations.Clear();

            foreach (Match match in Regex.Matches(EnabledVerboseConversations.Value, patternQuotedString))
            {
                if (match.Groups.Count > 1)
                {
                    ListEnabledVerboseConversations.Add(match.Groups[1].Value);
                }
            }
        }
        #endregion
    }
}

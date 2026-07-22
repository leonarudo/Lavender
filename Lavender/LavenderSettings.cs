using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;

namespace Lavender
{
    public class LavenderSettings(ConfigFile config)
    {
        public ConfigEntry<bool> DetailedLog = config.Bind<bool>("Log", "DetailedLog", false, "Enable Detailed Log Output");
        public ConfigEntry<bool> UseBepinexLog = config.Bind<bool>("Log", "UseBepinexLog", false, "Send logging through the BepinEx Plugin logger");
        public ConfigEntry<bool> SceneLoadingDoneNotification = config.Bind<bool>("Log", "SceneLoadingDoneNotification", true, "Enable 'Scene Loading Done' Notification");
    }
}

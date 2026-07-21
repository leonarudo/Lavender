using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lavender
{
    [BepInPlugin(LPluginInfo.PLUGIN_GUID, LPluginInfo.PLUGIN_NAME, LPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;

            Log.LogInfo($"Plugin {LPluginInfo.PLUGIN_NAME} version {LPluginInfo.PLUGIN_VERSION} is loaded!");
        }
    }
}

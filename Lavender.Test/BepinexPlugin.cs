using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.Test
{
    [BepInPlugin(LPluginInfo.PLUGIN_GUID, LPluginInfo.PLUGIN_NAME, LPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("Lavender", "8.0.0")]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;
        internal static LavenderContext ctx = null!;

        void Awake()
        {
            Log = Logger;

            ctx = Lavender.NewLavenderContext(this);

            ctx.RegisterCommand(new TestCommandEcho());

            Log.LogInfo($"Plugin {LPluginInfo.PLUGIN_NAME} version {LPluginInfo.PLUGIN_VERSION} is loaded!");
        }

        void Start()
        {
            TestCommandEcho.InternalTestCase();
        }
    }
}

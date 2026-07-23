using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lavender
{
    public class Lavender
    {
        public static Lavender instance = null!;

        public Harmony? harmony;

        public bool isInitialized = false;

        /// <summary>
        /// The build index of the last scene during the "SceneManager.sceneLoaded" callback
        /// </summary>
        public int lastLoadedScene = 0;

        /// <summary>
        /// You want to execute your mod logic only when LoadingDone = true to make sure that all game logic is already initialized!
        /// </summary>
        public bool LoadingDone = false;

        static Lavender()
        {
            lavenderContexts = new List<LavenderContext>();
        }

        public Lavender()
        {
            if (instance == null) instance = this;
            else return;

            harmony = new Harmony(LPluginInfo.PLUGIN_GUID);

            try
            {
                // harmony.PatchAll()
            }
            catch (Exception e)
            {
                LavenderLog.Error("Exception while applying Lavender patches:");
                LavenderLog.Error(e.ToString());
            }
        }

        public static List<LavenderContext> lavenderContexts;

        /// <summary>
        /// Creates a new LavenderContext for your BepInEx mod or returns an allready existing one for your mod GUID
        /// </summary>
        /// <param name="baseUnityPlugin">Your BepInEx plugin</param>
        /// <returns></returns>
        public static LavenderContext NewLavenderContext(BaseUnityPlugin baseUnityPlugin)
        {
            LavenderContext ctx = lavenderContexts.Find(x => x.ModGUID == baseUnityPlugin.Info.Metadata.GUID);

            if (ctx != null)
            {
                return ctx;
            }

            return new LavenderContext(baseUnityPlugin);
        }

        /// <summary>
        /// Sorts the LavenderContext list using the OrdinalIgnoreCase
        /// </summary>
        public static void SortLavenderContextList()
        {
            lavenderContexts = lavenderContexts.OrderBy(x => x.ModGUID, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

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
        public bool LoadingDone;

        static Lavender()
        {
            RenewStaticFields();
        }

        private static void RenewStaticFields()
        {

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
    }
}

using BepInEx;
using HarmonyLib;
using Lavender.RuntimeImporter;
using Lavender.RuntimeImporter.AssetTypes;
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

            lvAssetTypeBinder = new AssetTypeBinder(
                // Native LavenderAssetTypes
                (typeof(AssetBundleAsset),"AssetBundle"),
                (typeof(ImageAsset), "Image")
            );
        }

        public Lavender()
        {
            if (instance == null) instance = this;
            else return;

            harmony = new Harmony(LPluginInfo.PLUGIN_GUID);

            try
            {
                harmony.PatchAll(typeof(CommandLib.CommandPatches));
            }
            catch (Exception e)
            {
                LavenderLog.Error("Exception while applying Lavender patches:");
                LavenderLog.Error(e.ToString());
            }
        }

        #region LavenderContext

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

        #endregion

        #region RuntimeImporter

        internal static AssetTypeBinder lvAssetTypeBinder;

        /// <summary>
        /// Tries to find the LavenderAsset by its ID 'modguid-id' or '#lv_modguid-id'
        /// </summary>
        /// <param name="assetID">Format: 'MOD_GUID-ID' or '#lv_MOD_GUID-ID'</param>
        /// <returns></returns>
        public static LavenderAsset? GetLavenderAsset(string assetID)
        {
            string[] id = assetID.Replace("#lv_", "").Split('-');

            if (id.Length < 2)
            {
                LavenderLog.Error($"[GetLavenderAsset] Wrong assetId format! assetID: '{assetID}', correct format: '<MOD_GUID>-<ID>' e.g. 'Lavender-Asset1'");
                return null;
            }

            LavenderContext ctx = lavenderContexts.Find(x => x.ModGUID == id[0]);
            if (ctx != null)
            {
                if (ctx.OwnedAssets.TryGetValue(id[1], out var asset))
                    return asset;
                else
                {
                    LavenderLog.Error($"[GetLavenderAsset] Couldn't find asset '{assetID}': Couldn't find ID: '{id[1]}'");
                    return null;
                }
            }
            else
            {
                LavenderLog.Error($"[GetLavenderAsset] Couldn't find asset '{assetID}': Couldn't find a context for the MOD_GUID: '{id[0]}'");
                return null;
            }
        }

        #endregion
    }
}

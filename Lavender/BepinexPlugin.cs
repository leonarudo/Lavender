﻿using BepInEx;
using BepInEx.Logging;
using Lavender.RecipeLib;
using Lavender.RuntimeImporter;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lavender
{
    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;
        internal static LavenderSettings Settings = null!;

        private void Awake()
        {
            AssimpAPI.Innit();

            Log = Logger;
            Settings = new LavenderSettings(Config);
            Settings.SetupCustomSettingsHandling();

            Lavender.furniturePrefabHandlers = new Dictionary<string, Lavender.FurniturePrefabHandler>();
            Lavender.furnitureShopRestockHandlers = new Dictionary<string, Lavender.FurnitureShopRestockHandler>();
            Lavender.FurnitureDatabase = new List<Furniture>();

            Lavender.customItemDatabase = new List<Item>();

            Lavender.modifierInfos = new List<ModifierInfo>();
            Lavender.appliedCustomCraftingBaseModifiers = new Dictionary<string, int>();
            Lavender.customRecipeDatabase = new List<Recipe>();

            Lavender.customStorageCategoryDatabase = new List<StorageCategory>();
            Lavender.customStorageSpawnCategoryDatabase = new List<StorageSpawnCategory>();

            new Lavender();

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SaveController.LoadingDone += onLoadingDone;

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
            LavenderLog.Log($"{LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
            Lavender.instance.isInitialized = true;

            Settings.LogUnstableStatus();
        }

        private void OnSceneUnloaded(Scene current)
        {
            if (Lavender.FurnitureDatabase != null)
            {
                LavenderLog.Log($"Unloading {Lavender.FurnitureDatabase.Count} Furnitures...");

                Lavender.FurnitureDatabase.Clear();
                Lavender.FurnitureDBParent = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Lavender.instance.isInitialized) return;

            Debug.Log($"Scene: {scene.buildIndex}, {scene.name} loaded!");

            Lavender.instance.lastLoadedScene = scene.buildIndex;
            Lavender.instance.LoadingDone = false;
        }

        private void onLoadingDone()
        {
            Lavender.instance.LoadingDone = true;
            LavenderLog.Log("Scene Loading Done!");

            if (BepinexPlugin.Settings.SceneLoadingDoneNotification.Value)
            {
                Notifications.instance.CreateNotification("Lavender", "Scene Loading Done!", false);
            }
        }
    }
}

using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lavender
{
    [BepInPlugin(LPluginInfo.PLUGIN_GUID, LPluginInfo.PLUGIN_NAME, LPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;
        internal static LavenderSettings Settings = null!;
        public static readonly Version LavenderVersion = new Version(LPluginInfo.PLUGIN_VERSION);

        private void Awake()
        {
            Log = Logger;
            Settings = new LavenderSettings(Config);

            new Lavender();

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SaveController.LoadingDone += onLoadingDone;

            Log.LogInfo($"Plugin {LPluginInfo.PLUGIN_NAME} version {LPluginInfo.PLUGIN_VERSION} is loaded!");
            LavenderLog.Log($"{LPluginInfo.PLUGIN_NAME} version {LPluginInfo.PLUGIN_VERSION} is loaded!");
            Lavender.instance.isInitialized = true;
        }

        private void OnSceneUnloaded(Scene current)
        {
            
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!Lavender.instance.isInitialized) return;

            Debug.Log($"Scene: {scene.buildIndex}, {scene.name} loaded!");

            Lavender.instance.lastLoadedScene = scene.buildIndex;
            Lavender.instance.LoadingDone = false;

            if (scene.buildIndex == 0)
            {
                StartCoroutine(GitHubVersionChecker.CheckLatestVersionCoroutine());
            }
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

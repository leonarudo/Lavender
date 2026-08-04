using BepInEx;
using Lavender.CommandLib;
using Lavender.RuntimeImporter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static UnityEngine.InputForUI.CommandEvent;

namespace Lavender
{
    /// <summary>
    /// The LavenderContext is the main entry to Lavender. After creating one with an BaseUnityPlugin, it is used to access all higher Lavender functions
    /// </summary>
    public class LavenderContext
    {
        public string ModGUID { get; private set; }

        public string ModName { get; private set; }

        public Version ModVersion { get; private set; }

        internal LavenderContext(BaseUnityPlugin baseUnityPlugin)
        {
            ModGUID = baseUnityPlugin.Info.Metadata.GUID;
            ModName = baseUnityPlugin.Info.Metadata.Name;
            ModVersion = baseUnityPlugin.Info.Metadata.Version;

            Lavender.lavenderContexts.Add(this);
        }

        #region CommandLib

        /// <summary>
        /// Register an IConsoleCommand with the handler
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns true on success</returns>
        public bool RegisterCommand(IConsoleCommand command)
        {
            if(CommandManager.CommandRegistry.ContainsKey(command.Name)) {  return false; }

            CommandManager.CommandRegistry[command.Name] = (command, this);
            LavenderLog.Log($"[{ModGUID}] Registering command '{command.Name}'");
            return true;
        }

        /// <summary>
        /// Remove an IConsoleCommand with the given name and this LavenderContext as owner
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns false if there isn't any command registered by this name or if this LavenderContext is not the owner</returns>
        public bool RemoveCommand(string name)
        {
            if(CommandManager.CommandRegistry.TryGetValue(name, out var cmd))
            {
                if(cmd.Item2.Equals(this))
                {
                    CommandManager.CommandRegistry.Remove(name);
                    LavenderLog.Log($"[{ModGUID}] Removing command '{name}'");
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region RuntimeImporter

        /// <summary>
        /// A Dictionary with all LavenderAssets owned by your context, where the Key is an assets ID for fast lookup.
        /// </summary>
        public Dictionary<string, LavenderAsset> OwnedAssets = new Dictionary<string, LavenderAsset>();

        /// <summary>
        /// Adds all LavenderAssets from the JSON to your Contexts 'OwnedAssets'
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns>'-1' if the File couldn't be found! <br></br>
        /// '0' if there were an exception while reading the JSON <br></br>
        /// 'Count of all loaded assets' on success!</returns>
        public int AddLavenderAssets(string jsonPath)
        {
            if(File.Exists(jsonPath))
            {
                try
                {
                    string rawJsonData = File.ReadAllText(jsonPath);

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        SerializationBinder = Lavender.lvAssetTypeBinder
                    };

                    List<LavenderAsset>? assets = JsonConvert.DeserializeObject<List<LavenderAsset>>(jsonPath, settings);
                    if(assets == null)
                    {
                        LavenderLog.Error($"Error while deserializing List<LavenderAsset> at '{jsonPath}'!");
                        return 0;
                    }

                    foreach(LavenderAsset asset in assets)
                    {
                        asset.SrcFilePath = jsonPath;

                        OwnedAssets.Add(asset.ID, asset);
                    }

                    return assets.Count;

                }
                catch (Exception e)
                {
                    LavenderLog.Error($"[{ModGUID}] AddLavenderAssets: {e}");
                    return 0;
                }
            }
            else
            {
                LavenderLog.Error($"[{ModGUID}] AddLavenderAssets: Couldn't find json at: {jsonPath}");
                return -1;
            }
        }

        #endregion
    }
}

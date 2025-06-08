using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lavender.RuntimeImporter
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssetType
    {
        AssetBundle,
        Image,
        OBJ
    }

    public class LavenderAsset
    {
        public int ID { get; set; }
        public AssetType AssetType { get; set; }
        public AssetData Data { get; set; }
    }

    public class AssetData
    {
        public string ResPath { get; set; }
        public string? Name { get; set; } // only used by assetBundle
    }

    public class LavenderAssetBundle
    {
        public string ModName { get; set; }
        public List<LavenderAsset> assets { get; set; }

        public LavenderAssetBundle(string modName, string jsonPath)
        {
            ModName = modName;

            try
            {
                string rawJsonData = File.ReadAllText(jsonPath);

                List<LavenderAsset>? resources = JsonConvert.DeserializeObject<List<LavenderAsset>>(rawJsonData);
                if(resources != null)
                {
                    assets = resources;
                }
                else
                {
                    assets = new List<LavenderAsset>();

                    LavenderLog.Error($"Error while deserializing List<LavenderAsset> at '{jsonPath}'!");
                }
            }
            catch(Exception e)
            {
                assets = new List<LavenderAsset>();

                LavenderLog.Error($"LavenderAssetBundle: {e}");
            }
        }
    }
}

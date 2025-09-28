using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static NPC.NPCPathfinding;

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
        [JsonIgnore]
        public string ModName { get; set; }

        public int ID { get; set; }
        public AssetType AssetType { get; set; }
        public AssetData Data { get; set; }

        public object? GetAssetData<T>() where T : UnityEngine.Object
        {
            if (AssetType  == AssetType.AssetBundle)
            {
                var fileStream = new FileStream(Data.path, FileMode.Open, FileAccess.Read);
                var assetBundle = AssetBundle.LoadFromStream(fileStream);
                if (assetBundle == null)
                {
                    LavenderLog.Error($"Error while loading LavenderAsset '{ModName}-{ID}': couldn't get AssetBundle at '{Data.path}'!");
                    return null;
                }

                var data = assetBundle.LoadAsset<T>(Data.name);

                assetBundle.Unload(false);

                if (data == null)
                {
                    LavenderLog.Error($"Error while loading LavenderAsset '{ModName}-{ID}': couldn't get '{nameof(T)}' named '{Data.name}' from '{Data.path}'!");
                    return null;
                }
                return data;
            }
            else if(AssetType == AssetType.Image)
            {
                Sprite? s = ImageLoader.LoadSprite(Data.path);
                if(s == null)
                {
                    LavenderLog.Error($"Error while loading LavenderAsset '{ModName}-{ID}': couldn't get Sprite at '{Data.path}'!");
                    return null;
                }
                return s;
            }
            else if(AssetType == AssetType.OBJ)
            {
                try
                {
                    if (!File.Exists(Data.path))
                    {
                        LavenderLog.Error($"Error while loading LavenderAsset '{ModName}-{ID}': couldn't get OBJ at '{Data.path}'!");
                        return null;
                    }

                    Mesh? mesh = FastObjImporter.Instance.ImportFile(Data.path);
                    mesh.name = Data.name + "_Mesh";

                    List<Material> materials = new List<Material>();
                    foreach (string texturePath in Data.objTexturePaths)
                    {
                        Material mat = new Material(Shader.Find("Standard"));
                        mat.mainTexture = ImageLoader.LoadImage(texturePath);

                        materials.Add(mat);
                    }

                    return new LavenderOBJAsset(mesh, materials);
                }
                catch(Exception e)
                {
                    LavenderLog.Error($"Error while loading LavenderAsset '{ModName}-{ID}': {e}");
                    return null;
                }
            }

            return null;
        }
    }

    public class AssetData
    {
        public string path { get; set; }
        public string? name { get; set; }
        public string[]? objTexturePaths { get; set; }
    }

    public struct LavenderOBJAsset
    {
        public Mesh mesh { get; set; }
        public List<Material> materials { get; set; }

        public LavenderOBJAsset(Mesh mesh, List<Material> materials)
        {
            this.mesh = mesh;
            this.materials = materials;
        }
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

            foreach(LavenderAsset asset in assets)
            {
                asset.ModName = ModName;
            }
        }
    }
}

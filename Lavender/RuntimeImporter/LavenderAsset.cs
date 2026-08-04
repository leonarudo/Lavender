using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lavender.RuntimeImporter
{
    public class LavenderAsset
    {
        [JsonIgnore]
        public string SrcFilePath = "";

        public string ID { get; set; }
        public LavenderAssetType Asset {  get; set; }

        public LavenderAsset(string id, LavenderAssetType asset)
        {
            ID = id;
            Asset = asset;
        }

        public LavenderAsset(string id, LavenderAssetType asset, string srcPath)
        {
            ID = id;
            Asset = asset;
            SrcFilePath = srcPath;
        }

        public string GetSrcDir()
        {
            return SrcFilePath.Substring(0, SrcFilePath.Length - System.IO.Path.GetFileName(SrcFilePath).Length);
        }
    }

    public abstract class LavenderAssetType
    {
        
    }

    internal class AssetTypeBinder : ISerializationBinder
    {
        private readonly Dictionary<string, Type> _types;

        public AssetTypeBinder()
        {
            _types = new Dictionary<string, Type>();
        }

        public AssetTypeBinder(params (Type, string)[] types)
        {
            _types = new Dictionary<string, Type>();

            foreach(var pair in types)
            {
                this.AddType(pair.Item1, pair.Item2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeName">The name used for the "$type" json field. Will be nameof() if set to String.Empty</param>
        /// <returns></returns>
        public bool AddType(Type type, string typeName = "")
        {
            if (typeName == "") typeName = nameof(type);

            if (!type.IsSubclassOf(typeof(LavenderAssetType)))
            {
                LavenderLog.Error($"[AssetTypeBinder] AddType: Type: '{typeName}' is not a subclass of LavenderAssetType!");
                return false;
            }

            if (!_types.ContainsKey(typeName))
            {
                _types.Add(typeName, type.GetType());
                return true;
            }

            return false;
        }

        public bool RemoveType(string typeName)
        {
            if(_types.ContainsKey(typeName))
            {
                _types.Remove(typeName);
                return true;
            }

            return false;
        }


        // Deserializing
        public Type BindToType(string? assemblyName, string typeName)
        {
            if(_types.TryGetValue(typeName, out var type))
                return type;

            throw new JsonSerializationException($"Type '{typeName}' is not allowed.");
        }

        // Serializing - We don't need this, but the interface wants it :/
        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            // assemblyName = _types.First(x => x.Value == serializedType).Value.Assembly.FullName;
            assemblyName = null;
            typeName = _types.First(x => x.Value == serializedType).Key;
        }
    }
}

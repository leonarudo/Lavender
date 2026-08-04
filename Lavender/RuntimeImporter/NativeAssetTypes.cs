using System;
using Newtonsoft.Json;

namespace Lavender.RuntimeImporter.AssetTypes
{
    /// <summary>
    /// LavenderAsset for loading assets from an Unity AssetBundle
    /// </summary>
    public class AssetBundleAsset : LavenderAssetType
    {
        /// <summary>
        /// Path to the AssetBundle file
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Name of the Asset/Object inside the AssetBundle
        /// </summary>
        public string objectName { get; set; }

        public AssetBundleAsset(string path, string objectName)
        {
            this.path = path;
            this.objectName = objectName;
        }
    }

    /// <summary>
    /// LavenderAsset for loading PNGs or JPGs
    /// </summary>
    public class ImageAsset : LavenderAssetType
    {
        public string path { get; set; }

        public ImageAsset(string path) { this.path = path; }
    }
}

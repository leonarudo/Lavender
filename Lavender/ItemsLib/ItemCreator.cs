using Lavender.RuntimeImporter;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

namespace Lavender.ItemsLib
{
    public class ItemCreator
    {
        public static GameObject ItemPrefabFromOBJ(string meshPath, string texturePath, string name)
        {
            GameObject obj = new GameObject(name);

            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

            try
            {
                if (!File.Exists(meshPath)) return null;
                if (!File.Exists(texturePath)) return null;

                Mesh mesh = FastObjImporter.Instance.ImportFile(meshPath);
                mesh.name = name + "_Mesh";
                meshFilter.mesh = mesh;

                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(texturePath));
                mat.mainTexture = tex;
                meshRenderer.material = mat;
            }
            catch
            {
                return null;
            }

            obj.AddComponent<MeshCollider>();
            obj.AddComponent<CollectibleItem>();
            obj.layer = 17;

            return obj;
        }
    }
}

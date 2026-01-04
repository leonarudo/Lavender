using Lavender.RuntimeImporter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static NPC.NPCPathfinding;

namespace Lavender.FurnitureLib
{
    public class FurnitureCreator
    {
        internal static GameObject FurnitureDBParent_Innit()
        {
            LavenderLog.Log("Furniture Database Innit...");

            GameObject gameObject = new GameObject("Lavender.FurnitureDatabase");
            gameObject.SetActive(false);

            return gameObject;
        }

        public static Furniture NewFurniture(string id, string title, Sprite image, string details, Furniture.Category category, int priceOC, int priceRM, GameObject furniturePrefab, GameObject furniturePreviewPrefab, Furniture.BuildingArea[] restrictedArea, List<Furniture.ReseourceItem> dismantleItems, FurniturePlaceType placeType, Furniture.DisplayStyle displayStyle = Furniture.DisplayStyle.Default, int displayRotationY = 0, bool trash = false)
        {
            Furniture furniture = ScriptableObject.CreateInstance<Furniture>();

            //Furniture Prefab Setup
            GameObject prefab = new GameObject(title);
            FurniturePlaceable furniturePlaceable = prefab.AddComponent<FurniturePlaceable>();
            furniturePlaceable.furniture = furniture;

            GameObject prefabRotate = new GameObject("rotate");
            prefabRotate.transform.parent = prefab.transform;

            if (Lavender.furniturePrefabHandlers.TryGetValue(title, out Lavender.FurniturePrefabHandler handler))
            {
                furniturePrefab = handler.Invoke(furniturePrefab);
            }
            else
            {
                LavenderLog.Log($"WARNING! No Prefab Handler found for Furniture '{title}'!");
            }

            furniturePrefab.transform.parent = prefabRotate.transform;

            prefab.layer = 12;

            //Furniture Preview Prefab Setup
            GameObject preview = new GameObject($"{title}-Preview");

            GameObject previewRotate = new GameObject("rotate");
            ObjectPreview objectPreview = previewRotate.AddComponent<ObjectPreview>();
            objectPreview.SetOrientationType((PlaceType)placeType);
            previewRotate.transform.parent = preview.transform;

            //furniturePreviewPrefab.AddComponent<InsideTrigger>();
            furniturePreviewPrefab.transform.parent = previewRotate.transform;

            preview.layer = 11;

            //Sort Prefabs
            if(Lavender.FurnitureDBParent == null)
            {
                Lavender.FurnitureDBParent = FurnitureDBParent_Innit();
            }

            if (Lavender.FurnitureDBParent != null)
            {
                prefab.transform.SetParent(Lavender.FurnitureDBParent.transform);
                preview.transform.SetParent(Lavender.FurnitureDBParent.transform);
            }

            //Furniture
            furniture.id = id;
            furniture.title = title;
            furniture.image = image;
            furniture.details = details;
            furniture.category = category;
            furniture.priceOC = priceOC;
            furniture.priceRM = priceRM;
            furniture.restrictedAreas = restrictedArea;
            furniture.dismantleItems = dismantleItems;
            furniture.prefab = prefab;
            furniture.previewPrefab = preview;
            furniture.displayStyle = displayStyle;
            furniture.displayRotationY = displayRotationY;

            return furniture;
        }

        /// <summary>
        /// Uses a FurnitureConfig to create a Furniture
        /// </summary>
        /// <param name="furnitureData">Deserialized ur-furniture-name.json</param>
        /// <returns></returns>
        public static Furniture? FurnitureConfigToFurniture(FurnitureConfig furnitureData)
        {
            if (furnitureData == null) return null;

            // Convert Lavender.FurnitureLib.FurnitureBuildingArea to OS.Furniture.BuildingArea
            Furniture.BuildingArea[] _resArea = new Furniture.BuildingArea[furnitureData.restrictedAreas.Length];
            for (int i = 0; i < furnitureData.restrictedAreas.Length; i++)
            {
                _resArea[i] = (Furniture.BuildingArea)furnitureData.restrictedAreas[i];
            }

            GameObject prefab;
            GameObject previewPrefab;
            Sprite image;

            LavenderAsset prefabAsset = Lavender.GetLavenderAsset(furnitureData.prefabName);
            if (prefabAsset != null)
            {
                if (prefabAsset.AssetType == AssetType.AssetBundle)
                    prefab = (GameObject)prefabAsset.GetAssetData<GameObject>();
                else if (prefabAsset.AssetType == AssetType.OBJ)
                {
                    var objAsset = prefabAsset.GetAssetData<GameObject>();
                    if (objAsset == null) return null;
                    LavenderOBJAsset asset = (LavenderOBJAsset)objAsset;

                    GameObject obj = new GameObject($"{furnitureData.title} - OBJ");

                    MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                    MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

                    meshFilter.mesh = asset.mesh;
                    meshRenderer.materials = asset.materials.ToArray();

                    prefab = GameObject.Instantiate(obj);
                    prefab.AddComponent<MeshCollider>();
                }
                else
                {
                    LavenderLog.Error($"Error while creating furniture '{furnitureData.title}': couldn't get Prefab! AssetID: {furnitureData.prefabName}");
                    return null;
                }
            }
            else
            {
                LavenderLog.Error($"Error while creating furniture '{furnitureData.title}': couldn't get Prefab! AssetID: {furnitureData.prefabName}");
                return null;
            }

            LavenderAsset previewPrefabAsset = Lavender.GetLavenderAsset(furnitureData.previewPrefabName);
            if (previewPrefabAsset != null)
            {
                if (previewPrefabAsset.AssetType == AssetType.AssetBundle)
                    previewPrefab = (GameObject)previewPrefabAsset.GetAssetData<GameObject>();
                else if (previewPrefabAsset.AssetType == AssetType.OBJ)
                {
                    var objAsset = previewPrefabAsset.GetAssetData<GameObject>();
                    if (objAsset == null) return null;
                    LavenderOBJAsset asset = (LavenderOBJAsset)objAsset;

                    GameObject obj = new GameObject($"{furnitureData.title} - OBJ");

                    MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                    MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

                    meshFilter.mesh = asset.mesh;
                    meshRenderer.materials = asset.materials.ToArray();

                    previewPrefab = GameObject.Instantiate(obj);
                    previewPrefab.AddComponent<MeshCollider>();

                    var boxCollider = previewPrefab.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    var rb = previewPrefab.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                }
                else
                {
                    LavenderLog.Error($"Error while creating furniture '{furnitureData.title}': couldn't get previewPrefab! AssetID: {furnitureData.previewPrefabName}");
                    return null;
                }
            }
            else
            {
                LavenderLog.Error($"Error while creating furniture '{furnitureData.title}': couldn't get previewPrefab! AssetID: {furnitureData.previewPrefabName}");
                return null;
            }

            LavenderAsset imageAsset = Lavender.GetLavenderAsset(furnitureData.imageName);
            var s = imageAsset != null ? imageAsset.GetAssetData<Sprite>() : null;
            if (s != null && s.GetType() == typeof(Sprite))
            {
                image = (Sprite)s;
            }
            else
            {
                LavenderLog.Error($"Error while creating furniture '{furnitureData.title}': couldn't get image! AssetID: {furnitureData.imageName}");
                return null;
            }

            // Creates the Furniture
            Furniture furniture = NewFurniture(
                furnitureData.id,
                furnitureData.title,
                image,
                furnitureData.details,
                (Furniture.Category)furnitureData.category,
                furnitureData.priceOC,
                furnitureData.priceRM,
                prefab,
                previewPrefab,
                _resArea,
                new List<Furniture.ReseourceItem>(),
                furnitureData.placeType,
                (Furniture.DisplayStyle)furnitureData.displayStyle,
                furnitureData.displayRotationY,
                furnitureData.trash
            );

            return furniture;
        }

        /// <summary>
        /// Creats a Furniture from the given path to the FurnitureConfig json and adds it to the 'createdFurniture' list
        /// </summary>
        /// <param name="json_path">The path to the json</param>
        /// <param name="overwrite_db">Overwrite the Database entry if a furniture with this name or id allready exists?</param>
        /// <returns></returns>
        public static Furniture? Create(string json_path, bool overwrite_db = false)
        {
            if(File.Exists(json_path))
            {
                try
                {
                    string rawFurnitureConfig = File.ReadAllText(json_path);

                    FurnitureConfig? furnitureConfig = JsonConvert.DeserializeObject<FurnitureConfig>(rawFurnitureConfig);
                    if (furnitureConfig != null)
                    {
                        Furniture? furniture = Lavender.FurnitureDatabase.Find((Furniture f) => f.title.Equals(furnitureConfig.title) || f.id.Equals(furnitureConfig.id));
                        if (furniture != null && overwrite_db == false)
                        {
                            return furniture;
                        }

                        Furniture f = FurnitureCreator.FurnitureConfigToFurniture(furnitureConfig);
                        f.addressableAssetPath = $"Lavender<#>{json_path}";

                        if(f != null && !Lavender.FurnitureDatabase.Contains(f)) Lavender.FurnitureDatabase.Add(f);

                        return f;
                    }
                    else
                    {
                        LavenderLog.Error($"FurnitureCreator.Create(): Error while deserializing FurnitureConfig at '{json_path}'!");
                    }
                }
                catch (Exception e)
                {
                    LavenderLog.Error($"FurnitureCreator.Create(): {e}");
                    return null;
                }
            }

            LavenderLog.Error($"FurnitureCreator.Create(): Couldn't find json_path '{json_path}'");
            return null;
        }

        /// <summary>
        /// Creates an BuildingSystem.FurnitureInfo for the FurnitureShopRestockHandler
        /// </summary>
        /// <param name="json_path">The path to the furniture json</param>
        /// <param name="amount">The amount of the furniture you want to add to the shop</param>
        /// <returns></returns>
        public static BuildingSystem.FurnitureInfo? CreateShopFurniture(string json_path, int amount = 1)
        {
            Furniture? f = Create(json_path);
            if (f == null) return null;

            TaskItem taskItem = (TaskItem)ScriptableObject.CreateInstance(typeof(TaskItem));
            taskItem.id = f.id;
            taskItem.itemName = f.title;
            taskItem.itemDetails = f.details;
            taskItem.image = f.image;
            taskItem.itemType = TaskItem.Type.Furnitures;

            return new BuildingSystem.FurnitureInfo(f, new BuildingSystem.FurnitureInfo.Meta(), taskItem, null, amount, null);
        }

        /// <summary>
        /// Creates an BuildingSystem.FurnitureInfo for the FurnitureShopRestockHandler
        /// </summary>
        /// <param name="furniture_titel">The name of the Furniture</param>
        /// <param name="amount">The amount of the furniture you want to add to the shop</param>
        /// <returns></returns>
        public static BuildingSystem.FurnitureInfo? CreateShopFurniture(int amount, string furniture_titel)
        {
            Furniture? f = Lavender.FetchFurnitureByTitle(furniture_titel);
            if (f == null) return null;

            TaskItem taskItem = (TaskItem)ScriptableObject.CreateInstance(typeof(TaskItem));
            taskItem.id = f.id;
            taskItem.itemName = f.title;
            taskItem.itemDetails = f.details;
            taskItem.image = f.image;
            taskItem.itemType = TaskItem.Type.Furnitures;

            return new BuildingSystem.FurnitureInfo(f, new BuildingSystem.FurnitureInfo.Meta(), taskItem, null, amount, null);
        }
    }
}

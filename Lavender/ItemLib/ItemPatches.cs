using HarmonyLib;
using Lavender.RuntimeImporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace Lavender.ItemLib
{
    public static class ItemPatches
    {
        [HarmonyPatch(typeof(ItemDatabase), nameof(ItemDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void ItemDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if (type == typeof(List<Item>))
            {
                List<Item>? vanilla_db = __result as List<Item>;

                if (vanilla_db != null)
                {
                    List<Item> db = new List<Item>();
                    db.AddRange(vanilla_db);

                    List<int> item_ids = new List<int>();
                    foreach(Item item in vanilla_db)
                    {
                        item_ids.Add(item.ID);
                    }

                    foreach(Item item in Lavender.customItemDatabase)
                    {
                        if(item_ids.Contains(item.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla Item id={item.ID} with mod Item!");
                            db.Remove(db.Find((Item i) => i.ID == item.ID));
                            db.Add(item);
                        }
                        else
                        {
                            db.Add(item);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customItemDatabase.Count} Mod Items");
                }
                else
                {
                    LavenderLog.Error("ItemDatabase.DeSerialize: This shouldn't happen!");
                }
            }

            return;
        }

        [HarmonyPatch(typeof(ItemOperations), nameof(ItemOperations.SetCollectibleItemValues))]
        [HarmonyPostfix]
        static void ItemOperations_SetCollectibleItemValues_Postfix(ItemStack item, GameObject gameObject, bool noExpirationTimer)
        {
            CollectibleItem component = gameObject.GetComponent<CollectibleItem>();
            if (component != null)
            {
                component.Item = item.itemReference;
            }

            return;
        }

        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.LoadSprite))]
        [HarmonyPrefix]
        static bool Item_ItemAppearance_LoadSprite_Prefix(Item.ItemAppearance __instance)
        {
            if(!string.IsNullOrEmpty(__instance.SpritePath))
            {
                if(__instance.SpritePath.StartsWith("#lv_"))
                {
                    LavenderAsset sprite = Lavender.GetLavenderAsset(__instance.SpritePath);
                    if (sprite != null)
                    {
                        var s = sprite.GetAssetData<Sprite>();
                        if(s != null && s.GetType() == typeof(Sprite))
                        {
                            __instance.Sprite = (Sprite)s;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.Loadprefab))]
        [HarmonyPrefix]
        static bool Item_ItemAppearance_Loadprefab_Prefix(Item.ItemAppearance __instance, Item item)
        {
            bool noSkip = true;

            // Prefab
            if (__instance.PrefabPath.StartsWith("#lv_"))
            {
                LavenderAsset prefabAsset = Lavender.GetLavenderAsset(__instance.PrefabPath);
                if (prefabAsset != null)
                {
                    if(prefabAsset.AssetType == AssetType.AssetBundle)
                    {
                        GameObject prefab = (GameObject)prefabAsset.GetAssetData<GameObject>();
                        if (prefab != null)
                        {
                            prefab.AddComponent<CollectibleItem>();
                            prefab.layer = 17;

                            __instance.Prefab = prefab;
                        }
                    }
                    else if(prefabAsset.AssetType == AssetType.OBJ)
                    {
                        var objAsset = prefabAsset.GetAssetData<GameObject>();
                        if (objAsset != null)
                        {
                            LavenderOBJAsset asset = (LavenderOBJAsset)objAsset;

                            GameObject obj = new GameObject(item.Title);

                            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                            meshRenderer.materials = asset.materials.ToArray();

                            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                            meshFilter.mesh = asset.mesh;

                            obj.AddComponent<MeshCollider>();
                            obj.AddComponent<CollectibleItem>();
                            obj.layer = 17;

                            __instance.Prefab = obj;
                        }
                    }
                }

                noSkip = false;
            }

            // Prefab Many
            if(!string.IsNullOrEmpty(__instance.PrefabPathMany))
            {
                if(__instance.PrefabPathMany.StartsWith("#lv_"))
                {
                    LavenderAsset prefabManyAsset = Lavender.GetLavenderAsset(__instance.PrefabPathMany);
                    if (prefabManyAsset != null)
                    {
                        if (prefabManyAsset.AssetType == AssetType.AssetBundle)
                        {
                            GameObject prefab = (GameObject)prefabManyAsset.GetAssetData<GameObject>();
                            if (prefab != null)
                            {
                                prefab.AddComponent<CollectibleItem>();
                                prefab.layer = 17;

                                __instance.PrefabMany = prefab;
                            }
                        }
                        else if (prefabManyAsset.AssetType == AssetType.OBJ)
                        {
                            var objAsset = prefabManyAsset.GetAssetData<GameObject>();
                            if (objAsset != null)
                            {
                                LavenderOBJAsset asset = (LavenderOBJAsset)objAsset;

                                GameObject obj = new GameObject(item.Title);

                                MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                                meshRenderer.materials = asset.materials.ToArray();

                                MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                                meshFilter.mesh = asset.mesh;

                                obj.AddComponent<MeshCollider>();
                                obj.AddComponent<CollectibleItem>();
                                obj.layer = 17;

                                __instance.PrefabMany = obj;
                            }
                        }
                    }

                    noSkip = false;
                }
            }

            return noSkip;
        }
    }
}

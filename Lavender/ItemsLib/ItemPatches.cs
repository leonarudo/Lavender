using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using FullSerializer;
using NPC;
using HarmonyLib;
using Lavender.RuntimeImporter;

namespace Lavender.ItemsLib
{
    internal static class ItemPatches
    {
        #region ItemDatabase.DeSerialize
        private static readonly fsSerializer JSON_serializer = new fsSerializer();

        [HarmonyPatch(typeof(ItemDatabase), nameof(ItemDatabase.DeSerialize))]
        [HarmonyPrefix]
        public static bool ItemDatabase_DeSerialize_Pefix(ItemDatabase __instance, object __result, Type type, string serializedState)
        {
            fsData data = fsJsonParser.Parse(serializedState);
            object result = null;
            JSON_serializer.TryDeserialize(data, type, ref result).AssertSuccessWithoutWarnings();

            if (type == typeof(List<Item>))
            {
                List<Item> database = result as List<Item>;

                foreach (var modItems in Lavender.itemConfigPaths)
                {
                    List<Item> modItemDB = null;

                    try
                    {
                        Debug.Log($"[OSML] Loading '{modItems.Key}'s Item Database...");

                        fsData mod_data = fsJsonParser.Parse(File.ReadAllText(modItems.Value));
                        object mod_result = null;
                        JSON_serializer.TryDeserialize(mod_data, type, ref mod_result).AssertSuccessWithoutWarnings();

                        modItemDB = mod_result as List<Item>;

                        // Adding 'OSML ModItem' to the Item category as an signature/identifier
                        foreach (Item i in modItemDB)
                        {
                            string[] updated_categories = new string[i.Categories.Length + 1];
                            for (int j = 0; j < i.Categories.Length; j++)
                            {
                                updated_categories[j] = i.Categories[j];
                            }
                            updated_categories[i.Categories.Length - 1] = "OSML ModItem";
                        }
                    }
                    catch (Exception ex)
                    {
                        LavenderLog.Error($"Error while loading '{modItems.Key}'s Item Database!\nException: {ex}");
                    }
                    finally
                    {
                        database.AddRange(modItemDB);

                        LavenderLog.Log($"[OSML] {modItemDB.Count} Items loaded!");
                    }
                }

                foreach (var dbHandler in Lavender.itemDatabaseHandlers)
                {
                    database = dbHandler.Value.Invoke(database);
                }

                __result = database;
            }

            // END

            __result = result;

            if (!BepinexPlugin.Settings.ItemDatabase_DeSerialize_Prefix_SkipOriginal.Value) return true;
            return false;
        }

        #endregion

        #region ItemOperations.SetCollectibleItemValues
        [HarmonyPatch(typeof(ItemOperations), nameof(ItemOperations.SetCollectibleItemValues))]
        [HarmonyPrefix]
        public static void ItemOperations_SetCollectibleItemValues_Pefix(string __instance, object __result, ItemStack item, GameObject gameObject)
        {
            CollectibleItem component = gameObject.GetComponent<CollectibleItem>();
            if (component != null)
            {

                component.Item = item.itemReference;

                component.StartExpirationTimer(null);
                component.amount = item.itemAmount;
                component.owner = new NPCReference(item.ownerId);
                component.meta = item.Meta;
                component.useSpawnInfo = false;
                component.GUID = "";
                component.SetItemMeta(null);
                FurniturePlaceable component2 = gameObject.GetComponent<FurniturePlaceable>();
                if (component2)
                {
                    ItemOperations.SetCollectibleItemFurniturePlaceable(component, component2);
                }
            }
            else if (gameObject.GetComponent<CollectibleCoin>() != null)
            {
                gameObject.GetComponent<CollectibleCoin>().StartExpirationTimer(null);
                gameObject.GetComponent<CollectibleCoin>().Amount = (float)item.itemAmount;
            }
        }
        #endregion

        #region Item.ItemAppearance
        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.LoadSprite))]
        [HarmonyPrefix]
        static void Item_ItemAppearance_LoadSprite_Prefix(Item.ItemAppearance __instance, bool __result)
        {
            if (!string.IsNullOrEmpty(__instance.SpritePath))
            {
                ImageLoader.LoadSprite(__instance.SpritePath);
            }

            if (!string.IsNullOrEmpty(__instance.SpritePath))
            {
                __instance.Sprite = Resources.Load<Sprite>(__instance.SpritePath);
            }
        }

        [HarmonyPatch(typeof(Item.ItemAppearance), nameof(Item.ItemAppearance.Loadprefab))]
        [HarmonyPrefix]
        static Item.ItemAppearance Item_ItemAppearance_Loadprefab_Prefix(Item.ItemAppearance __instance, bool __result, Item item)
        {

            if (!string.IsNullOrEmpty(__instance.PrefabPath))
            {
                if (__instance.PrefabPath.StartsWith("Lavender#"))
                {
                    string path = Path.Combine(Assembly.GetAssembly(typeof(Lavender)).Location.Substring(0, Assembly.GetAssembly(typeof(Lavender)).Location.Length - 13), __instance.PrefabPath.Substring(5));
                    path = path.Substring(0, path.Length - 3);

                    if (File.Exists(path + "png") && File.Exists(path + "obj"))
                    {
                        __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(path + "obj", path + "png", item.Title);
                        if (__instance.Prefab == null)
                        {
                            __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                        }
                    }
                    else if (File.Exists(path + "jpg") && File.Exists(path + "obj"))
                    {
                        __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(path + "obj", path + "jpg", item.Title);
                        if (__instance.Prefab == null)
                        {
                            __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                        }
                    }

                    return null;
                }
            }

            if (!string.IsNullOrEmpty(__instance.PrefabPathMany))
            {
                if (__instance.PrefabPathMany.StartsWith("Lavender#"))
                {
                    if (__instance.PrefabPathMany.Length == 5) __instance.PrefabMany = __instance.Prefab;

                    string pathMany = Path.Combine(Assembly.GetAssembly(typeof(Lavender)).Location.Substring(0, Assembly.GetAssembly(typeof(Lavender)).Location.Length - 13), __instance.PrefabPathMany.Substring(5));
                    pathMany = pathMany.Substring(0, pathMany.Length - 3);

                    if (File.Exists(pathMany + "png") && File.Exists(pathMany + "obj"))
                    {
                        __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(pathMany + "obj", pathMany + "png", item.Title);
                        if (__instance.Prefab == null)
                        {
                            __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                        }
                    }
                    else if (File.Exists(pathMany + "jpg") && File.Exists(pathMany + "obj"))
                    {
                        __instance.Prefab = ItemCreator.ItemPrefabFromOBJ(pathMany + "obj", pathMany + "jpg", item.Title);
                        if (__instance.Prefab == null)
                        {
                            __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                        }
                    }

                    return null;
                }
            }

            // END

            if (!string.IsNullOrEmpty(__instance.PrefabPath))
            {
                __instance.Prefab = Resources.Load<GameObject>(__instance.PrefabPath);
                if (__instance.Prefab == null)
                {
                    __instance.PrefabPath = Item.StripPath(__instance.PrefabPath);
                    __instance.Prefab = Resources.Load<GameObject>(__instance.PrefabPath);
                    if (__instance.Prefab == null)
                    {
                        __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                    }
                }
            }
            else
            {
                __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/" + item.Title);
                if (__instance.Prefab == null)
                {
                    __instance.Prefab = Resources.Load<GameObject>("Prefabs/Items/ERROR");
                }
            }
            if (!string.IsNullOrEmpty(__instance.PrefabPathMany))
            {
                __instance.PrefabMany = Resources.Load<GameObject>(__instance.PrefabPathMany);
                if (__instance.PrefabMany == null)
                {
                    __instance.PrefabPathMany = Item.StripPath(__instance.PrefabPathMany);
                    __instance.PrefabMany = Resources.Load<GameObject>(__instance.PrefabPathMany);
                    if (__instance.PrefabMany == null)
                    {
                        __instance.PrefabMany = __instance.Prefab;
                        return null;
                    }
                }
            }
            else
            {
                __instance.PrefabMany = __instance.Prefab;
            }

            return null;
        }

        #endregion
    }
}
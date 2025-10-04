using HarmonyLib;
using System;
using UnityEngine;

namespace Lavender.FurnitureLib
{
    public static class FurniturePatches
    {
        [HarmonyPatch(typeof(SavableScriptableObject), nameof(SavableScriptableObject.LoadFromPath), new Type[] { })]
        [HarmonyPrefix]
        static bool SavableScriptableObject_LoadFromPath_Prefix(SavableScriptableObject __instance, ref ScriptableObject __result)
        {
            if (!string.IsNullOrEmpty(__instance.addressableAssetPath))
            {
                if(__instance.addressableAssetPath.StartsWith("Lavender"))
                {
                    string sep = "<#>";
                    string path = __instance.addressableAssetPath.Substring(__instance.addressableAssetPath.IndexOf(sep) + 3);

                    try
                    {
                        Furniture? f = FurnitureCreator.Create(path);

                        if (f != null)
                        {
                            __result = f;
                        }
                    }
                    catch (Exception e)
                    {
                        LavenderLog.Error(e.ToString());
                        __result = null;
                    }

                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(SavableScriptableObject), nameof(SavableScriptableObject.LoadFromPath), new Type[] { })]
        [HarmonyPostfix]
        static void SavableScriptableObject_LoadFromPath_Postfix(SavableScriptableObject __instance, ref ScriptableObject __result)
        {
            if(__instance.GetType() == typeof(Furniture))
            {
                Furniture furniture = (Furniture) __instance;
                if(Lavender.ingameFurniturePrefabHandlers.TryGetValue(furniture.title, out Lavender.FurniturePrefabHandler handler))
                {
                    furniture.prefab = handler.Invoke(furniture.prefab);
                    __result = furniture;
                    LavenderLog.Log($"Patched {furniture.title} prefab!");
                }
            }
        }

        [HarmonyPatch(typeof(FurnitureShop), nameof(FurnitureShop.AddItem))]
        [HarmonyPrefix]
        static bool FurnitureShop_AddFurniture_Prefix(FurnitureShop __instance,ref bool __result, Furniture item, object meta, int amount)
        {
            BuildingSystem.FurnitureInfo furnitureInfo = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == item.title);
            if (furnitureInfo == null || furnitureInfo.furniture == null)
            {
                TaskItem taskItem = (TaskItem)ScriptableObject.CreateInstance(typeof(TaskItem));
                taskItem.itemName = item.title;
                taskItem.itemDetails = item.details;
                taskItem.image = item.image;
                taskItem.itemType = TaskItem.Type.Furnitures;
                __instance.availableFurnitures.Add(new BuildingSystem.FurnitureInfo(item, new BuildingSystem.FurnitureInfo.Meta(), taskItem, null, amount, null));
                __result = true;
            }
            furnitureInfo.amount += amount;
            __result = true;

            if (!BepinexPlugin.Settings.FurnitureShop_AddFurniture_Prefix_SkipOriginal.Value) return true;
            return false;
        }

        [HarmonyPatch(typeof(FurnitureShop), nameof(FurnitureShop.UpdateShopItems))]
        [HarmonyPrefix]
        static bool FurnitureShop_UpdateShopItems_Postfix(FurnitureShop __instance)
        {
            __instance.availableFurnitures.Clear();
            FurnitureShopName name = (__instance.title == "" ? FurnitureShopName.OneStopShop : 
                (__instance.title == "Möbelmann Furnitures" ? FurnitureShopName.MoebelmannFurnitures : 
                (__instance.title == "Jonasson's Shop" ? FurnitureShopName.SamuelJonasson : 
                (__instance.title == "OS Mining Services" ? FurnitureShopName.OSMiningServices : FurnitureShopName.None))));

            //OS Mining Services

            if (name != FurnitureShopName.None)
            {
                foreach (var pair in Lavender.furnitureShopRestockHandlers)
                {
                    __instance.availableFurnitures.AddRange(pair.Value.Invoke(name));
                }
            }

            foreach (FurnitureShopItemsList furnitureShopItemsList in __instance.itemsLists)
            {
                __instance.availableFurnitures.AddRange(furnitureShopItemsList.GetFurnitureInfos(__instance.availableFurnitures));
            }

            if (!BepinexPlugin.Settings.FurnitureShop_Restock_Prefix_SkipOriginal.Value) return true;
            return false;
        }

        [HarmonyPatch(typeof(BuildingSystem), nameof(BuildingSystem.AddFurniture),
            [typeof(Furniture), typeof(UnityEngine.GameObject), typeof(UnityEngine.GameObject), typeof(int), typeof(BuildingSystem.FurnitureInfo.Meta)],
            [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal])]
        [HarmonyPrefix]
        static bool BuildingSystem_AddFurniture_Prefix(BuildingSystem __instance, ref bool __result, Furniture furniture, GameObject gameObject, out GameObject savedGameObject, int amount)
        {
            BuildingSystem.FurnitureInfo furnitureInfo = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == furniture.title && f.gameObject == null);
            if (gameObject != null)
            {
                gameObject.transform.SetParent((__instance.inventoryLocation != null) ? __instance.inventoryLocation : __instance.gameObject.transform);
                gameObject.transform.localPosition = Vector3.zero;
                if (!__instance.HasSaveableContent(gameObject))
                {
                    UnityEngine.Object.Destroy(gameObject);
                    gameObject = null;
                }
            }
            savedGameObject = gameObject;
            BuildingSystem.FurnitureInfo info = __instance.availableFurnitures.Find((BuildingSystem.FurnitureInfo f) => f.furniture.title == furniture.title);
            TaskItem taskItem = __instance.AddTaskItem(furniture, new BuildingSystem.FurnitureInfo.Meta(), info, amount);
            if (furnitureInfo == null || furnitureInfo.furniture == null || gameObject != null)
            {
                __instance.availableFurnitures.Add(new BuildingSystem.FurnitureInfo(furniture, new BuildingSystem.FurnitureInfo.Meta(), taskItem, gameObject, amount, null));
                __instance.availableFurnitures.Sort((BuildingSystem.FurnitureInfo slot1, BuildingSystem.FurnitureInfo slot2) => slot1.furniture.name.CompareTo(slot2.furniture.name));
                __result = true;

                return !BepinexPlugin.Settings.BuildingSystem_AddFurniture_Prefix_SkipOriginal.Value;
            }
            furnitureInfo.amount += amount;
            __result = true;

            return !BepinexPlugin.Settings.BuildingSystem_AddFurniture_Prefix_SkipOriginal.Value;
        }
    }
}

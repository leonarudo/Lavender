using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Lavender.StorageLib
{
    public static class StoragePatches
    {
        #region Storage

        [HarmonyPatch(typeof(Storage), nameof(Storage.EnterExternally))]
        [HarmonyPostfix]
        static void Storage_EnterExternally_Postfix(Storage __instance, bool forceOpen)
        {
            // If forceOpen is ture then the storage was opened using a lockpick

            FurniturePlaceable? furniture = __instance.gameObject.GetComponent<FurniturePlaceable>();
            string storagFurnitureTitle = furniture.furniture.title;

            if(Lavender.OnStorageEnterCallbacks.TryGetValue(storagFurnitureTitle, out Lavender.OnStorageEnter callback))
            {
                callback.Invoke(__instance.gameObject, forceOpen);
            }
        }

        [HarmonyPatch(typeof(Storage), nameof(Storage.ExitExternally))]
        [HarmonyPostfix]
        static void Storage_ExitExternally_Postfix(Storage __instance)
        {
            FurniturePlaceable? furniture = __instance.gameObject.GetComponent<FurniturePlaceable>();
            string storagFurnitureTitle = furniture.furniture.title;

            if (Lavender.OnStorageExitCallbacks.TryGetValue(storagFurnitureTitle, out Lavender.OnStorageExit callback))
            {
                callback.Invoke(__instance.gameObject);
            }
        }

        #endregion

        #region StorageCategory & StorageSpawnCategory

        [HarmonyPatch(typeof(StorageCategoryDatabase), nameof(StorageCategoryDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void StorageCategoryDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if(type == typeof(StorageCategory))
            {
                List<StorageCategory>? vanilla_db = __result as List<StorageCategory>;

                if(vanilla_db != null)
                {
                    List<StorageCategory> db = new List<StorageCategory>();
                    db.AddRange(vanilla_db);

                    List<int> category_ids = new List<int>();
                    foreach(StorageCategory category in vanilla_db)
                    {
                        category_ids.Add(category.ID);
                    }

                    foreach(StorageCategory category in Lavender.customStorageCategoryDatabase)
                    {
                        if(category_ids.Contains(category.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla StorageCategory id={category.ID} with mod StorageCategory!");
                            db.Remove(db.Find((StorageCategory sc) => sc.ID == category.ID));
                            db.Add(category);
                        }
                        else
                        {
                            db.Add(category);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customStorageCategoryDatabase.Count} Mod StorageCategories");
                }
                else
                {
                    LavenderLog.Error("StorageCategory.DeSerialize: This shouldn't happen!");
                }
            }
        }

        [HarmonyPatch(typeof(StorageSpawnCategoryDatabase), nameof(StorageSpawnCategoryDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void StorageSpawnCategoryDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            if (type == typeof(StorageSpawnCategory))
            {
                List<StorageSpawnCategory>? vanilla_db = __result as List<StorageSpawnCategory>;

                if (vanilla_db != null)
                {
                    List<StorageSpawnCategory> db = new List<StorageSpawnCategory>();
                    db.AddRange(vanilla_db);

                    List<int> category_ids = new List<int>();
                    foreach (StorageSpawnCategory category in vanilla_db)
                    {
                        category_ids.Add(category.ID);
                    }

                    foreach (StorageSpawnCategory category in Lavender.customStorageSpawnCategoryDatabase)
                    {
                        if (category_ids.Contains(category.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla StorageSpawnCategory id={category.ID} with mod StorageSpawnCategory!");
                            db.Remove(db.Find((StorageSpawnCategory sc) => sc.ID == category.ID));
                            db.Add(category);
                        }
                        else
                        {
                            db.Add(category);
                        }
                    }

                    __result = db;

                    LavenderLog.Log($"Successfully added {Lavender.customStorageSpawnCategoryDatabase.Count} Mod StorageSpawnCategories");
                }
                else
                {
                    LavenderLog.Error("StorageSpawnCategory.DeSerialize: This shouldn't happen!");
                }
            }
        }

        [HarmonyPatch(typeof(StorageCategoryDatabase), nameof(StorageCategoryDatabase.FetchCategoryByName))]
        [HarmonyPostfix]
        static void StorageCategoryDatabase_FetchCategoryByName_Postfix(StorageCategoryDatabase __instance, ref StorageCategory __result, string name)
        {
            StorageCategory? category = Lavender.customStorageCategoryDatabase.Find((StorageCategory s) => s.Name == name);

            if (category != null && __result != null) 
            {
                __result = category;

                LavenderLog.Log($"Replacing vanilla StorageCategory name={category.Name} with mod StorageCategory!");
            }
            else if (category != null) 
            { 
                __result = category; 
            }
        }

        [HarmonyPatch(typeof(StorageSpawnCategoryDatabase), nameof(StorageSpawnCategoryDatabase.FetchStorageSpawnCategory))]
        [HarmonyPostfix]
        static void StorageSpawnCategoryDatabase_FetchCategoryByName_Postfix(StorageSpawnCategoryDatabase __instance, ref StorageSpawnCategory __result, string category)
        {
            StorageSpawnCategory? r = Lavender.customStorageSpawnCategoryDatabase.Find((StorageSpawnCategory s) => s.Name == category);

            if (r != null && __result != null)
            {
                __result = r;

                LavenderLog.Log($"Replacing vanilla StorageSpawnCategory name={r.Name} with mod StorageSpawnCategory!");
            }
            else if (r != null)
            {
                __result = r;
            }
        }

        #endregion
    }
}

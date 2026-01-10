using FullSerializer;
using HarmonyLib;
using Lavender.CommandLib;
using Lavender.DialogueLib;
using Lavender.FurnitureLib;
using Lavender.ItemLib;
using Lavender.RecipeLib;
using Lavender.RuntimeImporter;
using Lavender.StorageLib;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Lavender
{
    public class Lavender
    {
        public static Lavender instance;

        public Harmony harmony;

        public bool isInitialized;

        /// <summary>
        /// The build index of the last scene during the "SceneManager.sceneLoaded" callback
        /// </summary>
        public int lastLoadedScene = 0;

        /// <summary>
        /// You want to execute your mod logic only when LoadingDone = true to make sure that all game logic is already initialized!
        /// </summary>
        public bool LoadingDone;

        public Lavender()
        {
            if (instance == null) instance = this;
            else return;

            harmony = new Harmony(LCMPluginInfo.PLUGIN_GUID);

            try
            {
                harmony.PatchAll(typeof(FurniturePatches));
                harmony.PatchAll(typeof(ItemPatches));
                harmony.PatchAll(typeof(RecipePatches));
                harmony.PatchAll(typeof(CommandManagerPatches));
                harmony.PatchAll(typeof(StoragePatches));
                harmony.PatchAll(typeof(DialogueInteractableTalkPatches));
            }
            catch (Exception e)
            {
                LavenderLog.Error("Exception while applying Lavender patches:");
                LavenderLog.Error(e.ToString());
            }
        }

        #region RuntimeImporter

        public static List<LavenderAssetBundle> lavenderAssets;

        public static void AddLavenderAssets(string json_path, string ModName)
        {
            string path = json_path.Substring(0, json_path.Length - Path.GetFileName(json_path).Length);

            LavenderAssetBundle newBundle = new LavenderAssetBundle(ModName, json_path);
            if(newBundle != null)
            {
                foreach(LavenderAsset asset in newBundle.assets)
                {
                    asset.Data.path = path + asset.Data.path;
                }

                lavenderAssets.Add(newBundle);
            }
        }

        // assetID: <ModName>-<id> e.g. Lavender-100
        public static LavenderAsset? GetLavenderAsset(string assetID)
        {
            string[] strings = assetID.Replace("#lv_", "").Split('-');

            if(strings.Length < 2)
            {
                LavenderLog.Error($"Wrong assetId format! assetID: '{assetID}', correct format: '<ModName>-<id>' e.g. 'Lavender-100'");
                return null;
            }

            int id = int.Parse(strings[1]);

            List<LavenderAsset> assets = GetLavenderAssetsFromMod(strings[0]);

            return assets.Find(x => x.ID == id);
        }

        public static List<LavenderAsset> GetLavenderAssetsFromMod(string ModName)
        {
            List<LavenderAsset> result = new List<LavenderAsset>();

            foreach (LavenderAssetBundle bundle in lavenderAssets)
            {
                if(bundle.ModName == ModName)
                {
                    result.AddRange(bundle.assets);
                }
            }

            return result;
        }

        #endregion

        #region FurnitureLib

        public delegate GameObject FurniturePrefabHandler(GameObject prefab);
        public delegate List<BuildingSystem.FurnitureInfo> FurnitureShopRestockHandler(FurnitureShopName name);

        public static Dictionary<string, FurniturePrefabHandler> furniturePrefabHandlers;
        public static Dictionary<string, FurniturePrefabHandler> ingameFurniturePrefabHandlers;
        public static Dictionary<string, FurnitureShopRestockHandler> furnitureShopRestockHandlers;

        public static List<Furniture> FurnitureDatabase;

        public static GameObject? FurnitureDBParent;

        public static Furniture? FetchFurnitureByTitle(string title)
        {
            return FurnitureDatabase.Find((Furniture f) => f.title.Equals(title));
        }

        public static Furniture? FetchFurnitureByID(string ID)
        {
            return FurnitureDatabase.Find((Furniture f) => f.id.Equals(ID));
        }

        /// <summary>
        /// Gets all FurniturePrefabHandler methods defined in the given Type: type and registers them for the Handler callback
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AddFurniturePrefabHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(FurniturePrefabHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                FurniturePrefabHandlerAttribute attribute = method.GetCustomAttribute<FurniturePrefabHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but furniture handler methods must be static");
                    return false;
                }

                Delegate furnitureHandler = Delegate.CreateDelegate(typeof(FurniturePrefabHandler), method, false);
                if (furnitureHandler != null)
                {
                    if (!attribute.IsIngameFurniture)
                    {
                        if (furniturePrefabHandlers.ContainsKey(attribute.FurnitureTitle))
                        {
                            LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per furniture!");
                            return false;
                        }
                        else
                        {
                            furniturePrefabHandlers.Add(attribute.FurnitureTitle, (FurniturePrefabHandler)furnitureHandler);
                        }
                    }
                    else
                    {
                        if (ingameFurniturePrefabHandlers.ContainsKey(attribute.FurnitureTitle))
                        {
                            LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per furniture!");
                            return false;
                        }
                        else
                        {
                            ingameFurniturePrefabHandlers.Add(attribute.FurnitureTitle, (FurniturePrefabHandler)furnitureHandler);
                        }
                    }
                }
                else
                {
                    LavenderLog.Error($"InvalidHandlerSignatureException: '{method.DeclaringType}.{method.Name}' doesn't match any acceptable furniture handler method signatures! Furniture handler methods should have a 'Furniture' parameter and should return 'Furniture'.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all FurnitureShopRestockHandler methods defined in the given Type: type and registers them for the Handler callback
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AddFurnitureShopRestockHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(FurnitureShopRestockHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                FurnitureShopRestockHandlerAttribute attribute = method.GetCustomAttribute<FurnitureShopRestockHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but furniture shop restock handler methods must be static");
                    return false;
                }

                Delegate furnitureHandler = Delegate.CreateDelegate(typeof(FurnitureShopRestockHandler), method, false);
                if (furnitureHandler != null)
                {
                    if (furnitureShopRestockHandlers.ContainsKey(attribute.HandlerUID))
                    {
                        LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per UID!");
                        return false;
                    }
                    else
                    {
                        furnitureShopRestockHandlers.Add(attribute.HandlerUID, (FurnitureShopRestockHandler)furnitureHandler);
                    }
                }
                else
                {
                    LavenderLog.Error($"InvalidHandlerSignatureException: '{method.DeclaringType}.{method.Name}' doesn't match any acceptable furniture shop restock handler method signatures! Furniture handler methods should have a 'FurnitureShopName' parameter and should return 'List<BuildingSystem.FurnitureInfo>'.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ItemLib

        private static readonly fsSerializer JSON_serializer = new fsSerializer();

        public static List<Item> customItemDatabase;

        public static void AddCustomItem(Item item, string mod_name)
        {
            item.Categories.AddToArray("Lavender ModItem");
            item.Categories.AddToArray(mod_name);

            customItemDatabase.Add(item);
        }

        public static void AddCustomItemsFromJson(string jsonPath, string mod_name)
        {
            if (!File.Exists(jsonPath)) { LavenderLog.Error($"AddCustomItemsFromJson(): File at path '{jsonPath}' doesn't exists!"); return; }

            try
            {
                fsData data = fsJsonParser.Parse(File.ReadAllText(jsonPath));
                object result = null;
                ItemDatabase.JSON_serializer.TryDeserialize(data, typeof(List<Item>), ref result).AssertSuccessWithoutWarnings();

                List<Item> Items = result as List<Item>;

                foreach (Item i in Items)
                {
                    AddCustomItem(i, mod_name);
                }
            }
            catch (Exception e)
            {
                LavenderLog.Error($"Error while loading '{mod_name}'s Item Database!\nException: {e}");
            }
        }

        #endregion

        #region RecipeLib
        public static List<ModifierInfo> modifierInfos;

        /// <summary>
        /// ManuName, modifier_ID
        /// </summary>
        public static Dictionary<string, int> appliedCustomCraftingBaseModifiers;

        public static List<Recipe> customRecipeDatabase;

        public static void AddCustomRecipe(Recipe recipe, string mod_name)
        {
            customRecipeDatabase.Add(recipe);
        }

        public static void AddCustomRecipesFromJson(string jsonPath, string mod_name)
        {
            if (!File.Exists(jsonPath)) { LavenderLog.Error($"AddCustomRecipesFromJson(): File at path '{jsonPath}' doesn't exists!"); return; }

            try
            {
                fsData data = fsJsonParser.Parse(File.ReadAllText(jsonPath));
                object result = null;
                RecipeDatabase.JSON_serializer.TryDeserialize(data, typeof(List<Recipe>), ref result).AssertSuccessWithoutWarnings();

                List<Recipe> Recipes = result as List<Recipe>;

                foreach (Recipe r in Recipes)
                {
                    AddCustomRecipe(r, mod_name);
                }
            }
            catch (Exception e)
            {
                LavenderLog.Error($"Error while loading '{mod_name}'s Recipe Database!\nException: {e}");
            }
        }

        public static void AddModifierToCraftingBase(string manu_name, int modifier_id, bool skip_warnings = false)
        {
            if(Enum.IsDefined(typeof(RecipeCondition), modifier_id) && !skip_warnings)
            {
                LavenderLog.Error($"WARNING: RecipeCondition with id {modifier_id} is a base game modifier!");
            }

            appliedCustomCraftingBaseModifiers.Add(manu_name, modifier_id);
        }

        public static void AddModifierInfo(ModifierInfo info)
        {
            if (modifierInfos.Find((ModifierInfo i) => i.id == info.id) != null)
            {
                LavenderLog.Error($"Couldn't add ModifierInfo id={info.id} because another ModifierInfo allready uses this id!");
                return;
            }

            modifierInfos.Add(info);
        }
        #endregion

        #region StorageLib

        public delegate void OnStorageEnter(GameObject gameObject, bool forceOpen);
        public delegate void OnStorageExit(GameObject gameObject);

        public static Dictionary<string,  OnStorageEnter> OnStorageEnterCallbacks;
        public static Dictionary<string , OnStorageExit> OnStorageExitCallbacks;

        public static List<StorageCategory> customStorageCategoryDatabase;
        public static List<StorageSpawnCategory> customStorageSpawnCategoryDatabase;

        public static void AddCustomStorageCategory(StorageCategory category)
        {
            customStorageCategoryDatabase.Add(category);
        }

        public static void AddCustomStorageSpawnCategory(StorageSpawnCategory category)
        {
            customStorageSpawnCategoryDatabase.Add(category);
        }

        public static void AddCustomStorageCategoryFromJson(string jsonPath, string mod_name)
        {
            if (!File.Exists(jsonPath)) { LavenderLog.Error($"AddCustomStorageCategoryFromJson(): File at path '{jsonPath}' doesn't exists!"); return; }

            try
            {
                fsData data = fsJsonParser.Parse(File.ReadAllText(jsonPath));
                object? result = null;
                StorageCategoryDatabase.JSON_serializer.TryDeserialize(data, typeof(List<StorageCategory>), ref result).AssertSuccessWithoutWarnings();

                List<StorageCategory> categories = result as List<StorageCategory>;

                foreach(StorageCategory category in categories)
                {
                    AddCustomStorageCategory(category);
                }
            }
            catch (Exception e)
            {
                LavenderLog.Error($"Error while loading '{mod_name}'s StorageCategory Database!\nException: {e}");
            }
        }

        public static void AddCustomStorageSpawnCategoryFromJson(string jsonPath, string mod_name)
        {
            if (!File.Exists(jsonPath)) { LavenderLog.Error($"AddCustomStorageSpawnCategoryFromJson(): File at path '{jsonPath}' doesn't exists!"); return; }

            try
            {
                fsData data = fsJsonParser.Parse(File.ReadAllText(jsonPath));
                object? result = null;
                StorageSpawnCategoryDatabase.JSON_serializer.TryDeserialize(data, typeof(List<StorageSpawnCategory>), ref result).AssertSuccessWithoutWarnings();

                List<StorageSpawnCategory> categories = result as List<StorageSpawnCategory>;

                foreach (StorageSpawnCategory category in categories)
                {
                    AddCustomStorageSpawnCategory(category);
                }
            }
            catch (Exception e)
            {
                LavenderLog.Error($"Error while loading '{mod_name}'s StorageSpawnCategory Database!\nException: {e}");
            }
        }

        public static void AddOnStorageEnterCallback(string storage_name, OnStorageEnter callback)
        {
            if(!OnStorageEnterCallbacks.ContainsKey(storage_name))
            {
                OnStorageEnterCallbacks.Add(storage_name, callback);
            }
            else
            {
                LavenderLog.Error($"DuplicateHandlerException: '{storage_name}' Only one OnStorageEnter callback is allowed per storage!");
            }
        }

        public static void AddOnStorageExitCallback(string storage_name, OnStorageExit callback)
        {
            if (!OnStorageExitCallbacks.ContainsKey(storage_name))
            {
                OnStorageExitCallbacks.Add(storage_name, callback);
            }
            else
            {
                LavenderLog.Error($"DuplicateHandlerException: '{storage_name}' Only one OnStorageExit callback is allowed per storage!");
            }
        }

        #endregion

        #region DialogueLib

        /// <summary>
        /// Register a conversation patcher.
        /// Should only be called once per conversation patch (probably during your mod startup).
        /// Safe to be called at any time.
        /// Each registered patch should be a unique object instance, even if you have 1 patcher that handles multiple conversations in the same class.
        /// You can keep a reference to the patcher object after registration, but it is not required.
        /// </summary>
        /// <param name="patcher">Instantiated ConversationPatcher object that will be used to patch its conversation</param>
        public static void AddConversationPatcher(ConversationPatcher patcher)
        {
            ConversationPatchesManager.Instance.AddConversationPatcher(patcher);
        }

        /// <summary>
        /// Fetch all patcher objects that patch conversationName.
        /// WARNING: Results may include patcher objects from other mods.
        /// If you need to keep track of your patcher, it is strongly recommended that you do this with a List or variables in your mod instead of using this function.
        /// This function is provided to be able to check for the presence of other patchers potentially affecting the same conversation - ie conflict detection.
        /// </summary>
        /// <param name="conversationName">The name of the conversation being patched.  For example: "Tenement/Outside/Tatyana Gopnikova"</param>
        /// <returns>0 or more unique ConversationPatcher objects</returns>
        public IEnumerable<ConversationPatcher> GetPatchersForConversation(string conversationName)
        {
            return ConversationPatchesManager.Instance.GetPatchersForConversation(conversationName);
        }

        #endregion
    }
}

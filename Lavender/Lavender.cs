using Lavender.FurnitureLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Lavender.ItemsLib;
using static Lavender.ItemsLib.ItemHandlerAttributes;

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

            harmony.PatchAll(typeof(FurniturePatches));
            harmony.PatchAll(typeof(ItemPatches));
        }

        #region FurnitureLib

        public delegate Furniture FurnitureHandler(Furniture furniture);
        public delegate List<BuildingSystem.FurnitureInfo> FurnitureShopRestockHandler(FurnitureShopName name);

        public static Dictionary<string, FurnitureHandler> furnitureHandlers;
        public static Dictionary<string, FurnitureShopRestockHandler> furnitureShopRestockHandlers;

        /// <summary>
        /// Gets all FurnitureHandler methods defined in the given Type: type and registers them for the Handler callback
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AddFurnitureHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(FurnitureHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                FurnitureHandlerAttribute attribute = method.GetCustomAttribute<FurnitureHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but furniture handler methods must be static");
                    return false;
                }

                Delegate furnitureHandler = Delegate.CreateDelegate(typeof(FurnitureHandler), method, false);
                if (furnitureHandler != null)
                {
                    if (furnitureHandlers.ContainsKey(attribute.FurnitureTitle))
                    {
                        LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per furniture!");
                        return false;
                    }
                    else
                    {
                        furnitureHandlers.Add(attribute.FurnitureTitle, (FurnitureHandler)furnitureHandler);
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

        public delegate List<Item> ItemDatabaseHandler(List<Item> db);
        public static Dictionary<string, ItemDatabaseHandler> itemDatabaseHandlers;
        public static Dictionary<string, string> itemConfigPaths;

        public static bool AddItemDatabaseHandlers(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(ItemDatabaseHandlerAttribute), false).Length > 0).ToArray();

            foreach (MethodInfo method in methods)
            {
                ItemDatabaseHandlerAttribute attribute = method.GetCustomAttribute<ItemDatabaseHandlerAttribute>();

                if (!method.IsStatic)
                {
                    LavenderLog.Error($"'{method.DeclaringType.Name}.{method.Name}' is an instance method, but Item Database handler methods must be static");
                    return false;
                }

                Delegate itemDBHandler = Delegate.CreateDelegate(typeof(ItemDatabaseHandler), method, false);
                if (itemDBHandler != null)
                {
                    if (itemDatabaseHandlers.ContainsKey(attribute.HandlerUID))
                    {
                        LavenderLog.Error($"DuplicateHandlerException: '{method.DeclaringType}.{method.Name}' Only one handler method is allowed per UID!");
                        return false;
                    }
                    else
                    {
                        itemDatabaseHandlers.Add(attribute.HandlerUID, (ItemDatabaseHandler)itemDBHandler);
                    }
                }
                else
                {
                    LavenderLog.Error($"InvalidHandlerSignatureException: '{method.DeclaringType}.{method.Name}' doesn't match any acceptable Item Database handler method signatures! Furniture handler methods should have a 'List<Item>' parameter and should return 'List<Item>'.");
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}

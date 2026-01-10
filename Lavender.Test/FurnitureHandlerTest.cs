using Lavender.FurnitureLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using Lavender.StorageLib;

namespace Lavender.Test
{
    public class FurnitureHandlerTest
    {
        [FurniturePrefabHandlerAttribute("OSML Box")]
        public static GameObject osmlBoxHandler(GameObject gameObject)
        {
            Debug.Log($"[OSML BOX] Hello world from {gameObject.name}!");

            StorageCreator.AddSimpleStorage(gameObject, "Modded Storage", "View", "StorageTest");

            return gameObject;
        }

        [FurniturePrefabHandlerAttribute("Pallet", true)]
        public static GameObject inagmePrefabPatchTest(GameObject gameObject)
        {
            return gameObject;
        }

        [FurnitureShopRestockHandlerAttribute("LavenderTest")]
        public static List<BuildingSystem.FurnitureInfo> furShopRestockHandler(FurnitureShopName name)
        {
            List<BuildingSystem.FurnitureInfo> restock = new List<BuildingSystem.FurnitureInfo>();

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "osml_box.json");

            BuildingSystem.FurnitureInfo? info = FurnitureCreator.CreateShopFurniture(path, 20);
            if(info != null) restock.Add(info);

            return restock;
        }
    }
}

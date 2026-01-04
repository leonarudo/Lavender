using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Lavender.FurnitureLib
{
    public class FurnitureConfig
    {
        /// <summary>
        /// Furniture ID
        /// </summary>
        public string id;
        /// <summary>
        /// The name of the furniture
        /// </summary>
        public string title;
        /// <summary>
        /// A short description of the furniture
        /// </summary>
        public string details;

        /// <summary>
        /// OC price
        /// </summary>
        public int priceOC;
        /// <summary>
        /// RM price
        /// </summary>
        public int priceRM;

        /// <summary>
        /// ?
        /// </summary>
        public bool trash;

        /// <summary>
        /// LavenderAsset-ID 'ModName-id' e.g. Lavender-100
        /// </summary>
        public string imageName;
        /// <summary>
        /// LavenderAsset-ID 'ModName-id' e.g. Lavender-100
        /// </summary>
        public string prefabName;
        /// <summary>
        /// LavenderAsset-ID 'ModName-id' e.g. Lavender-100
        /// </summary>
        public string previewPrefabName;

        /*
         * To do:
         * 

        /// <summary>
        /// LavenderAsset-ID 'ModName-id' e.g. Lavender-100
        /// </summary>
        public string customDisplayPrefab; */

        /// <summary>
        /// The category of the Furniture
        /// </summary>
        public FurnitureCategory category;
        /// <summary>
        /// Restricted areas to build the furniture -> Allowed to put everywhere if empty
        /// </summary>
        public FurnitureBuildingArea[] restrictedAreas;
        /// <summary>
        /// Should it be positioned like a normal furniture or like a painting or ceiling object?
        /// </summary>
        public FurnitureDisplayStyle displayStyle;
        /// <summary>
        /// Restricted places to build (wall, floor, etc)
        /// </summary>
        public FurniturePlaceType placeType;
        /// <summary>
        /// Display rotation on the Y-Achses
        /// </summary>
        public int displayRotationY = 0;

        // To do:

        // public List<Furniture.ReseourceItem> dismantleItems;
        // public Furniture.Skin[] skins;
        // public Furniture.AlternativePrefab[] alternativePrefabs;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FurnitureCategory
    {
        All,
        Chairs,
        Tables,
        Electronics,
        Paintings,
        Lights,
        Rugs,
        Items,
        Machines,
        Storage,
        Clutter,
        Beds,
        Sofas,
        Decoration,
        Plants,
        Shelves,
        Manufacturing,
        Growing,
        Letters,
        Bathroom,
        Signs,
        Magazines,
        Posters,
        Curtains,
        Flags,
        LicensePlates,
        Kitchen,
        Tank,
        None
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FurnitureDisplayStyle
    {
        Default,
        Painting,
        Ceiling
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FurnitureBuildingArea
    {
        None,
        Stairs,
        PlayerApartment,
        Workshop,
        Outside,
        Greenhouse,
        Store,
        Communal,
        Public
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FurniturePlaceType
    {
        all,
        floor,
        wall,
        ceiling,
        floorAndWall
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Lavender.FurnitureLib
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FurniturePrefabHandlerAttribute : System.Attribute
    {
        public readonly string FurnitureTitle;
        public readonly bool IsIngameFurniture;

        public FurniturePrefabHandlerAttribute(string furnitureTitle, bool isIngameFurniture = false)
        {
            FurnitureTitle = furnitureTitle;
            IsIngameFurniture = isIngameFurniture;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FurnitureShopRestockHandlerAttribute : System.Attribute
    {
        public readonly string HandlerUID;

        public FurnitureShopRestockHandlerAttribute(string handlerUID)
        {
            HandlerUID = handlerUID;
        }
    }

    public enum FurnitureShopName
    {
        None,
        [Description("One Stop Shop")]
        OneStopShop,
        [Description("Möbelmann Furnitures")]
        MoebelmannFurnitures,
        [Description("Jonasson's Shop")]
        SamuelJonasson,
        [Description("OS Mining Services")]
        OSMiningServices
    }
}

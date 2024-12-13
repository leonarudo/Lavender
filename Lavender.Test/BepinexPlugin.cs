using BepInEx;
using BepInEx.Logging;
using Lavender.FurnitureLib;
using System.Reflection;
using System.IO;

namespace Lavender.Test
{
    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class BepinexPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log = null!;

        void Awake()
        {
            Log = Logger;

            SaveController.LoadingDone += onLoadingDone;

            Lavender.AddFurnitureHandlers(typeof(FurnitureHandlerTest));
            Lavender.AddFurnitureShopRestockHandlers(typeof(FurnitureHandlerTest));

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

            //Log.LogInfo($"Item Path {Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17)}");
            //Lavender.itemConfigPaths.Add("LavenderTest", Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "Items.json"));

            string groundGlass2String = "{'ID': 35171,'Title': 'Ground Glass','Categories': ['Hardware',],'Value': 26,'Description': 'It used to be a rock','Stackable': 8,'Appearance': {'Material': 'Cement','SpritePath': 'Sprites/Items/Sand_spr','ColorValue': 'RGBA(0.000, 0.000, 0.000, 0.000)','PrefabPath': 'Prefabs/Items/Sand','PrefabPathMany': 'Prefabs/Items/Sand'},'Actions': [],'Attachments': {'Name': '','Attachment1': 'none','Attachment2': 'none'},'Meta': []}";
            Lavender.AddCustomItemFromString(groundGlass2String);
        }

        private void onLoadingDone()
        {
            // !Only add Furniture after Loading is done
            string path = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "osml_box.json");

            Furniture? f = FurnitureCreator.Create(path);
            if (f != null) f.GiveItem();
        }
    }
}

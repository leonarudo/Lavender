using BepInEx;
using BepInEx.Logging;
using Lavender.FurnitureLib;
using Lavender.CommandLib;
using System.Reflection;
using System.IO;
using System;

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

            string lvAssetPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "TestLvAssets.json");
            Lavender.AddLavenderAssets("LavenderTest", lvAssetPath);

            Lavender.AddFurniturePrefabHandlers(typeof(FurnitureHandlerTest));
            Lavender.AddFurnitureShopRestockHandlers(typeof(FurnitureHandlerTest));

            // Item test
            string itemsPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "Items.json");
            Lavender.AddCustomItemsFromJson(itemsPath, LCMPluginInfo.PLUGIN_NAME);

            string recipesPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "Recipes.json");
            Lavender.AddCustomRecipesFromJson(recipesPath, LCMPluginInfo.PLUGIN_NAME);

            string imagePath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "item_test_img.png");
            Lavender.AddModifierInfo(new RecipeLib.ModifierInfo(101, "Nextej Modifier Test", "Test, test, 123..", RuntimeImporter.ImageLoader.LoadSprite(imagePath)));
            Lavender.AddModifierToCraftingBase("Brick Furnace", 101);

            //Console test
            CommandManager.RegisterCommand(new TestCommandEcho());

            //Storage test
            string storageSettingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.Length - 17), "StorageSettings.json");
            Lavender.AddCustomStorageCategoryFromJson(storageSettingsPath, LCMPluginInfo.PLUGIN_NAME);


            // Conversation patcher test
            Random rand = new Random(); // Randomize which patch we apply first to confirm order of application doesn't change anything
            if (rand.Next() % 2 == 0)
            {
                Lavender.AddConversationPatcher(new TestConversationPatcherTatyana());
                Lavender.AddConversationPatcher(new TestConversationPatcherTatyana2());
                Log.LogInfo($"Registered conversation patch tests; test menu first.");
            }
            else
            {
                Lavender.AddConversationPatcher(new TestConversationPatcherTatyana2());
                Lavender.AddConversationPatcher(new TestConversationPatcherTatyana());
                Log.LogInfo($"Registered conversation patch tests; rewritter first.");
            }

            Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
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

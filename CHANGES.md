# Upcoming
## Data Only Mods
- added DataOnlyModManager
- added DataOnlyModDeclaration

## Runtime Importer
- fixed Lavender.AddLavenderAsset inconsistent param. order
- changed AddLavenderAssets to return int

## Item
- changed AddCustomItemsFromJson to return int

## Recipe
- changed AddCustomRecipesFromJson to return int

## Storage
- added OnStorageEnter callback
  - added AddOnStorageEnterCallback
- added OnStorageEnter callback
  - added AddOnStorageExitCallback

## Patches
- added Storage.EnterExternally Postfix
- added Storage.ExitExternally Postfix

## Settings
- added EnableDataOnlyMods
- added AdditionalDOMPath

# v6.0 (January 4th 2026) - Obenseuer v0.4.17
*changed GitHub and API version labeling to the one used on discord! (v0.6.0 -> v6.0)*
## Runtime Importer
- added LavenderAsset (LVA)
  - added lavenderAssets List
  - added AddLavenderAssets
  - added GetLavenderAsset
  - added GetLavenderAssetsFromMod
- added LavenderAssetBundle

## Furniture
- removed FurnitureAssetData
- updated FurnitureLib to use LVA 
- added 'InGame' Furniture Prefab Patching

## Item
- removed ItemCreator
- updated ItemLib to use LVA

## Recipe
- removed RecipeCreator
- updated RecipeLib to use LVA

## Dialogue
- improved in-code documentation for DialogueLib
- made ConversationUtils static
- small tweaks (mostly stability tweaks)

## Patches
- added SavableScriptableObject.LoadFromPath Postfix

# v0.5.0 (June 7th 2025)
## DialogueLib 
- added ConversationPatcher
- added ConversationPatchesManager
- added ConversationUtils
  - added GetResponsesTo
  - added GetStaticResponsesTo
  - added AdvanceToRespondable
  - added DoResponsesIncludeNPC
- added LinkOrdering

## Patches
- added InteractableTalk.OnDialogueStart Prefix
- added InteractableTalk.OnDialogueEnded Prefix

## Settings
- added DialoguePatcherVerboseLogging
- added EnabledVerboseConversations

# v0.4.0 (April 7th 2025)
## Recipe
- added ModifierInfo
  - added AddModifierInfo()
- added AddModifierToCraftingBase()

## Patches
- added CraftingBase.Start Prefix
- added ModifierUI.UpdateModifierInfo Prefix

# v0.3.0 (March 30th 2025)
## Furniture
- fixed SavableScriptableObject.LoadFromPath being ambiguous in OS v0.4.05
- reworked createdFurniture List as FurnitureDatabase
  - renamed GetFurnitureByTitle to FetchFurnitureByTitle
  - added FetchFurnitureByID
  - added FurnitureDatabaseParent GameObject
- improved FurnitureCreator methods (Database usage & no duplicate loading)
- removed FurnitureHandler
- added FurniturePrefabHandlerAttribute
  - renamed all FurnitureHandlerAttribute functions to FurniturePrefabHandlerAttribute

## Storage
- added customStorageCategoryDatabase
  - added AddCustomStorageCategory
  - added AddCustomStorageCategoryFromJson
- added customStorageSpawnCategoryDatabase
  - added AddCustomStorageSpawnCategory
  - added AddCustomStorageSpawnCategoryFromJson
- added StorageCreator
  - added AddSimpleStorage

## Patches
- added StorageCategoryDatabase.DeSerialize Postfix
- added StorageCategoryDatabase.FetchCategoryByName Postfix
- added StorageSpawnCategoryDatabase.DeSerialize Postfix
- added StorageSpawnCategoryDatabase.FetchStorageSpawnCategory Postfix

# v0.2.1 (February 24th 2025)
- added UseBepinexLog
- added Harmony patching in a try/catch -> better error reporting
- fixed BuildingSystem.AddFurniture Prefix being ambiguous

# v0.2.0 (February 8th 2025)
## Furniture
- added createdFurniture List
  - added GetFurnitureByTitel
  - added CreateShopFurniture (int amount, string furniture_titel)
- updated FurnitureCreator
- updated FurnitureConfig
  - added Id
  - added new categories
## Item
- added customItemDatabase
  - added AddCustomItem
  - added AddCustomItemsFromJson
- added ItemCreator
## Recipe
- added customRecipeDatabase
  - added AddCustomRecipe
  - added AddCustomRecipesFromJson
- added RecipeCreator
## Developer Console
- added Command manager (GreenMushLib merge)
  - added PrintToDevConsole (GreenMushLib merge)
## Patches
- added ItemDatabase.DeSerialize Postfix
- added ItemOperations.SetCollectibleItemValues Postfix
- added Item.ItemAppearance.LoadSprite Prefix
- added Item.ItemAppearance.Loadprefab Prefix
- added RecipeDatabase.DeSerialize Postfix
- added Recipe.RecipeAppearance.LoadSprite Prefix
- added DeveloperConsole.HandleInput Prefix (GreenMushLib merge)

*Downgraded project to netstandard 2.0*
# v0.1.0 (December 8th 2024)
## Furniture
- added FurnitureCreator
  - added FurnitureConfigToFurniture
  - added Create
  - added CreateShopFurniture
- added Furniture Config
- added FurnitureAssetData
- added Furniture Handler -> Callback when Furniture gets loaded, which allows us to add custum scripts to the prefabs
- added Furniture Shop Restock Handler -> sell your furnitures!
## Runtime Importer
- added FastObjImporter
- added ImageLoader
## Patches
- added SavableScriptableObject.LoadFromPath Prefix
- added FurnitureShop.AddFurniture Prefix -> always skips remaining prefixes and original
- added FurnitureShop.Restock Prefix -> always skips remaining prefixes and original
- added BuildingSystem.AddFurniture Prefix -> always skips remaining prefixes and original
## Settings
- added DetailedLog
- added SceneLoadingDoneNotification
- added FurnitureShop_AddFurniture_Prefix_SkipOriginal
- added FurnitureShop_Restock_Prefix_SkipOriginal
- added BuildingSystem_AddFurniture_Prefix_SkipOriginal

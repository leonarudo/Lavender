using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

/// <summary>
/// Handle loading data only (item/recipe/assets) mods
/// </summary>
namespace Lavender.DataOnlyModLib
{
    public static class DataOnlyModManager
    {
        private static bool hasRun = false;
        private static bool hasProcessedPotentialDOMs = false;
        private static string defaultSearchPath = "";
        
        private static List<DOMInfo> allFoundDOMs = new List<DOMInfo>();
        
        // dataObjectMods[modName] = infoObject
        public static Dictionary<string, DOMInfo> dataObjectMods = new Dictionary<string, DOMInfo>();

        // The standard filename of a dataonly mod's declaration file
        public const string DOMDeclarationFilename = "datamod.json";

        #region Events
        public delegate void GatherDataOnlyModsEvt();
        public static event GatherDataOnlyModsEvt? GatherDataOnlyMods;
        #endregion

        public static void Run()
        {
            if (hasRun)
            {
                return;
            }

            hasRun = true;
            if (!BepinexPlugin.Settings.EnableDataOnlyMods.Value)
            {
                LavenderLog.Log("Loading of data-only mods disabled in settings.  Skipping.");
                return;
            }

            defaultSearchPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "datamods");

            SearchDirectoryForDOMs(defaultSearchPath);
            GatherDataOnlyMods?.Invoke();

            PreloadProcessPotentialDOMs();
            hasProcessedPotentialDOMs = true;

            LoadDOMs();
        }

        public static string GetDefaultDOMSearchPath()
        {
            return defaultSearchPath;
        }

        // Search for potential data-only mods in the file system at the given path
        //  NOTE: This means that the directory being inspected is the "housing" folder, with each DOM having its own folder beneath.
        //  ie, if we are searching Obenseuer/DataObjectMods, then a dataonly mod would be found at Obenseuer/DataObjectMods/StevesModelTrains

        // If specified, looks for {@param declarationFileName} instead of the default filename
        // The criteriaCallback allows for filtering of found potential DOMs
        public static void SearchDirectoryForDOMs(string path, string declarationFileName = DOMDeclarationFilename, Func<DOMInfo, bool>? criteriaCallback = null)
        {
            IEnumerable<string> potentialDOMRoots = 
                Directory.EnumerateDirectories(path).
                Where<string>(x => File.Exists(Path.Combine(x, declarationFileName)));

            foreach (string domRoot in potentialDOMRoots)
            {
                LavenderLog.Detailed($"Inspecting {domRoot} to see if it contains a datamod");
                DOMInfo? dom = DOMInfo.TryLoadFromFile(domRoot, declarationFileName);
                if (dom != null)
                {
                    if (criteriaCallback == null || criteriaCallback(dom))
                    {
                        AddPotentialDOM(dom);
                    }
                    else
                    {
                        LavenderLog.Detailed($" Dataonly mod {dom.ModName} at '{Path.Combine(path, declarationFileName)}' skipped because criteriaCallback returned false.");
                    }
                }
                else
                {
                    LavenderLog.Detailed(" DOMInfo did not load from this location.  Presumably, not a datamod.");
                }
            }
        }

        // Add a potential data-only mod for consideration.
        // Only valid to be called while executing an OnFindDataObjectMods callback.
        public static void AddPotentialDOM(DOMInfo info)
        {
            if (hasProcessedPotentialDOMs)
            {
                throw new InvalidOperationException("Dataonly mod loading has already completed.  Cannot add potential DOM for loading after done.");
            }
            allFoundDOMs.Add(info);
        }

        // Process all potential dataonly mods prior to full loading
        // Determines whether any of the mods have errors or conflicts.  Any that do will not be loaded.
        private static void PreloadProcessPotentialDOMs()
        {
            Dictionary<string, DOMInfo> dict = new Dictionary<string, DOMInfo>();
            foreach (DOMInfo mod in allFoundDOMs)
            {
                if (mod.IsVersionCompatible(true))
                {
                    dict.Add(mod.DeclarationFilePath, mod);
                }
                else
                {
                    // Error logging happens in the IsVersionCompatible call
                    LavenderLog.Detailed($"Disabled dataonly mod {mod.ModName}");
                    mod.SetDOMState(DOMLoadingState.Error_Incompatible_Version);
                }
            }
            List<DOMInfo> uniqueFoundDOMs = dict.Values.ToList();

            Dictionary<string, List<DOMInfo>> domsByName = new Dictionary<string, List<DOMInfo>>();
            HashSet<string> conflictingNames = new HashSet<string>();
            foreach (DOMInfo mod in uniqueFoundDOMs)
            {
                if (domsByName.TryGetValue(mod.ModName, out List<DOMInfo> existingDOMsWithName))
                {
                    conflictingNames.Add(mod.ModName);
                    existingDOMsWithName.Add(mod);
                }
                else
                {
                    domsByName.Add(mod.ModName, [mod]);
                }
            }

            if (conflictingNames.Count > 0)
            {
                LavenderLog.Error($"While loading data-only mods, found multiple mods using the same name.  None of the conflicting mods will be loaded.");
                foreach (string name in conflictingNames)
                {
                    LavenderLog.Log($" Mod name '{name}' is used by multiple dataonlymods: ");
                    foreach (DOMInfo mod in domsByName[name])
                    {
                        LavenderLog.Log($"  '{mod.DeclarationFilePath}' ({mod.Declaration.Name} ver {mod.Declaration.Version})");
                        mod.SetDOMState(DOMLoadingState.Error_Incompatible_DuplicateName);
                    }
                }
            }

            foreach (DOMInfo mod in uniqueFoundDOMs)
            {
                if (!mod.IsErrored())
                {
                    dataObjectMods.Add(mod.ModName, mod);
                }
            }
        }

        // Load all unloaded DOMs
        public static void LoadDOMs()
        {
            List<DOMInfo> pendingMods = dataObjectMods.Values.Where(x => x.State == DOMLoadingState.Unloaded).ToList();
            if (pendingMods.Count > 0)
            {
                LavenderLog.Log($"Loading data-only mods (found {pendingMods.Count}):");

                foreach (DOMInfo mod in pendingMods)
                {
                    LavenderLog.Log($" Loading {mod.Declaration.ModName} (v{mod.Declaration.Version})...");
                    string modRoot = mod.AbsoluteDirectory;

                    int loadedItems = 0;
                    int loadedRecipes = 0;
                    int loadedAssetBundles = 0;
                    int loadedAssets = 0;

                    foreach (string file in mod.Declaration.ItemFiles)
                    {
                        string f = Path.Combine(modRoot, file);
                        if (File.Exists(f))
                        {
                            int fileItems = Lavender.AddCustomItemsFromJson(f, mod.ModName);
                            loadedItems += fileItems;
                            if (fileItems > 0)
                            {
                                LavenderLog.Detailed($"   Loaded {loadedItems} items from {file}");
                            }
                            else
                            {
                                LavenderLog.Error($"   File {file} in dataonly mod {modRoot} does not contain any items.  Does it have an invalid format/syntax error?");
                            }
                        }
                        else
                        {
                            LavenderLog.Error($"   Skipped loading dataonly mod {mod.ModName} item file because it was missing: {f}");
                        }
                    }

                    foreach (string file in mod.Declaration.RecipeFiles)
                    {
                        string f = Path.Combine(modRoot, file);
                        if (File.Exists(f))
                        {
                            int fileRecipes = Lavender.AddCustomRecipesFromJson(f, mod.ModName);
                            loadedRecipes += fileRecipes;
                            if (fileRecipes > 0)
                            {
                                LavenderLog.Detailed($"   Loaded {fileRecipes} items from {file}");
                            }
                            else
                            {
                                LavenderLog.Error($"   File {file} in dataonly mod {modRoot} does not contain any recipes.  Does it have an invalid format/syntax error?");
                            }
                        }
                        else
                        {
                            LavenderLog.Error($"  Skipped loading recipe file because it was missing: {f}");
                        }
                    }

                    foreach (string file in mod.Declaration.AssetBundles)
                    {
                        string f = Path.Combine(modRoot, file);
                        if (File.Exists(f))
                        {
                            // Why is the mod name/json file path order reversed for this one function?
                            int numLoadedAssets = Lavender.AddLavenderAssets(mod.ModName, f);
                            if (numLoadedAssets != 0)
                            {
                                loadedAssetBundles++;
                                loadedAssets += numLoadedAssets;
                            }
                        }
                        else
                        {
                            LavenderLog.Error($"  Skipped loading lavender asset bundle file because it was missing: {f}");
                        }
                    }

                    LavenderLog.Log($"  Loaded {loadedItems} items, {loadedRecipes} recipes, {loadedAssets} in {loadedAssetBundles} asset bundles.");

                    mod.SetDOMState(DOMLoadingState.Loaded);
                }
            }
        }
    }
}

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
        private static bool hasProcessedPotentialDOMs = false;
        private static List<string> searchPaths = new List<string>();
        private static List<DOMInfo> allFoundDOMs = new List<DOMInfo>();
        
        /// <summary>
        /// Map dataonly mods by unique internal name to their DOMInfo object storing runtime info about them
        /// This dictionary does not contain unloaded or erroring mods.
        /// The contents of this dictionary are only valid AFTER loading dataonly mods has finished.  You can check this via <see cref="IsLoadingDone"/>
        /// </summary>
        /// <seealso cref="DOMInfo"/>
        public static Dictionary<string, DOMInfo> dataOnlyMods = new Dictionary<string, DOMInfo>();

        /// <summary>
        /// Have dataonly mods been loaded?
        /// When true, the entire lifecycle of loading these mods has finished, and additional dataobject mods cannot be loaded
        /// </summary>
        public static bool IsLoadingDone { get; private set; } = false;

        /// <summary>
        /// The standard filename of a dataonly mod's declaration/metadata file
        /// A dataonly mod is required to contain this file within the mod's root directory
        /// </summary>
        public const string DOMDeclarationFilename = "datamod.json";

        #region Events
        public delegate void GatherDataOnlyModsEvt();

        /// <summary>
        /// This event fires when Lavender is starting to search for and load dataonly mods.
        /// If you wish to explicitly add dataonly mods or search locations, this is the LAST VALID TIME to do so.
        /// Ideally you would perform these calls when your plugin receives Awake from BepInEx.
        /// </summary>
        /// <seealso cref="AddModSearchPath(string)"/>
        /// <seealso cref="AddPotentialDOM(DOMInfo)"/>
        public static event GatherDataOnlyModsEvt? GatherDataOnlyMods;
        #endregion

        /// <summary>
        /// Perform any startup initialization
        /// Loading of dataonly mods is deferred to ensure we have time for all BepInEx plugins to be loaded first
        /// </summary>
        internal static void Init()
        {
            // Search the BepInEx plugins folder
            AddModSearchPath(BepInEx.Paths.PluginPath);
            
            // TODO - When Steam Workshop gets looked at by Loiste, add support for that.
            //  We need to work with Loiste to ensure we help them in this effort, and don't cause any headaches or extra work.
            //  Premature implementation on this front risks creating unnecessary implementation boundaries.
        }

        /// <summary>
        /// Search for and load dataonly mods
        /// Blocking operation; must occur during game startup before the ItemDatabase and RecipeDatabase deserialize from disc
        /// </summary>
        internal static void Run()
        {
            if (hasProcessedPotentialDOMs)
            {
                return;
            }

            if (!BepinexPlugin.Settings.EnableDataOnlyMods.Value)
            {
                LavenderLog.Log("Loading of data-only mods disabled in settings.  Skipping.");
                return;
            }

            // Let other mods hook into this, should they have any logic they want to run
            GatherDataOnlyMods?.Invoke();

            foreach (string path in searchPaths)
            {
                SearchDirectoryForDOMs(path);
            }
            

            PreloadProcessPotentialDOMs();
            hasProcessedPotentialDOMs = true;

            LoadDOMs();

            IsLoadingDone = true;

            // Clear up memory
            allFoundDOMs.Clear();
        }

        /// <summary>
        /// Search for potential data-only mods in the file system at the given path
        /// NOTE: This means that the directory being inspected is the "housing" folder, with each DOM having its own folder beneath.
        ///   ie, if we are searching Obenseuer/DataObjectMods, then a dataonly mod would be found at Obenseuer/DataObjectMods/StevesModelTrains
        ///   
        /// </summary>
        /// <param name="path">Directory to search for sub-directories containing the declarative dataonly mod file in</param>
        /// <param name="declarationFileName">Optional. If a custom value is provided, searches for files with that name instead.  Includes file extension.</param>
        /// <param name="criteriaCallback">Optional.  Filtering callback to perform additional validation on a found dataonly mod.</param>
        /// <exception cref="InvalidOperationException">When called after dataonly mod loading has finished.</exception>
        public static void SearchDirectoryForDOMs(string path, string declarationFileName = DOMDeclarationFilename, Func<DOMInfo, bool>? criteriaCallback = null)
        {
            if (hasProcessedPotentialDOMs)
            {
                throw new InvalidOperationException("Dataonly mod loading has already completed.  Cannot search for new dataonly mods.");
            }

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

        /// <summary>
        /// Add a data-only mod for validation and loading.
        /// NOTE: The <see cref="DOMInfo"/> object passed to this method is not guaranteed to be the object used to load the dataonly mod.
        /// If you want to get at a <see cref="DOMInfo"/> object (once dataonly mod loading is done), access it from <see cref="dataOnlyMods"/>.
        /// </summary>
        /// <param name="info">Standard <see cref="DOMInfo"/> object representing a dataonly mod</param>
        /// <exception cref="InvalidOperationException">When called after dataonly mod loading has finished.</exception>
        public static void AddPotentialDOM(DOMInfo info)
        {
            if (hasProcessedPotentialDOMs)
            {
                throw new InvalidOperationException("Dataonly mod loading has already completed.  Cannot add potential DOM for loading after done.");
            }

            // Uniqueness check based on file location and mod internal name
            if (!allFoundDOMs.Where(x => x.AbsoluteDirectory == info.AbsoluteDirectory && 
                x.DeclarationFileName == info.DeclarationFileName && 
                x.ModName == info.ModName).Any())
            {
                allFoundDOMs.Add(info);
            }
        }

        /// <summary>
        /// Add a search path to dataonly mod discovery.
        /// Expectation is that the pointed at directory exists and contains 0 or more child directories, which may optionally contain a "datamods.json" file declaring the dataonly mod.
        /// </summary>
        /// <param name="path">Path of the directory to add.  Please use absolute directories when possible</param>
        /// <exception cref="InvalidOperationException">When called after dataonly mod loading has finished.</exception>
        public static void AddModSearchPath(string path)
        {
            if (hasProcessedPotentialDOMs)
            {
                throw new InvalidOperationException("Dataonly mod loading has already completed.  Cannot add search path for loading after done.");
            }

            path = Path.GetFullPath(path);

            if (!searchPaths.Contains(path))
            {
                searchPaths.Add(path);
            }
        }

        /// <summary>
        /// Process all potential dataonly mods prior to full loading.
        /// Determines whether any of the mods have errors or conflicts.  Any that do will not be loaded.
        /// </summary>
        private static void PreloadProcessPotentialDOMs()
        {
            // Dictionary allows us to enforce uniqueness based on filepath (it should already be enforced in AddPotentialDOM, but this is a safeguard)
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
                    dataOnlyMods.Add(mod.ModName, mod);
                }
            }
        }

        /// <summary>
        /// Load all unloaded DOMs
        /// Expects dataOnlyMods to already be populated
        /// </summary>
        private static void LoadDOMs()
        {
            List<DOMInfo> pendingMods = dataOnlyMods.Values.Where(x => x.State == DOMLoadingState.Unloaded).ToList();
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
                            int numLoadedAssets = Lavender.AddLavenderAssets(f, mod.ModName);
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

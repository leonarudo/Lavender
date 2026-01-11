using Lavender.RuntimeImporter;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lavender.DataOnlyModLib
{
    /// <summary>
    /// Metadata declaring a dataonly mod.  Stored in the datamod.json file in the dataonly mod's directory.
    /// </summary>
    public class DataOnlyModDeclaration
    {
        /// <summary>
        /// Identifier name of the mod.  Must be unique; uniqueness is enforced at runtime - clashing entries will cause no offenders to be loaded
        /// </summary>
        public string ModName { get; set; } = "" ;

        /// <summary>
        /// The minimum version of Lavender that this data-only mod is compatible with.
        /// If not supported by this version of Lavender, we won't try to load the mod and will instead report to the log.
        /// </summary>
        public string? MinimumLavenderVersion { get; set; } = null;

        /// <summary>
        /// If a mod knows that it becomes incompatible with Lavender after a certain version, it can set this to disable loading after that version.
        /// </summary>
        public string? MaximumLavenderVersion { get; set; } = null;

        /// <summary>
        /// Player-facing mod name (ie "Chicken's Home Delivery Kebabs" instead of "ChickenKebab")
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Short description of the mod.  Player-facing, put whatever you want here as long as it's a string
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Name of mod authors and/or contributors
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// Mod version; useful for diagnosing issues and ensuring players have the correct mod version.
        /// Please increment this every time you release an update.
        /// <see cref="https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-version"/>
        /// </summary>
        public string Version { get; set; } = "";

        /// <summary>
        /// URL where your mod's download and updates can be found.  Github repo, Steam workshop, etc.
        /// </summary>
        public string URL { get; set; } = "";

        /// <summary>
        /// List of file(s) to load items from.  Must have the same JSON format as vanilla items.json.
        /// Example values: [], ["items.json"], ["shoes.json", "masks.json"]
        /// </summary>
        public List<string> ItemFiles { get; set; } = [];

        /// <summary>
        /// List of file(s) to load recipes from.  Must have the same JSON format as vanilla recipes.json.
        /// Example values: [], ["recipes.json"], ["lumbering.json", "smithing.json"]
        /// </summary>
        public List<string> RecipeFiles { get; set; } = [];

        /// <summary>
        /// List of Lavender Asset Bundle files to associate with your dataonly mod.
        /// Example values: [], ["assets.json"], ["osmo_variants.json", "shoes.json", "hammer_textures.json"]
        /// <see cref="https://leonarudo.github.io/Lavender-Docs/docs/RuntimeImporter/LavenderAsset.html"/>
        /// </summary>
        public List<string> LvAssetBundles { get; set; } = [];
    }
    
    /// <summary>
    /// Track the state of a dataonly mod.
    /// Upon entering an error state, it is not a valid operation to leave the error state.
    /// </summary>
    public enum DOMLoadingState
    {
        Unloaded,
        Loaded,
        Error_Incompatible_Version,
        Error_Incompatible_DuplicateName,
        Error
    }

    /// <summary>
    /// Hold runtime information about a dataonly mod, including where it is located, its metadata declaration, and compatibility logic
    /// </summary>
    public class DOMInfo
    {
        public DataOnlyModDeclaration Declaration { get; }

        /// <summary>
        /// The directory where the mod is located
        /// </summary>
        public string AbsoluteDirectory { get; }
        public string DeclarationFileName { get; }
        public string DeclarationFilePath { get
            {
                return Path.Combine(AbsoluteDirectory, DeclarationFileName);
            }
        }

        /// <summary>
        /// Internal name of the mod.  Must be unique across all mods.
        /// </summary>
        public string ModName { get { return Declaration.ModName; } }

        /// <summary>
        /// Loading state of this dataonly mod.
        /// </summary>
        /// <seealso cref="IsErrored()"/>
        public DOMLoadingState State { get; private set; }

        private DOMInfo(DataOnlyModDeclaration declaration, string directory, string declFile)
        {
            Declaration = declaration;
            AbsoluteDirectory = directory;
            DeclarationFileName = declFile;
        }

        /// <summary>
        /// Load a dataonly mod's metadata declaration from the specified directory & file, and construct a new DOMInfo object to track it
        /// The constructed DOMInfo object must be registered with the DataOnlyModManager to be used.
        /// </summary>
        /// <param name="directory">The dataonly mod's directory</param>
        /// <param name="filename">Name of the dataonly mod's declaration file.  Standard filename is "datamod.json"</param>
        /// <returns>On success, a new DOMInfo object representing this dataonly mod.  On failure, logs the error and returns null.</returns>
        /// <seealso cref="DataOnlyModManager.AddPotentialDOM(DOMInfo)"/>
        internal static DOMInfo? TryLoadFromFile(string directory, string filename)
        {
            string fullPath = Path.Combine(directory, filename);
            if (!File.Exists(fullPath))
            {
                LavenderLog.Error($"DataObjectMod declaration file does not exist: {fullPath}");
                return null;
            }

            try
            {
                string rawJsonData = File.ReadAllText(fullPath);

                DataOnlyModDeclaration? decl = JsonConvert.DeserializeObject<DataOnlyModDeclaration>(rawJsonData);

                if (decl == null)
                {
                    LavenderLog.Error($"Failed to parse dataonly mod declaration file {fullPath} - please check it for syntax errors.");
                    return null;
                }

                LavenderLog.Detailed($" Found dataonly mod with these meta properties: ModName = {decl.ModName}, Name = {decl.Name}, Version = {decl.Version}");

                if (string.IsNullOrEmpty(decl.ModName))
                {
                    LavenderLog.Error($"Dataonly mod declaration located at {fullPath} does not contain a ModName.");
                    return null;
                }

                return new DOMInfo(decl, directory, filename);
            }
            catch (Exception ex)
            {
                LavenderLog.Error($"Failed to load dataonly mod declaration found at {fullPath}");
                LavenderLog.Error(ex.ToString());
            }

            return null;
        }

        /// <summary>
        /// Test if the dataonly mod is compatible with the running environment.
        /// </summary>
        /// <param name="bLogErrors"></param>
        /// <returns>True if compatible with the running environment</returns>
        public bool IsVersionCompatible(bool bLogErrors = false)
        {
            if (Declaration.MinimumLavenderVersion != null)
            {
                Version minVersion = new Version(Declaration.MinimumLavenderVersion);
                if (minVersion.CompareTo(BepinexPlugin.LavenderVersion) > 0)
                {
                    if (bLogErrors)
                    {
                        LavenderLog.Error($"Data-Only Mod {ModName} requires minimum Lavender version {Declaration.MinimumLavenderVersion}.  Please update Lavender.  You have: {BepinexPlugin.LavenderVersion.ToString()}");
                    }
                    return false;
                }

                if (Declaration.MaximumLavenderVersion == null)
                {
                    /*
                     * Example of adding a barrier to the check when Lavender introduces a breaking change.
                     * In this example, we have a theoretical breaking change to functionality at version 6.0.
                     * Dataonly mods that were made for earlier versions are not expected to function correctly, so we don't load them
                     * UNLESS... They have an explicit supported maximum lavender version, in which case we leave it to the mod author to ensure everything is fine
                    
                    // Breaking change happened at Lavender version 6.0; don't load mods made for earlier versions unless they explicitly support (via MaximumLavenderVersion)
                    Version breakingChangeBarrier = new Version(6, 0);
                    if (breakingChangeBarrier.CompareTo(minVersion) < 0)
                    {
                        if (bLogErrors)
                        {
                            LavenderLog.Error($"Data-Only Mod {ModName} was built for Lavender version prior to 6.0.  Breaking changes happened during the 6.0 update cycle.  This mod cannot be loaded until updated.");
                        }
                        return false;
                    }*/
                }
            }

            if (Declaration.MaximumLavenderVersion != null)
            {
                Version maxVersion = new Version(Declaration.MaximumLavenderVersion);
                if (maxVersion.CompareTo(BepinexPlugin.LavenderVersion) < 0)
                {
                    if (bLogErrors)
                    {
                        LavenderLog.Error($"Data-Only Mod {ModName} is not compatible with Lavender versions beyond {Declaration.MaximumLavenderVersion}.  Not loading.  You have: {BepinexPlugin.LavenderVersion.ToString()}");
                    }
                    return false;
                }
            }

            return true;
        }

        internal void SetDOMState(DOMLoadingState state, bool canRecoverFromError = false)
        {
            if (IsErrored())
            {
                if (!canRecoverFromError)
                {
                    return;
                }
            }
            State = state;
        }

        /// <summary>
        /// Check if this dataonly mod is in a state that prevents it from being loaded.
        /// Note that the state is not checked instantly, but is cached and may not be correct until after all dataonly mods have been loaded.
        /// </summary>
        /// <returns>True/False</returns>
        public bool IsErrored()
        {
            return State == DOMLoadingState.Error || State == DOMLoadingState.Error_Incompatible_Version || State == DOMLoadingState.Error_Incompatible_DuplicateName;
        }
    }
}

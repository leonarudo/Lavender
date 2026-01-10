using Lavender.RuntimeImporter;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lavender.DataOnlyModLib
{
    // The data stored in the mod's datamod.json file
    public class DataOnlyModDeclaration
    {
        // Identifier name of the mod.  Must be unique; uniqueness is enforced at runtime - clashing entries will cause no offenders to be loaded
        public string ModName { get; set; } = "" ;

        // The minimum version of Lavender that this data-only mod is compatible with.
        // If not supported by this version of Lavender, we won't try to load the mod and will instead report to the log.
        public string? MinimumLavenderVersion { get; set; } = null;

        // If a mod knows that it becomes incompatible with Lavender after a certain version, it can set this to disable loading after that version.
        public string? MaximumLavenderVersion { get; set; } = null;

        // Player-facing data about the mod
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Author { get; set; } = "";
        public string Version { get; set; } = "";

        // Where to load data from
        public List<string> ItemFiles { get; set; } = []; // Example value: ["items.json"];
        public List<string> RecipeFiles { get; set; } = []; // Example value: ["recipes.json"];
        public List<string> AssetBundles { get; set; } = []; // Example value: ["assetbundle.json"];
    }

    public enum DOMLoadingState
    {
        Unloaded,
        Loaded,
        Error_Incompatible_Version,
        Error_Incompatible_DuplicateName,
        Error
    }

    // Hold meta information about a data-only mod at runtime
    public class DOMInfo
    {
        public DataOnlyModDeclaration Declaration { get; }

        public string AbsoluteDirectory { get; }
        public string DeclarationFileName { get; }
        public string DeclarationFilePath { get
            {
                return Path.Combine(AbsoluteDirectory, DeclarationFileName);
            }
        }

        public string ModName { get { return Declaration.ModName; } }

        public DOMLoadingState State { get; private set; }

        public DOMInfo(DataOnlyModDeclaration declaration, string directory, string declFile)
        {
            Declaration = declaration;
            AbsoluteDirectory = directory;
            DeclarationFileName = declFile;
        }

        public static DOMInfo? TryLoadFromFile(string directory, string filename)
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

        public bool IsErrored()
        {
            return State == DOMLoadingState.Error || State == DOMLoadingState.Error_Incompatible_Version || State == DOMLoadingState.Error_Incompatible_DuplicateName;
        }
    }
}

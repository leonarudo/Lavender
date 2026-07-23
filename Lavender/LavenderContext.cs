using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender
{
    /// <summary>
    /// The LavenderContext is the main entry to Lavender. After creating one with an BaseUnityPlugin, it is used to access all higher Lavender functions
    /// </summary>
    public class LavenderContext
    {
        public string ModGUID { get; private set; }

        public string ModName { get; private set; }

        public Version ModVersion { get; private set; }

        internal LavenderContext(BaseUnityPlugin baseUnityPlugin)
        {
            ModGUID = baseUnityPlugin.Info.Metadata.GUID;
            ModName = baseUnityPlugin.Info.Metadata.Name;
            ModVersion = baseUnityPlugin.Info.Metadata.Version;

            Lavender.lavenderContexts.Add(this);
        }
    }
}

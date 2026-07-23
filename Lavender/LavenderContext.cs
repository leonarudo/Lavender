using BepInEx;
using Lavender.CommandLib;
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

        #region CommandLib

        /// <summary>
        /// Register an IConsoleCommand with the handler
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Returns true on success</returns>
        public bool RegisterCommand(IConsoleCommand command)
        {
            if(CommandManager.CommandRegistry.ContainsKey(command.Name)) {  return false; }

            CommandManager.CommandRegistry[command.Name] = (command, this);
            LavenderLog.Log($"[{ModGUID}] Registering command '{command.Name}'");
            return true;
        }

        #endregion
    }
}

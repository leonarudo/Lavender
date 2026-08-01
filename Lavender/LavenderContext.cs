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

        /// <summary>
        /// Remove an IConsoleCommand with the given name and this LavenderContext as owner
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns false if there isn't any command registered by this name or if this LavenderContext is not the owner</returns>
        public bool RemoveCommand(string name)
        {
            if(CommandManager.CommandRegistry.TryGetValue(name, out var cmd))
            {
                if(cmd.Item2.Equals(this))
                {
                    CommandManager.CommandRegistry.Remove(name);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}

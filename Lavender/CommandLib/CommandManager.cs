using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lavender.CommandLib
{
    public static class CommandManager
    {
        public static Dictionary<string, (IConsoleCommand, LavenderContext)> CommandRegistry = [];

        internal static DeveloperConsole? LastInConsole;

        internal static bool RunCommand(string cmdName, string[] args)
        {
            if (cmdName.ToLowerInvariant() == "ext_help")
            {
                ShowHelp(args);
                return false;
            }

            if (CommandRegistry.TryGetValue(cmdName, out var command))
            {
                try
                {
                    command.Item1.Execute(args);
                }
                catch (Exception ex)
                {
                    LavenderLog.Log($"\n\nEncountered an exception while running a command:\n [Ctx: {command.Item2.ModGUID}] Command: {cmdName}\n  Ex: {ex}");
                }

                return false;
            }

            return true;
        }

        private static void ShowHelp(string[] args)
        {
            if (args.Length < 2)
            {
                StringBuilder sb = new StringBuilder("--- Lavender CommandLib Extended Console ---\n");

                var grouped = CommandRegistry.GroupBy(x => x.Value.Item2.ModGUID);

                foreach (var group in grouped)
                {
                    string guid = group.Key;
                    sb.AppendLine($"\n{guid.ToUpper()}");

                    foreach (var cmd in group)
                    {
                        sb.AppendLine($"{cmd.Key.ToUpper()} -> {cmd.Value.Item1.Description}\n");
                    }
                }

                sb.AppendLine("\nType 'ext_help <cmd>' for usage info!");

                PrintToDevConsole(sb);
            }
            else
            {
                if (CommandRegistry.TryGetValue(args[1], out var command))
                {
                    PrintToDevConsole(command.Item1.Usage);
                }
                else
                {
                    PrintToDevConsole($"Unknown modded command '{args[1]}'");
                    PrintToDevConsole("Maybe it's spelled wrong or a vanilla command?\n");
                }
            }
        }

        internal static void PrintToDevConsole(string message) => LastInConsole?.Print(message);

        /// <summary>
        /// Prints a message to the dev console
        /// </summary>
        /// <param name="message"></param>
        public static void PrintToDevConsole(object message) => PrintToDevConsole(message.ToString());
    }
}

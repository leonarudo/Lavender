using Lavender.CommandLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.Test
{
    public class TestCommandEcho : IConsoleCommand
    {
        public string Name => "echo";

        public string Description => "echoes whatever you tell it to";

        public string Usage => "echo <string>";

        public void Execute(params string[] args)
        {
            if (args.Length < 2)
            {
                CommandManager.PrintToDevConsole(Usage);
                return;
            }

            //Specifically call string.Trim(char[]) instead of 
            //string.Trim(char) to avoid a netstandard 2.0 -> 2.1
            //issue
            CommandManager.PrintToDevConsole(args[1].Trim(['"']));
        }

        // Not good, but good enought for internal testing
        public static void InternalTestCase()
        {
            TestLog.Log("-- CommandLib Echo Test --");

            var cmdRegistry = CommandManager.GetCommandRegistry();

            if(cmdRegistry.ContainsKey("echo"))
            {
                TestLog.Log("Cmd 'echo' found in ReadOnlyCMDRegistry...");

                if(BepinexPlugin.ctx.RemoveCommand("echo"))
                {
                    cmdRegistry = CommandManager.GetCommandRegistry();

                    if (!cmdRegistry.ContainsKey("echo"))
                    {
                        TestLog.Log("Cmd 'echo' succesfully removed...\nAdding it again...");
                        BepinexPlugin.ctx.RegisterCommand(new TestCommandEcho());
                    }
                    else { TestLog.Error("!!! Remove didn't happen even do it did?!"); }
                }
                else { TestLog.Error("Something went wrong while trying to remove found 'echo' cmd :("); }
            }
            else { TestLog.Error("Couldn't find 'echo' in the ReadOnlyCMDRegistry :("); }

            TestLog.Log("-- CommandLib Echo Test --");
        }
    }
}

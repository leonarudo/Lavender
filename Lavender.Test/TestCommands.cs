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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.CommandLib
{
    /// <summary>
    /// Interface representing the general functionality of a console command.
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// The name of the console command, (e.g., "give").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The description of the console command (e.g., "Gives the player an item").
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// A short explanation on how to use the console command (e.g., "give [item_name]").
        /// </summary>
        public string Usage { get; }

        /// <summary>
        /// A function to be executed when the console command is run.
        /// </summary>
        public void Execute(params string[] args);
    }
}

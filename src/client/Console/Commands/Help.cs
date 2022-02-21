// <copyright file="Help.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    /// <summary>
    ///     Provides help with using the commands.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Help : Command
    {
        private const int PageSize = 5;
        private readonly CommandInvoker commandInvoker;
        private Dictionary<string, List<string>> commandDescriptions = new();

        private List<List<string>> commandPages = new();

        /// <summary>
        ///     Create a help command for all commands discovered by a <see cref="CommandInvoker" />.
        /// </summary>
        /// <param name="invoker">The invoker providing the commands.</param>
        public Help(CommandInvoker invoker)
        {
            commandInvoker = invoker;
            commandInvoker.CommandsUpdated += (_, _) => BuildInfos();
        }

        /// <inheritdoc />
        public override string Name => "help";

        /// <inheritdoc />
        public override string HelpText => "Provides help with using the commands.";

        private void BuildInfos()
        {
            BuildPages();
            BuildCommandDetails();
        }

        private void BuildPages()
        {
            commandPages.Clear();

            List<string> commands = commandInvoker.CommandNames
                .Select(command => $"{command} # {commandInvoker.GetCommandHelpText(command)}").ToList();

            commandPages.Add(new List<string>());

            foreach (string command in commands)
            {
                if (commandPages[^1].Count >= PageSize) commandPages.Add(new List<string>());

                commandPages[^1].Add(command);
            }
        }

        private void BuildCommandDetails()
        {
            commandDescriptions.Clear();

            foreach (string command in commandInvoker.CommandNames)
            {
                List<string> description = new() { $"{command} # {commandInvoker.GetCommandHelpText(command)}" };
                description.AddRange(commandInvoker.GetCommandSignatures(command));

                commandDescriptions.Add(command, description);
            }
        }

        /// <exclude />
        public void Invoke()
        {
            Context.Console.WriteResponse("Use 'help' to get information on available commands.");
            Context.Console.WriteResponse("Use 'help <page : Int32>' to get a specific command list page.");
            Context.Console.WriteResponse("Use 'help <command : String>' to get info for a specific command.");
        }

        /// <exclude />
        public void Invoke(int page)
        {
            if (page > commandPages.Count || page <= 0)
            {
                Context.Console.WriteError($"There are only {commandPages.Count} pages of commands.");
            }
            else
            {
                Context.Console.WriteResponse($"Page {page} of {commandPages.Count}:");
                commandPages[page - 1].ForEach(Context.Console.WriteResponse);
            }
        }

        /// <exclude />
        public void Invoke(string command)
        {
            if (commandDescriptions.TryGetValue(command, out List<string>? description))
                description.ForEach(Context.Console.WriteResponse);
            else Context.Console.WriteError($"Command '{command}' not found.");
        }
    }
}

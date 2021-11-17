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

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Help : Command
    {
        private const int PageSize = 5;
        private readonly CommandInvoker commandInvoker;

        private List<List<string>> commandPages = new();

        public Help(CommandInvoker invoker)
        {
            commandInvoker = invoker;
            commandInvoker.CommandsUpdated += BuildPages;
        }

        public override string Name => "help";
        public override string HelpText => "Provides help with using the commands.";

        private void BuildPages()
        {
            List<string> commands = commandInvoker.CommandNames
                .Select(command => $"{command} # {commandInvoker.GetCommandHelpText(command)}").ToList();

            commandPages.Add(new List<string>());

            foreach (string command in commands)
            {
                if (commandPages[^1].Count >= PageSize) commandPages.Add(new List<string>());

                commandPages[^1].Add(command);
            }
        }

        public void Invoke()
        {
            Context.Console.WriteResponse("Use 'help' to get information on available commands.");
            Context.Console.WriteResponse("Use 'help <page : int>' to get a specific command list page.");
        }

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
    }
}

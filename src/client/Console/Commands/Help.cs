// <copyright file="Help.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Provides help with using the commands.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Help : Command
{
    private const Int32 PageSize = 5;
    private readonly Dictionary<String, List<Entry>> commandDescriptions = new();
    private readonly CommandInvoker commandInvoker;
    private readonly List<List<Entry>> commandPages = [];

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
    public override String Name => "help";

    /// <inheritdoc />
    public override String HelpText => "Provides help with using the commands.";

    private void BuildInfos()
    {
        BuildPages();
        BuildCommandDetails();
    }

    private void BuildPages()
    {
        commandPages.Clear();

        List<(String command, String description)> commands = commandInvoker.CommandNames
            .Select(command => (command, $"{command} # {commandInvoker.GetCommandHelpText(command)}")).ToList();

        commandPages.Add([]);

        foreach ((String command, String description) in commands)
        {
            if (commandPages[^1].Count >= PageSize) commandPages.Add([]);

            commandPages[^1].Add(new Entry(description,
                [new FollowUp("Show details", () => { Invoke(command); })]));
        }
    }

    private void BuildCommandDetails()
    {
        commandDescriptions.Clear();

        foreach (String command in commandInvoker.CommandNames)
        {
            List<Entry> description = [new($"{command} # {commandInvoker.GetCommandHelpText(command)}", [])];

            description.AddRange(commandInvoker
                .GetCommandSignatures(command)
                .Select(signature => new Entry(signature, [])));

            commandDescriptions.Add(command, description);
        }
    }

    /// <exclude />
    public void Invoke()
    {
        Context.Output.WriteResponse("Use 'help' to get information on available commands.");
        Context.Output.WriteResponse("Use 'help <page : Int32>' to get a specific command list page.");
        Context.Output.WriteResponse("Use 'help <command : String>' to get info for a specific command.");
    }

    /// <exclude />
    public void Invoke(Int32 page)
    {
        if (page > commandPages.Count || page <= 0)
        {
            Context.Output.WriteError($"There are only {commandPages.Count} pages of commands.");
        }
        else
        {
            Context.Output.WriteResponse($"Page {page} of {commandPages.Count}:",
            [
                new FollowUp("Show next page", () => { Invoke(page + 1); }),
                new FollowUp("Show previous page", () => { Invoke(page - 1); })
            ]);

            commandPages[page - 1].ForEach(entry => Context.Output.WriteResponse(entry.Text, entry.FollowUp));
        }
    }

    /// <exclude />
    public void Invoke(String command)
    {
        if (commandDescriptions.TryGetValue(command, out List<Entry>? description))
            description.ForEach(entry => Context.Output.WriteResponse(entry.Text, entry.FollowUp));
        else Context.Output.WriteError($"Command '{command}' not found.");
    }

    private sealed record Entry(String Text, FollowUp[] FollowUp);
}

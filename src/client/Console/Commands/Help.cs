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
    private const int PageSize = 5;

    private readonly Dictionary<string, List<Entry>> commandDescriptions = new();
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

        List<(string command, string description)> commands = commandInvoker.CommandNames
            .Select(command => (command, $"{command} # {commandInvoker.GetCommandHelpText(command)}")).ToList();

        commandPages.Add([]);

        foreach ((string command, string description) in commands)
        {
            if (commandPages[^1].Count >= PageSize) commandPages.Add([]);

            commandPages[^1].Add(new Entry(description,
                [new FollowUp("Show details", () => { Invoke(command); })]));
        }
    }

    private void BuildCommandDetails()
    {
        commandDescriptions.Clear();

        foreach (string command in commandInvoker.CommandNames)
        {
            List<Entry> description = [new Entry($"{command} # {commandInvoker.GetCommandHelpText(command)}", Array.Empty<FollowUp>())];

            description.AddRange(commandInvoker
                .GetCommandSignatures(command)
                .Select(signature => new Entry(signature, Array.Empty<FollowUp>())));

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
            Context.Console.WriteResponse($"Page {page} of {commandPages.Count}:",
                new FollowUp("Show next page", () => { Invoke(page + 1); }),
                new FollowUp("Show previous page", () => { Invoke(page - 1); }));

            commandPages[page - 1].ForEach(entry => Context.Console.WriteResponse(entry.Text, entry.FollowUp));
        }
    }

    /// <exclude />
    public void Invoke(string command)
    {
        if (commandDescriptions.TryGetValue(command, out List<Entry>? description))
            description.ForEach(entry => Context.Console.WriteResponse(entry.Text, entry.FollowUp));
        else Context.Console.WriteError($"Command '{command}' not found.");
    }

    private sealed record Entry(string Text, FollowUp[] FollowUp);
}

// <copyright file="Help.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Presentation.Legacy.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Provides help with using the commands.
/// </summary>
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
                context => [new FollowUp("Show details", () => { Invoke(command, context); })]));
        }
    }

    private void BuildCommandDetails()
    {
        commandDescriptions.Clear();

        foreach (String command in commandInvoker.CommandNames)
        {
            List<Entry> description = [new($"{command} # {commandInvoker.GetCommandHelpText(command)}", _ => [])];

            description.AddRange(commandInvoker
                .GetCommandSignatures(command)
                .Select(signature => new Entry(signature, _ => [])));

            commandDescriptions.Add(command, description);
        }
    }

    /// <exclude />
    public void Invoke(Context context)
    {
        context.Output.WriteResponse("Use 'help' to get information on available commands.");
        context.Output.WriteResponse("Use 'help <page : Int32>' to get a specific command list page.");
        context.Output.WriteResponse("Use 'help <command : String>' to get info for a specific command.");
    }

    /// <exclude />
    public void Invoke(Int32 page, Context context)
    {
        if (page > commandPages.Count || page <= 0)
        {
            context.Output.WriteError($"There are only {commandPages.Count} pages of commands.");
        }
        else
        {
            context.Output.WriteResponse($"Page {page} of {commandPages.Count}:",
            [
                new FollowUp("Show next page", () => { Invoke(page + 1, context); }),
                new FollowUp("Show previous page", () => { Invoke(page - 1, context); })
            ]);

            commandPages[page - 1].ForEach(entry => context.Output.WriteResponse(entry.Text, entry.GetFollowUp(context)));
        }
    }

    /// <exclude />
    public void Invoke(String command, Context context)
    {
        if (commandDescriptions.TryGetValue(command, out List<Entry>? description))
            description.ForEach(entry => context.Output.WriteResponse(entry.Text, entry.GetFollowUp(context)));
        else context.Output.WriteError($"Command '{command}' not found.");
    }

    private sealed record Entry(String Text, Func<Context, FollowUp[]> GetFollowUp);
}

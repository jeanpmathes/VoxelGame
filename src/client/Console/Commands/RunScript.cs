// <copyright file="RunScript.cs" company="VoxelGame">
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
using System.IO;
using JetBrains.Annotations;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;

/// <summary>
///     Runs a script.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RunScript : Command
{
    /// <inheritdoc />
    public override String Name => "run-script";

    /// <inheritdoc />
    public override String HelpText => "Runs a script.";

    /// <exclude />
    public void Invoke(String name)
    {
        if (!Context.IsScript) Do(Context, name);
        else Context.Output.WriteError("Cannot run scripts from scripts.");
    }

    /// <summary>
    ///     Perform a run of the command, allowing to run a script.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="name">The name of the script to edit.</param>
    /// <param name="ignoreErrors">Whether to ignore errors.</param>
    /// <returns>Whether a script was run.</returns>
    public static Boolean Do(Context context, String name, Boolean ignoreErrors = false)
    {
        String? script = context.Player.World.Data.GetScript(name);

        FollowUp followUp = new("Edit script",
            () =>
            {
                EditScript.Do(context, name);
            });

        if (script == null)
        {
            if (!ignoreErrors)
                context.Output.WriteError($"Script '{name}' does not exist.", [followUp]);

            return false;
        }

        using StringReader lines = new(script);

        var loc = 0;

        while (lines.ReadLine() is {} line)
        {
            if (line.Length == 0) continue;

            context.Invoker.InvokeCommand(line, context.ToScript());

            loc++;
        }

        context.Output.WriteResponse($"Executed {loc} line(s) of the '{name}' script.", [followUp]);

        return true;
    }
}

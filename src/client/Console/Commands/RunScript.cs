// <copyright file="RunScript.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using JetBrains.Annotations;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Runs a script.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RunScript : Command
{
    /// <inheritdoc />
    public override string Name => "run-script";

    /// <inheritdoc />
    public override string HelpText => "Runs a script.";

    /// <exclude />
    public void Invoke(string name)
    {
        if (!Context.IsScript) Do(Context, name);
        else Context.Console.WriteError("Cannot run scripts from scripts.");
    }

    /// <summary>
    ///     Perform a run of the command, allowing to run a script.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="name">The name of the script to edit.</param>
    /// <param name="ignoreErrors">Whether to ignore errors.</param>
    /// <returns>Whether a script was run.</returns>
    public static bool Do(Context context, string name, bool ignoreErrors = false)
    {
        string? script = context.Player.World.Data.GetScript(name);

        FollowUp followUp = new("Edit script",
            () =>
            {
                EditScript.Do(context, name);
            });

        if (script == null)
        {
            if (!ignoreErrors)
                context.Console.WriteError($"Script '{name}' does not exist.", followUp);

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

        context.Console.WriteResponse($"Executed {loc} lines of the '{name}' script", followUp);

        return true;
    }
}

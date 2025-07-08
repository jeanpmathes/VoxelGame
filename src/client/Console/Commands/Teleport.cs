// <copyright file="Teleport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace VoxelGame.Client.Console.Commands;
    #pragma warning disable CA1822

/// <summary>
///     Teleport to a specified position or target.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Teleport : Command
{
    /// <inheritdoc />
    public override String Name => "teleport";

    /// <inheritdoc />
    public override String HelpText => "Teleport to a specified position or target.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        Do(Context, this, (x, y, z));
    }

    /// <exclude />
    public void Invoke(String target)
    {
        if (GetNamedPosition(target) is {} position) Do(Context, this, position);
        else Context.Output.WriteError($"Unknown target: {target}");
    }

    /// <summary>
    ///     Externally simulate command invocation, teleporting the player.
    /// </summary>
    /// <param name="context">The context in which the command is executed.</param>
    /// <param name="command">The command being invoked.</param>
    /// <param name="position">The position to teleport to.</param>
    public static void Do(Context context, Command command, Vector3d position)
    {
        command.SetPreviousPlayerPosition(context.Player.Body.Transform.Position);
        context.Player.Body.Transform.Position = position;
    }
}

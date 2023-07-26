// <copyright file="Teleport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    public override string Name => "teleport";

    /// <inheritdoc />
    public override string HelpText => "Teleport to a specified position or target.";

    /// <exclude />
    public void Invoke(double x, double y, double z)
    {
        Do(Context, (x, y, z));
    }

    /// <exclude />
    public void Invoke(string target)
    {
        if (GetNamedPosition(target) is {} position) Do(Context, position);
        else Context.Console.WriteError($"Unknown target: {target}");
    }

    /// <summary>
    ///     Externally simulate command invocation, teleporting the player.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="position">The position to teleport to.</param>
    public static void Do(Context context, Vector3d position)
    {
        context.Player.Teleport(position);
    }
}

// <copyright file="Teleport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

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
        Context.Player.Position = (x, y, z);
    }

    /// <exclude />
    public void Invoke(string target)
    {
        if (GetNamedPosition(target) is {} position)
        {
            Context.Player.Position = position;
        }
        else
        {
            Context.Console.WriteError($"Unknown target: {target}");
        }
    }
}


// <copyright file="Teleport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    public void Invoke(float x, float y, float z)
    {
        Context.Player.Position = (x, y, z);
    }

    /// <exclude />
    public void Invoke(string target)
    {
        switch (target)
        {
            case "origin":
                SetPlayerPosition((0, 0, 0));

                break;

            case "spawn":
                SetPlayerPosition(Context.Player.World.SpawnPosition);

                break;

            default:
                Context.Console.WriteError($"Unknown target: {target}");

                break;
        }

        void SetPlayerPosition(Vector3d position)
        {
            Context.Player.Position = position;
        }
    }
}
// <copyright file="SetSpawnPoint.cs" company="VoxelGame">
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
///     Sets the spawn position for the current world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SetSpawnPoint : Command
{
    /// <inheritdoc />
    public override String Name => "set-spawnpoint";

    /// <inheritdoc />
    public override String HelpText => "Sets the spawn position for the current world.";

    /// <exclude />
    public void Invoke(Double x, Double y, Double z)
    {
        SetSpawnPosition((x, y, z));
    }

    /// <exclude />
    public void Invoke()
    {
        SetSpawnPosition(Context.Player.Position);
    }

    private void SetSpawnPosition(Vector3d newSpawnPoint)
    {
        Context.Player.World.SpawnPosition = newSpawnPoint;
    }
}

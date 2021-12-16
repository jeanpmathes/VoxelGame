﻿// <copyright file="SetSpawnPoint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenToolkit.Mathematics;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SetSpawnPoint : Command
    {
        public override string Name => "set-spawnpoint";

        public override string HelpText => "Sets the spawn position for the current world.";

        public void Invoke(float x, float y, float z)
        {
            SetSpawnPosition((x, y, z));
        }

        public void Invoke()
        {
            SetSpawnPosition(Context.Player.Position);
        }

        private void SetSpawnPosition(Vector3 newSpawnPoint)
        {
            Context.Player.World.SetSpawnPosition(newSpawnPoint);
        }
    }
}
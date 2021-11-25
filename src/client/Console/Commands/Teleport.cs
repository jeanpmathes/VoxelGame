// <copyright file="Teleport.cs" company="VoxelGame">
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
    public class Teleport : Command
    {
        public override string Name => "teleport";
        public override string HelpText => "Teleport to a specified position or target.";

        public void Invoke(float x, float y, float z)
        {
            Context.Player.Position = (x, y, z);
        }

        public void Invoke(string target)
        {
            switch (target)
            {
                case "origin":
                    SetPlayerPosition((0, 0, 0));

                    break;

                case "spawn":
                    SetPlayerPosition(Context.Player.World.GetSpawnPosition());

                    break;

                default:
                    Context.Console.WriteError($"Unknown target: {target}");

                    break;
            }

            void SetPlayerPosition(Vector3 position)
            {
                Context.Player.Position = position;
            }
        }
    }
}

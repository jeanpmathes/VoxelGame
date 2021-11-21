// <copyright file="Teleport.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Teleport : Command
    {
        public override string Name => "teleport";
        public override string HelpText => "Teleport to a specified position.";

        public void Invoke(float x, float y, float z)
        {
            Context.Player.Position = (x, y, z);
        }
    }
}

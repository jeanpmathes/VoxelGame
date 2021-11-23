// <copyright file="DoUpdate.cs" company="VoxelGame">
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
    public class DoUpdate : Command
    {
        public override string Name => "do-update";
        public override string HelpText => "Cause a random update to occur for a targeted position.";

        public void Invoke()
        {
            Update(Context.Player.TargetPosition);
        }

        public void Invoke(int x, int y, int z)
        {
            Update((x, y, z));
        }

        private void Update(Vector3i position)
        {
            bool success = Context.Player.World.DoRandomUpdate(position);
            if (!success) Context.Console.WriteError("Cannot update at this position.");
        }
    }
}

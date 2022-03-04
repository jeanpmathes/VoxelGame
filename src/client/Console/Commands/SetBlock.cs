// <copyright file="SetBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using JetBrains.Annotations;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Console.Commands
{
    #pragma warning disable CA1822

    /// <summary>
    ///     Sets the block at the target position. Can cause invalid block state.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SetBlock : Command
    {
        /// <inheritdoc />
        public override string Name => "set-block";

        /// <inheritdoc />
        public override string HelpText => "Sets the block at the target position. Can cause invalid block state.";

        /// <exclude />
        public void Invoke(string namedID, int data, int x, int y, int z)
        {
            Set(namedID, data, (x, y, z));
        }

        /// <exclude />
        public void Invoke(string namedID, int data)
        {
            Set(namedID, data, Context.Player.TargetPosition);
        }

        private void Set(string namedID, int data, Vector3i position)
        {
            Block? block = Block.TranslateNamedID(namedID);

            if (block == null)
            {
                Context.Console.WriteError("Cannot find block.");

                return;
            }

            if (data is < 0 or > 0b11_1111)
            {
                Context.Console.WriteError("Invalid data value.");

                return;
            }

            Context.Player.World.SetBlock(block.AsInstance((uint) data), position);
        }
    }
}

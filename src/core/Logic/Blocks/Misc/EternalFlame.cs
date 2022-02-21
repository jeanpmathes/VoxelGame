// <copyright file="EternalFlame.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block that does not stop burning.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class EternalFlame : BasicBlock, IFlammable
    {
        internal EternalFlame(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                BlockFlags.Basic,
                layout) {}

        /// <inheritdoc />
        public bool Burn(World world, Vector3i position, Block fire)
        {
            return false;
        }
    }
}

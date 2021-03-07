// <copyright file="EternalFlame.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that does not stop burning.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class EternalFlame : BasicBlock, IFlammable
    {
        public EternalFlame(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
        }

        public virtual bool Burn(int x, int y, int z, Block fire)
        {
            return false;
        }
    }
}
// <copyright file="EternalFlame.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks.Misc
{
    /// <summary>
    /// A block that does not stop burning.
    /// Data bit usage: <c>-----</c>
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
                isInteractable: false)
        {
        }

        public bool Burn(int x, int y, int z, Block fire)
        {
            return false;
        }
    }
}
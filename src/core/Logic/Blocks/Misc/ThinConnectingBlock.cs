// <copyright file="ThinConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A thin block that connects to other blocks.
    /// Data bit usage: <c>--nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    public class ThinConnectingBlock : ConnectingBlock
    {
        public ThinConnectingBlock(string name, string namedId, string texture, string post, string extension) :
            base(
                name,
                namedId,
                texture,
                post,
                extension,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0625f, 0.5f, 0.0625f)))
        {
        }
    }
}
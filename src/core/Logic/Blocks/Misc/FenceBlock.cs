// <copyright file="FenceBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     This class represents a block which connects to blocks with the <see cref="IWideConnectable" /> interface. The
    ///     texture and indices of the BlockModels are ignored.
    ///     Data bit usage: <c>--nesw</c>
    /// </summary>
    // n: connected north
    // e: connected east
    // s: connected south
    // w: connected west
    public class FenceBlock : WideConnectingBlock, IFlammable
    {
        /// <summary>
        ///     Create a new <see cref="FenceBlock" />.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="namedId">The named ID of the block.</param>
        /// <param name="texture">The texture to apply to the model.</param>
        /// <param name="postModel">The name of the post model. All model textures are ignored.</param>
        /// <param name="extensionModel">The name of the extension model. All model textures are ignored.</param>
        internal FenceBlock(string name, string namedId, string texture, string postModel, string extensionModel) :
            base(
                name,
                namedId,
                texture,
                postModel,
                extensionModel,
                new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                    new Vector3(x: 0.1875f, y: 0.5f, z: 0.1875f))) {}

        /// <inheritdoc />
        protected override BoundingBox GetBoundingBox(uint data)
        {
            bool north = (data & 0b00_1000) != 0;
            bool east = (data & 0b00_0100) != 0;
            bool south = (data & 0b00_0010) != 0;
            bool west = (data & 0b00_0001) != 0;

            int extensions = BitHelper.CountSetBooleans(north, east, south, west);

            var children = new BoundingBox[2 * extensions];
            extensions = 0;

            if (north)
            {
                children[0] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.28125f, z: 0.15625f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.15625f));

                children[1] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.71875f, z: 0.15625f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.15625f));

                extensions += 2;
            }

            if (east)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.84375f, y: 0.28125f, z: 0.5f),
                    new Vector3(x: 0.15625f, y: 0.15625f, z: 0.125f));

                children[extensions + 1] = new BoundingBox(
                    new Vector3(x: 0.84375f, y: 0.71875f, z: 0.5f),
                    new Vector3(x: 0.15625f, y: 0.15625f, z: 0.125f));

                extensions += 2;
            }

            if (south)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.28125f, z: 0.84375f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.15625f));

                children[extensions + 1] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.71875f, z: 0.84375f),
                    new Vector3(x: 0.125f, y: 0.15625f, z: 0.15625f));

                extensions += 2;
            }

            if (west)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.15625f, y: 0.28125f, z: 0.5f),
                    new Vector3(x: 0.15625f, y: 0.15625f, z: 0.125f));

                children[extensions + 1] = new BoundingBox(
                    new Vector3(x: 0.15625f, y: 0.71875f, z: 0.5f),
                    new Vector3(x: 0.15625f, y: 0.15625f, z: 0.125f));
            }

            return new BoundingBox(
                new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3(x: 0.1875f, y: 0.5f, z: 0.1875f),
                children);
        }
    }
}

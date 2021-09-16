// <copyright file="WallBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     This class represents a wall block which connects to blocks with the
    ///     <see cref="VoxelGame.Core.Logic.Interfaces.IWideConnectable" /> interface. When connecting in a straight line, no
    ///     post is used and indices are not ignored, else indices are ignored.
    ///     Data bit usage: <c>--nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class WallBlock : WideConnectingBlock
    {
        private readonly string extensionStraight;
        private float[] extensionStraightXVertices = null!;

        private float[] extensionStraightZVertices = null!;
        private uint[] indicesStraight = null!;
        private uint straightVertexCount;

        private int[] texIndicesStraight = null!;

        internal WallBlock(string name, string namedId, string texture, string post, string extension,
            string extensionStraight) :
            base(
                name,
                namedId,
                texture,
                post,
                extension,
                new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.25f, y: 0.5f, z: 0.25f)))
        {
            this.extensionStraight = extensionStraight;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            base.Setup(indexProvider);

            BlockModel extensionStraightModel = BlockModel.Load(extensionStraight);
            straightVertexCount = (uint) extensionStraightModel.VertexCount;

            extensionStraightModel.RotateY(rotations: 0, rotateTopAndBottomTexture: false);
            extensionStraightModel.ToData(out extensionStraightZVertices, out texIndicesStraight, out indicesStraight);

            extensionStraightModel.RotateY(rotations: 1, rotateTopAndBottomTexture: false);
            extensionStraightModel.ToData(out extensionStraightXVertices, out _, out _);

            int tex = indexProvider.GetTextureIndex(texture);

            for (var i = 0; i < texIndicesStraight.Length; i++) texIndicesStraight[i] = tex;
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            bool north = (data & 0b00_1000) != 0;
            bool east = (data & 0b00_0100) != 0;
            bool south = (data & 0b00_0010) != 0;
            bool west = (data & 0b00_0001) != 0;

            bool straightZ = north && south && !east && !west;
            bool straightX = !north && !south && east && west;

            if (straightZ)
                return new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.46875f, z: 0.5f),
                    new Vector3(x: 0.1875f, y: 0.46875f, z: 0.5f));

            if (straightX)
                return new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.46875f, z: 0.5f),
                    new Vector3(x: 0.5f, y: 0.46875f, z: 0.1875f));

            int extensions = BitHelper.CountSetBits(data & 0b1111);

            BoundingBox[] children = new BoundingBox[extensions];
            extensions = 0;

            if (north)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.46875f, z: 0.125f),
                    new Vector3(x: 0.1875f, y: 0.46875f, z: 0.125f));

                extensions++;
            }

            if (east)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.875f, y: 0.46875f, z: 0.5f),
                    new Vector3(x: 0.125f, y: 0.46875f, z: 0.1875f));

                extensions++;
            }

            if (south)
            {
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.46875f, z: 0.875f),
                    new Vector3(x: 0.1875f, y: 0.46875f, z: 0.125f));

                extensions++;
            }

            if (west)
                children[extensions] = new BoundingBox(
                    new Vector3(x: 0.125f, y: 0.46875f, z: 0.5f),
                    new Vector3(x: 0.125f, y: 0.46875f, z: 0.1875f));

            return new BoundingBox(
                new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3(x: 0.25f, y: 0.5f, z: 0.25f),
                children);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            bool north = (info.Data & 0b00_1000) != 0;
            bool east = (info.Data & 0b00_0100) != 0;
            bool south = (info.Data & 0b00_0010) != 0;
            bool west = (info.Data & 0b00_0001) != 0;

            bool straightZ = north && south && !east && !west;
            bool straightX = !north && !south && east && west;

            if (straightZ || straightX)
                return BlockMeshData.Complex(
                    straightVertexCount,
                    straightZ ? extensionStraightZVertices : extensionStraightXVertices,
                    texIndicesStraight,
                    indicesStraight);

            return base.GetMesh(info);
        }
    }
}
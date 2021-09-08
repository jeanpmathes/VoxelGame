// <copyright file="ConnectingBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A base class for blocks that connect to other blocks as wide connectables, like fences or walls.
    ///     Data bit usage: <c>--nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public abstract class WideConnectingBlock : ConnectingBlock<IWideConnectable>, IWideConnectable
    {
        private readonly string extension;
        private readonly string post;

        private protected readonly string texture;
        private float[] eastVertices = null!;
        private uint extensionVertexCount;

        private uint[][] indices = null!;

        private float[] northVertices = null!;
        private uint postVertexCount;

        private float[] postVertices = null!;
        private float[] southVertices = null!;

        private int[][] textureIndices = null!;
        private float[] westVertices = null!;

        protected WideConnectingBlock(string name, string namedId, string texture, string post, string extension,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                false,
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.texture = texture;
            this.post = post;
            this.extension = extension;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            BlockModel postModel = BlockModel.Load(post);
            BlockModel extensionModel = BlockModel.Load(extension);

            postVertexCount = (uint) postModel.VertexCount;
            extensionVertexCount = (uint) extensionModel.VertexCount;

            postModel.ToData(out postVertices, out _, out _);

            extensionModel.RotateY(0, false);
            extensionModel.ToData(out northVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out eastVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out southVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out westVertices, out _, out _);

            int tex = indexProvider.GetTextureIndex(texture);

            textureIndices = new int[5][];

            for (var i = 0; i < 5; i++)
                textureIndices[i] =
                    BlockModels.GenerateTextureDataArray(tex, postModel.VertexCount + i * extensionModel.VertexCount);

            indices = new uint[5][];

            for (var i = 0; i < 5; i++)
                indices[i] =
                    BlockModels.GenerateIndexDataArray(postModel.Quads.Length + i * extensionModel.Quads.Length);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            bool north = (info.Data & 0b00_1000) != 0;
            bool east = (info.Data & 0b00_0100) != 0;
            bool south = (info.Data & 0b00_0010) != 0;
            bool west = (info.Data & 0b00_0001) != 0;

            int extensions = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
            var vertexCount = (uint) (postVertexCount + extensions * extensionVertexCount);

            float[] vertices = new float[vertexCount * 8];
            int[] currentTextureIndices = textureIndices[extensions];
            uint[] currentIndices = indices[extensions];

            // Combine the required vertices into one array
            var position = 0;
            Array.Copy(postVertices, 0, vertices, 0, postVertices.Length);
            position += postVertices.Length;

            if (north)
            {
                Array.Copy(northVertices, 0, vertices, position, northVertices.Length);
                position += northVertices.Length;
            }

            if (east)
            {
                Array.Copy(eastVertices, 0, vertices, position, eastVertices.Length);
                position += eastVertices.Length;
            }

            if (south)
            {
                Array.Copy(southVertices, 0, vertices, position, southVertices.Length);
                position += southVertices.Length;
            }

            if (west) Array.Copy(westVertices, 0, vertices, position, westVertices.Length);

            return BlockMeshData.Complex(vertexCount, vertices, currentTextureIndices, currentIndices);
        }
    }
}
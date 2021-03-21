// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Client.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Client.Logic
{
    [Serializable]
    public class ClientSection : Core.Logic.Section
    {
        [NonSerialized] private bool hasMesh;
        [NonSerialized] private SectionRenderer? renderer;

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public override void Setup()
        {
            renderer = GLManager.SectionRendererFactory.CreateSectionRenderer();

            hasMesh = false;
            disposed = false;
        }

        public void CreateAndSetMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out SectionMeshData meshData);
            SetMeshData(ref meshData);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out SectionMeshData meshData)
        {
            // Set the neutral tint colors.
            TintColor blockTint = TintColor.Green;
            TintColor liquidTint = TintColor.Blue;

            // Get the sections next to this section.
            var frontNeighbor = Game.World.GetSection(sectionX, sectionY, sectionZ + 1) as ClientSection;
            var backNeighbor = Game.World.GetSection(sectionX, sectionY, sectionZ - 1) as ClientSection;
            var leftNeighbor = Game.World.GetSection(sectionX - 1, sectionY, sectionZ) as ClientSection;
            var rightNeighbor = Game.World.GetSection(sectionX + 1, sectionY, sectionZ) as ClientSection;
            var bottomNeighbor = Game.World.GetSection(sectionX, sectionY - 1, sectionZ) as ClientSection;
            var topNeighbor = Game.World.GetSection(sectionX, sectionY + 1, sectionZ) as ClientSection;

            BlockMeshFaceHolder simpleFrontFaceHolder = new BlockMeshFaceHolder(BlockSide.Front);
            BlockMeshFaceHolder simpleBackFaceHolder = new BlockMeshFaceHolder(BlockSide.Back);
            BlockMeshFaceHolder simpleLeftFaceHolder = new BlockMeshFaceHolder(BlockSide.Left);
            BlockMeshFaceHolder simpleRightFaceHolder = new BlockMeshFaceHolder(BlockSide.Right);
            BlockMeshFaceHolder simpleBottomFaceHolder = new BlockMeshFaceHolder(BlockSide.Bottom);
            BlockMeshFaceHolder simpleTopFaceHolder = new BlockMeshFaceHolder(BlockSide.Top);

            PooledList<float> complexVertexPositions = new PooledList<float>(64);
            PooledList<int> complexVertexData = new PooledList<int>(32);
            PooledList<uint> complexIndices = new PooledList<uint>(16);

            uint complexVertexCount = 0;

            LiquidMeshFaceHolder opaqueLiquidFrontFaceHolder = new LiquidMeshFaceHolder(BlockSide.Front);
            LiquidMeshFaceHolder opaqueLiquidBackFaceHolder = new LiquidMeshFaceHolder(BlockSide.Back);
            LiquidMeshFaceHolder opaqueLiquidLeftFaceHolder = new LiquidMeshFaceHolder(BlockSide.Left);
            LiquidMeshFaceHolder opaqueLiquidRightFaceHolder = new LiquidMeshFaceHolder(BlockSide.Right);
            LiquidMeshFaceHolder opaqueLiquidBottomFaceHolder = new LiquidMeshFaceHolder(BlockSide.Bottom);
            LiquidMeshFaceHolder opaqueLiquidTopFaceHolder = new LiquidMeshFaceHolder(BlockSide.Top);

            LiquidMeshFaceHolder transparentLiquidFrontFaceHolder = new LiquidMeshFaceHolder(BlockSide.Front);
            LiquidMeshFaceHolder transparentLiquidBackFaceHolder = new LiquidMeshFaceHolder(BlockSide.Back);
            LiquidMeshFaceHolder transparentLiquidLeftFaceHolder = new LiquidMeshFaceHolder(BlockSide.Left);
            LiquidMeshFaceHolder transparentLiquidRightFaceHolder = new LiquidMeshFaceHolder(BlockSide.Right);
            LiquidMeshFaceHolder transparentLiquidBottomFaceHolder = new LiquidMeshFaceHolder(BlockSide.Bottom);
            LiquidMeshFaceHolder transparentLiquidTopFaceHolder = new LiquidMeshFaceHolder(BlockSide.Top);

            // Loop through the section
            for (var x = 0; x < SectionSize; x++)
            {
                for (var y = 0; y < SectionSize; y++)
                {
                    for (var z = 0; z < SectionSize; z++)
                    {
                        uint val = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.TranslateID(val & BLOCKMASK);
                        uint data = (val & DATAMASK) >> DATASHIFT;

                        Liquid currentLiquid = Liquid.TranslateID((val & Section.LIQUIDMASK) >> Section.LIQUIDSHIFT);
                        LiquidLevel level = (LiquidLevel)((val & Section.LEVELMASK) >> Section.LEVELSHIFT);
                        bool isStatic = (val & Section.STATICMASK) != 0;
                        bool isFull = level == LiquidLevel.Eight;

                        switch (currentBlock.TargetBuffer)
                        {
                            case TargetBuffer.Simple:
                                {
                                    Block? blockToCheck;

                                    // Check all six sides of this block

                                    // Front
                                    if (z + 1 >= SectionSize && frontNeighbor != null)
                                    {
                                        blockToCheck = frontNeighbor.GetBlock(x, y, 0);
                                    }
                                    else if (z + 1 >= SectionSize)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x, y, z + 1);
                                    }

                                    if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Front, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Front << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleFrontFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    // Back
                                    if (z - 1 < 0 && backNeighbor != null)
                                    {
                                        blockToCheck = backNeighbor.GetBlock(x, y, SectionSize - 1);
                                    }
                                    else if (z - 1 < 0)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x, y, z - 1);
                                    }

                                    if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Back, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Back << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleBackFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    // Left
                                    if (x - 1 < 0 && leftNeighbor != null)
                                    {
                                        blockToCheck = leftNeighbor.GetBlock(SectionSize - 1, y, z);
                                    }
                                    else if (x - 1 < 0)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x - 1, y, z);
                                    }

                                    if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Left, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Left << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleLeftFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    // Right
                                    if (x + 1 >= SectionSize && rightNeighbor != null)
                                    {
                                        blockToCheck = rightNeighbor.GetBlock(0, y, z);
                                    }
                                    else if (x + 1 >= SectionSize)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x + 1, y, z);
                                    }

                                    if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Right, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Right << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleRightFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    // Bottom
                                    if (y - 1 < 0 && bottomNeighbor != null)
                                    {
                                        blockToCheck = bottomNeighbor.GetBlock(x, SectionSize - 1, z);
                                    }
                                    else if (y - 1 < 0)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x, y - 1, z);
                                    }

                                    if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Bottom, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Bottom << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleBottomFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    // Top
                                    if (y + 1 >= SectionSize && topNeighbor != null)
                                    {
                                        blockToCheck = topNeighbor.GetBlock(x, 0, z);
                                    }
                                    else if (y + 1 >= SectionSize)
                                    {
                                        blockToCheck = null;
                                    }
                                    else
                                    {
                                        blockToCheck = GetBlock(x, y + 1, z);
                                    }

                                    if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                                    {
                                        BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.Top, data, currentLiquid));
                                        float[] vertices = mesh.GetVertices();

                                        // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                        int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                        int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                        int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                        int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                        // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)BlockSide.Top << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                        simpleTopFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                    }

                                    break;
                                }
                            case TargetBuffer.Complex:
                                {
                                    BlockMeshData mesh = currentBlock.GetMesh(new BlockMeshInfo(BlockSide.All, data, currentLiquid));
                                    float[] vertices = mesh.GetVertices();
                                    int[] textureIndices = mesh.GetTextureIndices();
                                    uint[] indices = mesh.GetIndices();

                                    complexIndices.AddRange(indices);

                                    for (int i = 0; i < mesh.VertexCount; i++)
                                    {
                                        complexVertexPositions.Add(vertices[(i * 8) + 0] + x);
                                        complexVertexPositions.Add(vertices[(i * 8) + 1] + y);
                                        complexVertexPositions.Add(vertices[(i * 8) + 2] + z);

                                        // int: nnnn nooo oopp ppp- ---- --uu uuuv vvvv (nop: normal; uv: texture coords)
                                        int upperData =
                                            (((vertices[(i * 8) + 5] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 5] * -15f)) : (int)(vertices[(i * 8) + 5] * 15f)) << 27) |
                                            (((vertices[(i * 8) + 6] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 6] * -15f)) : (int)(vertices[(i * 8) + 6] * 15f)) << 22) |
                                            (((vertices[(i * 8) + 7] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 7] * -15f)) : (int)(vertices[(i * 8) + 7] * 15f)) << 17) |
                                            ((int)(vertices[(i * 8) + 3] * 16f) << 5) |
                                            ((int)(vertices[(i * 8) + 4] * 16f));

                                        complexVertexData.Add(upperData);

                                        // int: tttt tttt t--- ---a ---i iiii iiii iiii(t: tint; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | mesh.GetAnimationBit(i, 16) | textureIndices[i];
                                        complexVertexData.Add(lowerData);
                                    }

                                    for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
                                    {
                                        complexIndices[i] += complexVertexCount;
                                    }

                                    complexVertexCount += mesh.VertexCount;
                                    break;
                                }
                        }

                        if (currentLiquid.RenderType != RenderType.NotRendered && ((currentBlock is IFillable fillable && fillable.RenderLiquid) || (currentBlock is not IFillable && !currentBlock.IsSolidAndFull)))
                        {
                            LiquidMeshFaceHolder liquidFrontFaceHolder;
                            LiquidMeshFaceHolder liquidBackFaceHolder;
                            LiquidMeshFaceHolder liquidLeftFaceHolder;
                            LiquidMeshFaceHolder liquidRightFaceHolder;
                            LiquidMeshFaceHolder liquidBottomFaceHolder;
                            LiquidMeshFaceHolder liquidTopFaceHolder;

                            if (currentLiquid.RenderType == RenderType.Opaque)
                            {
                                liquidFrontFaceHolder = opaqueLiquidFrontFaceHolder;
                                liquidBackFaceHolder = opaqueLiquidBackFaceHolder;
                                liquidLeftFaceHolder = opaqueLiquidLeftFaceHolder;
                                liquidRightFaceHolder = opaqueLiquidRightFaceHolder;
                                liquidBottomFaceHolder = opaqueLiquidBottomFaceHolder;
                                liquidTopFaceHolder = opaqueLiquidTopFaceHolder;
                            }
                            else // RenderType.Opaque
                            {
                                liquidFrontFaceHolder = transparentLiquidFrontFaceHolder;
                                liquidBackFaceHolder = transparentLiquidBackFaceHolder;
                                liquidLeftFaceHolder = transparentLiquidLeftFaceHolder;
                                liquidRightFaceHolder = transparentLiquidRightFaceHolder;
                                liquidBottomFaceHolder = transparentLiquidBottomFaceHolder;
                                liquidTopFaceHolder = transparentLiquidTopFaceHolder;
                            }

                            Liquid? liquidToCheck;
                            Block? blockToCheck;
                            int sideHeight = -1;
                            bool isNeighbourLiquidMeshed;

                            // Front.
                            if (z + 1 >= SectionSize && frontNeighbor != null)
                            {
                                liquidToCheck = frontNeighbor.GetLiquid(x, y, 0, out sideHeight);
                                blockToCheck = frontNeighbor.GetBlock(x, y, 0);
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y, z + 1, out sideHeight);
                                blockToCheck = GetBlock(x, y, z + 1);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable frontFillable && frontFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Front, isStatic));

                                bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Front << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidFrontFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Back.
                            if (z - 1 < 0 && backNeighbor != null)
                            {
                                liquidToCheck = backNeighbor.GetLiquid(x, y, SectionSize - 1, out sideHeight);
                                blockToCheck = backNeighbor.GetBlock(x, y, SectionSize - 1);
                            }
                            else if (z - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y, z - 1, out sideHeight);
                                blockToCheck = GetBlock(x, y, z - 1);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable backFillable && backFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Back, isStatic));

                                bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Back << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidBackFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Left.
                            if (x - 1 < 0 && leftNeighbor != null)
                            {
                                liquidToCheck = leftNeighbor.GetLiquid(SectionSize - 1, y, z, out sideHeight);
                                blockToCheck = leftNeighbor.GetBlock(SectionSize - 1, y, z);
                            }
                            else if (x - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x - 1, y, z, out sideHeight);
                                blockToCheck = GetBlock(x - 1, y, z);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable leftFillable && leftFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Left, isStatic));

                                bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Left << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidLeftFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Right.
                            if (x + 1 >= SectionSize && rightNeighbor != null)
                            {
                                liquidToCheck = rightNeighbor.GetLiquid(0, y, z, out sideHeight);
                                blockToCheck = rightNeighbor.GetBlock(0, y, z);
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x + 1, y, z, out sideHeight);
                                blockToCheck = GetBlock(x + 1, y, z);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable rightFillable && rightFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Right, isStatic));

                                bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Right << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidRightFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Bottom.
                            if (y - 1 < 0 && bottomNeighbor != null)
                            {
                                liquidToCheck = bottomNeighbor.GetLiquid(x, SectionSize - 1, z, out sideHeight);
                                blockToCheck = bottomNeighbor.GetBlock(x, SectionSize - 1, z);
                            }
                            else if (y - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y - 1, z, out sideHeight);
                                blockToCheck = GetBlock(x, y - 1, z);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable bottomFillable && bottomFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((currentLiquid.Direction > 0 && sideHeight != 7 && blockToCheck?.IsOpaque != true) || (currentLiquid.Direction < 0 && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))))
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Bottom, isStatic));

                                bool singleSided = ((currentLiquid.Direction > 0 || level == LiquidLevel.Eight) &&
                                                    blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Bottom << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidBottomFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Top.
                            if (y + 1 >= SectionSize && topNeighbor != null)
                            {
                                liquidToCheck = topNeighbor.GetLiquid(x, 0, z, out sideHeight);
                                blockToCheck = topNeighbor.GetBlock(x, 0, z);
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y + 1, z, out sideHeight);
                                blockToCheck = GetBlock(x, y + 1, z);
                            }

                            isNeighbourLiquidMeshed = blockToCheck is IFillable topFillable && topFillable.RenderLiquid;

                            if (liquidToCheck != currentLiquid || !isNeighbourLiquidMeshed) sideHeight = -1;

                            if ((currentLiquid.Direction < 0 && sideHeight != 7 && blockToCheck?.IsOpaque != true) || (currentLiquid.Direction > 0 && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))))
                            {
                                LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, BlockSide.Top, isStatic));

                                bool singleSided = ((currentLiquid.Direction < 0 || level == LiquidLevel.Eight) &&
                                                    blockToCheck?.IsOpaque == false &&
                                                    blockToCheck?.IsSolidAndFull == true);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)BlockSide.Top << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidTopFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }
                        }
                    }
                }
            }

            // Build the simple mesh data.
            PooledList<int> simpleVertexData = new PooledList<int>(4096);

            simpleFrontFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBackFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleLeftFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleRightFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBottomFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleTopFaceHolder.GenerateMesh(ref simpleVertexData);

            // Build the liquid mesh data.
            PooledList<int> opaqueLiquidVertexData = new PooledList<int>();
            PooledList<uint> opaqueLiquidIndices = new PooledList<uint>();
            uint opaqueLiquidVertCount = 0;

            opaqueLiquidFrontFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);
            opaqueLiquidBackFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);
            opaqueLiquidLeftFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);
            opaqueLiquidRightFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);
            opaqueLiquidBottomFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);
            opaqueLiquidTopFaceHolder.GenerateMesh(ref opaqueLiquidVertexData, ref opaqueLiquidVertCount, ref opaqueLiquidIndices);

            PooledList<int> transparentLiquidVertexData = new PooledList<int>();
            PooledList<uint> transparentLiquidIndices = new PooledList<uint>();
            uint transparentLiquidVertCount = 0;

            transparentLiquidFrontFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);
            transparentLiquidBackFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);
            transparentLiquidLeftFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);
            transparentLiquidRightFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);
            transparentLiquidBottomFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);
            transparentLiquidTopFaceHolder.GenerateMesh(ref transparentLiquidVertexData, ref transparentLiquidVertCount, ref transparentLiquidIndices);

            hasMesh = complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 || opaqueLiquidVertexData.Count != 0 || transparentLiquidVertexData.Count != 0;

            meshData = new SectionMeshData(ref simpleVertexData, ref complexVertexPositions, ref complexVertexData, ref complexIndices, ref opaqueLiquidVertexData, ref opaqueLiquidIndices, ref transparentLiquidVertexData, ref transparentLiquidIndices);

            simpleFrontFaceHolder.ReturnToPool();
            simpleBackFaceHolder.ReturnToPool();
            simpleLeftFaceHolder.ReturnToPool();
            simpleRightFaceHolder.ReturnToPool();
            simpleBottomFaceHolder.ReturnToPool();
            simpleTopFaceHolder.ReturnToPool();

            opaqueLiquidFrontFaceHolder.ReturnToPool();
            opaqueLiquidBackFaceHolder.ReturnToPool();
            opaqueLiquidLeftFaceHolder.ReturnToPool();
            opaqueLiquidRightFaceHolder.ReturnToPool();
            opaqueLiquidBottomFaceHolder.ReturnToPool();
            opaqueLiquidTopFaceHolder.ReturnToPool();

            transparentLiquidFrontFaceHolder.ReturnToPool();
            transparentLiquidBackFaceHolder.ReturnToPool();
            transparentLiquidLeftFaceHolder.ReturnToPool();
            transparentLiquidRightFaceHolder.ReturnToPool();
            transparentLiquidBottomFaceHolder.ReturnToPool();
            transparentLiquidTopFaceHolder.ReturnToPool();
        }

        public void SetMeshData(ref SectionMeshData meshData)
        {
            renderer?.SetData(ref meshData);
        }

        public void PrepareRender(int stage)
        {
            renderer?.PrepareStage(stage);
        }

        public void FinishRender(int stage)
        {
            renderer?.FinishStage(stage);
        }

        public void Render(int stage, Vector3 position)
        {
            if (hasMesh)
            {
                renderer?.DrawStage(stage, position);
            }
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    renderer?.Dispose();
                }

                disposed = true;
            }
        }

        #endregion IDisposable Support
    }
}
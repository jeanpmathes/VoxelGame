// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A section of the world, specifically for the client.
///     Sections do not know their exact position in the world.
/// </summary>
[Serializable]
public class ClientSection : Section
{
    private static long runCount;
    private static long runTime;
    [NonSerialized] private bool hasMesh;
    [NonSerialized] private SectionRenderer? renderer;

    /// <summary>
    ///     Create a new client section.
    /// </summary>
    /// <param name="world">The world containing the client section.</param>
    public ClientSection(World world) : base(world) {}

    /// <inheritdoc />
    protected override void Setup()
    {
        renderer = new SectionRenderer();

        hasMesh = false;
        disposed = false;
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    public void CreateAndSetMesh(SectionPosition position)
    {
        SectionMeshData meshData = CreateMeshData(position);
        SetMeshData(meshData);
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(SectionPosition position)
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();

        SectionMeshData result = CreateMeshData_Untimed(position);

        stopwatch.Stop();

        runCount++;
        runTime += stopwatch.ElapsedMilliseconds;

        if (runCount % 1000 == 0) System.Console.WriteLine($"Average run time: {runTime / (float) runCount}ms");

        return result;
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The created mesh data.</returns>
    [SuppressMessage(
        "Blocker Code Smell",
        "S2437:Silly bit operations should not be performed",
        Justification = "Improves readability.")]
    private SectionMeshData CreateMeshData_Untimed(SectionPosition position)
    {
        MeshingContext context = new(this, position, World);
        context.BlockTint = TintColor.Green;
        context.FluidTint = TintColor.Blue;

        VaryingHeightMeshFaceHolder[] opaqueFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        VaryingHeightMeshFaceHolder[] transparentFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        ClientSection?[] neighbors = GetNeighborSections(position);

        // Loop through the section
        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            uint val = blocks[(x << SizeExp2) + (y << SizeExp) + z];

            Decode(
                val,
                out Block currentBlock,
                out uint data,
                out Fluid currentFluid,
                out FluidLevel level,
                out bool isStatic);

            var pos = new Vector3i(x, y, z);
            bool isFull = level == FluidLevel.Eight;

            IBlockMeshable meshable = currentBlock;
            meshable.CreateMesh((x, y, z), new BlockMeshInfo(BlockSide.All, data, currentFluid), context);

            if (currentFluid.RenderType != RenderType.NotRendered &&
                (currentBlock is IFillable { RenderFluid: true } ||
                 currentBlock is not IFillable && !currentBlock.IsSolidAndFull))
            {
                VaryingHeightMeshFaceHolder[] fluidMeshFaceHolders =
                    currentFluid.RenderType == RenderType.Opaque
                        ? opaqueFluidMeshFaceHolders
                        : transparentFluidMeshFaceHolders;

                MeshFluidSide(BlockSide.Front);
                MeshFluidSide(BlockSide.Back);
                MeshFluidSide(BlockSide.Left);
                MeshFluidSide(BlockSide.Right);
                MeshFluidSide(BlockSide.Bottom);
                MeshFluidSide(BlockSide.Top);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void MeshFluidSide(BlockSide side)
                {
                    ClientSection? neighbor = neighbors[(int) side];

                    Fluid? fluidToCheck;
                    Block? blockToCheck;

                    Vector3i checkPos = side.Offset(pos);

                    int sideHeight = -1;
                    bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

                    if (IsPositionOutOfSection(checkPos))
                    {
                        checkPos = checkPos.Mod(Size);

                        fluidToCheck = neighbor?.GetFluid(checkPos, out sideHeight);
                        blockToCheck = neighbor?.GetBlock(checkPos);
                    }
                    else
                    {
                        fluidToCheck = GetFluid(checkPos, out sideHeight);
                        blockToCheck = GetBlock(checkPos);
                    }

                    bool isNeighborFluidMeshed =
                        blockToCheck is IFillable { RenderFluid: true };

                    if (fluidToCheck != currentFluid || !isNeighborFluidMeshed) sideHeight = -1;

                    bool flowsTowardsFace = side == BlockSide.Top
                        ? currentFluid.Direction == VerticalFlow.Upwards
                        : currentFluid.Direction == VerticalFlow.Downwards;

                    bool meshAtNormal = (int) level > sideHeight && blockToCheck?.IsOpaque != true;

                    bool meshAtEnd =
                        flowsTowardsFace && sideHeight != 7 && blockToCheck?.IsOpaque != true
                        || !flowsTowardsFace && (level != FluidLevel.Eight ||
                                                 fluidToCheck != currentFluid &&
                                                 blockToCheck?.IsOpaque != true);

                    if (atVerticalEnd ? !meshAtEnd : !meshAtNormal) return;

                    FluidMeshData mesh =
                        currentFluid.GetMesh(FluidMeshInfo.Fluid(level, side, isStatic));

                    bool singleSided = blockToCheck?.IsOpaque == false &&
                                       blockToCheck.IsSolidAndFull;

                    side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                    // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                    int upperDataA = (0 << 31) | (0 << 30) | ((x + a[0]) << 10) | (a[1] << 9) | (y << 5) |
                                     (z + a[2]);

                    int upperDataB = (0 << 31) | (1 << 30) | ((x + b[0]) << 10) | (b[1] << 9) | (y << 5) |
                                     (z + b[2]);

                    int upperDataC = (1 << 31) | (1 << 30) | ((x + c[0]) << 10) | (c[1] << 9) | (y << 5) |
                                     (z + c[2]);

                    int upperDataD = (1 << 31) | (0 << 30) | ((x + d[0]) << 10) | (d[1] << 9) | (y << 5) |
                                     (z + d[2]);

                    // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                    int lowerData = (mesh.Tint.GetBits(context.FluidTint) << 23) | ((int) side << 16) |
                                    ((sideHeight + 1) << 12) |
                                    (currentFluid.Direction.GetBit() << 11) | ((int) level << 8) |
                                    (isStatic ? 1 << 7 : 0 << 7) |
                                    ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                    fluidMeshFaceHolders[(int) side].AddFace(
                        pos,
                        lowerData,
                        (upperDataA, upperDataB, upperDataC, upperDataD),
                        singleSided,
                        isFull);
                }
            }
        }

        SectionMeshData meshData = context.GenerateMeshData(
            opaqueFluidMeshFaceHolders,
            transparentFluidMeshFaceHolders);

        hasMesh = meshData.IsFilled;

        context.ReturnToPool(opaqueFluidMeshFaceHolders, transparentFluidMeshFaceHolders);

        return meshData;
    }

    private ClientSection?[] GetNeighborSections(SectionPosition position)
    {
        var neighbors = new ClientSection?[6];

        foreach (BlockSide side in BlockSide.All.Sides())
            neighbors[(int) side] =
                World.GetSection(side.Offset(position)) as ClientSection;

        return neighbors;
    }

    private static bool IsPositionOutOfSection(Vector3i position)
    {
        return position.X is < 0 or >= Size || position.Y is < 0 or >= Size ||
               position.Z is < 0 or >= Size;
    }

    private static VaryingHeightMeshFaceHolder[] CreateVaryingHeightMeshFaceHolders()
    {
        var holders = new VaryingHeightMeshFaceHolder[6];

        holders[(int) BlockSide.Front] = new VaryingHeightMeshFaceHolder(BlockSide.Front);
        holders[(int) BlockSide.Back] = new VaryingHeightMeshFaceHolder(BlockSide.Back);
        holders[(int) BlockSide.Left] = new VaryingHeightMeshFaceHolder(BlockSide.Left);
        holders[(int) BlockSide.Right] = new VaryingHeightMeshFaceHolder(BlockSide.Right);
        holders[(int) BlockSide.Bottom] = new VaryingHeightMeshFaceHolder(BlockSide.Bottom);
        holders[(int) BlockSide.Top] = new VaryingHeightMeshFaceHolder(BlockSide.Top);

        return holders;
    }

    /// <summary>
    ///     Set the mesh data for this section. The mesh must be generated from this section.
    /// </summary>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(SectionMeshData meshData)
    {
        Debug.Assert(renderer != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        renderer.SetData(meshData);
    }

    /// <summary>
    ///     Render this section.
    /// </summary>
    /// <param name="stage">The current render stage.</param>
    /// <param name="position">The position of this section in world coordinates.</param>
    public void Render(int stage, Vector3 position)
    {
        if (hasMesh) renderer?.DrawStage(stage, position);
    }

    #region IDisposable Support

    [NonSerialized] private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) renderer?.Dispose();

            disposed = true;
        }
    }

    #endregion IDisposable Support
}

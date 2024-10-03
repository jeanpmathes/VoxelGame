// <copyright file="MeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

#pragma warning disable S4049

/// <summary>
///     The context for section meshing.
/// </summary>
public class MeshingContext
{
    private readonly IMeshing basicOpaqueMeshing;
    private readonly IMeshing basicTransparentMeshing;
    private readonly IMeshing foliageMeshing;
    private readonly IMeshing fluidMeshing;

    private readonly Section current;
    private readonly Sides<Section?> neighbors;

    private readonly Sides<MeshFaceHolder> opaqueFullBlockMeshFaceHolders;
    private readonly Sides<MeshFaceHolder> transparentFullBlockMeshFaceHolders;

    private readonly Sides<MeshFaceHolder> opaqueVaryingHeightBlockMeshFaceHolders;
    private readonly Sides<MeshFaceHolder> transparentVaryingHeightBlockMeshFaceHolders;
    private readonly Sides<MeshFaceHolder> fluidMeshFaceHolders;

    private readonly (TintColor block, TintColor fluid)[,] tintColors;

    /// <summary>
    ///     Create a new block meshing context.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="context">The chunk meshing context of the chunk the section is in.</param>
    public MeshingContext(SectionPosition position, IChunkMeshingContext context)
    {
        Section? section = context.GetSection(position);
        Debug.Assert(section != null);
        current = section;

        IMeshingFactory factory = context.MeshingFactory;
        basicOpaqueMeshing = factory.Create(hint: 1024);
        basicTransparentMeshing = factory.Create(hint: 1024);
        foliageMeshing = factory.Create(hint: 1024);
        fluidMeshing = factory.Create(hint: 1024);

        neighbors = GetNeighborSections(position, context);
        tintColors = GetTintColors(position, context);

        opaqueFullBlockMeshFaceHolders = CreateMeshFaceHolders();
        transparentFullBlockMeshFaceHolders = CreateMeshFaceHolders();

        opaqueVaryingHeightBlockMeshFaceHolders = CreateMeshFaceHolders();
        transparentVaryingHeightBlockMeshFaceHolders = CreateMeshFaceHolders();

        fluidMeshFaceHolders = CreateMeshFaceHolders(inset: 0.001f);
    }

    /// <summary>
    ///     Get current block tint, used when the tint is set to neutral.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    public TintColor GetBlockTint(Vector3i position)
    {
        return tintColors[position.X, position.Z].block;
    }

    /// <summary>
    ///     Get current fluid tint, used when the tint is set to neutral.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    public TintColor GetFluidTint(Vector3i position)
    {
        return tintColors[position.X, position.Z].fluid;
    }

    private static Sides<Section?> GetNeighborSections(SectionPosition position, IChunkMeshingContext context)
    {
        Sides<Section?> neighborSections = new();

        foreach (BlockSide side in BlockSide.All.Sides())
            neighborSections[side] = context.GetSection(side.Offset(position));

        return neighborSections;
    }

    private static (TintColor block, TintColor fluid)[,] GetTintColors(SectionPosition position, IChunkMeshingContext context)
    {
        var colors = new (TintColor block, TintColor fluid)[Section.Size, Section.Size];

        for (var x = 0; x < Section.Size; x++)
        for (var z = 0; z < Section.Size; z++)
            colors[x, z] = context.GetPositionTint(position.FirstBlock + new Vector3i(x, y: 0, z));

        return colors;
    }

    private static Sides<MeshFaceHolder> CreateMeshFaceHolders(Single inset = 0.0f)
    {
        Sides<MeshFaceHolder> holders = new();

        foreach (BlockSide side in BlockSide.All.Sides()) holders[side] = new MeshFaceHolder(side, inset);

        return holders;
    }

    /// <summary>
    ///     Get the meshing object for the basic mesh.
    /// </summary>
    /// <param name="isOpaque">Whether the mesh is opaque or not.</param>
    /// <returns>The meshing object.</returns>
    public IMeshing GetBasicMesh(Boolean isOpaque)
    {
        return isOpaque ? basicOpaqueMeshing : basicTransparentMeshing;
    }

    /// <summary>
    ///     Get the block mesh face holder for (full) blocks.
    ///     This considers the side and whether it is opaque or not.
    /// </summary>
    public MeshFaceHolder GetFullBlockMeshFaceHolder(BlockSide side, Boolean isOpaque)
    {
        return isOpaque ? opaqueFullBlockMeshFaceHolders[side] : transparentFullBlockMeshFaceHolders[side];
    }

    /// <summary>
    ///     Get the block mesh face holder for varying height faces, given a block side.
    ///     This considers the side and whether it is opaque or not.
    /// </summary>
    public MeshFaceHolder GetVaryingHeightBlockMeshFaceHolder(BlockSide side, Boolean isOpaque)
    {
        return isOpaque ? opaqueVaryingHeightBlockMeshFaceHolders[side] : transparentVaryingHeightBlockMeshFaceHolders[side];
    }

    /// <summary>
    ///     Get the fluid mesh face holders for varying height faces.
    /// </summary>
    public Sides<MeshFaceHolder> GetFluidMeshFaceHolders()
    {
        return fluidMeshFaceHolders;
    }

    /// <summary>
    ///     Get the foliage meshing object.
    /// </summary>
    public IMeshing GetFoliageMesh()
    {
        return foliageMeshing;
    }

    /// <summary>
    ///     Get a block and fluid from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block and fluid or null if there is nothing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (BlockInstance block, FluidInstance fluid)? GetBlockAndFluid(Vector3i position, BlockSide side)
    {
        (BlockInstance block, FluidInstance fluid)? result;

        if (Section.IsInBounds(position.ToTuple()))
        {
            BlockInstance block = current.GetBlock(position);
            FluidInstance fluid = current.GetFluid(position);

            result = (block, fluid);
        }
        else
        {
            position = Section.ToLocalPosition(position);

            Section? neighbor = neighbors[side];
            BlockInstance? block = neighbor?.GetBlock(position);
            FluidInstance? fluid = neighbor?.GetFluid(position);

            result = neighbor != null ? (block!.Value, fluid!.Value) : null;
        }

        return result;
    }

    /// <summary>
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block or null if there is no block.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance? GetBlock(Vector3i position, BlockSide side)
    {
        BlockInstance? block;

        if (Section.IsInBounds(position.X, position.Y, position.Z))
        {
            block = current.GetBlock(position);
        }
        else
        {
            position = Section.ToLocalPosition(position);

            Section? neighbor = neighbors[side];
            block = neighbor?.GetBlock(position);
        }

        return block;
    }

    /// <summary>
    ///     Generate the section mesh data.
    /// </summary>
    public SectionMeshData GenerateMeshData()
    {
        // We build the mesh data for everything except complex meshes, as they are already in the correct format.

        GenerateMesh(opaqueFullBlockMeshFaceHolders, basicOpaqueMeshing);
        GenerateMesh(transparentFullBlockMeshFaceHolders, basicTransparentMeshing);

        GenerateMesh(opaqueVaryingHeightBlockMeshFaceHolders, basicOpaqueMeshing);
        GenerateMesh(transparentVaryingHeightBlockMeshFaceHolders, basicTransparentMeshing);

        GenerateMesh(fluidMeshFaceHolders, fluidMeshing);

        return new SectionMeshData
        {
            BasicMeshing = (basicOpaqueMeshing, basicTransparentMeshing),
            FoliageMeshing = foliageMeshing,
            FluidMeshing = fluidMeshing
        };
    }

    /// <summary>
    ///     Return all pooled resources.
    /// </summary>
    public void ReturnToPool()
    {
        ReturnToPool(opaqueFullBlockMeshFaceHolders);
        ReturnToPool(transparentFullBlockMeshFaceHolders);

        ReturnToPool(opaqueVaryingHeightBlockMeshFaceHolders);
        ReturnToPool(transparentVaryingHeightBlockMeshFaceHolders);

        ReturnToPool(fluidMeshFaceHolders);
    }

    private static void GenerateMesh(Sides<MeshFaceHolder> holders, IMeshing meshing)
    {
        foreach (MeshFaceHolder holder in holders) holder.GenerateMesh(meshing);
    }

    private static void ReturnToPool(Sides<MeshFaceHolder> holders)
    {
        foreach (MeshFaceHolder holder in holders) holder.ReturnToPool();
    }
}

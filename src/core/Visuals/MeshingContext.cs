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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;

namespace VoxelGame.Core.Visuals;

#pragma warning disable S4049

/// <summary>
///     The context for section meshing.
/// </summary>
public class MeshingContext
{
    private readonly IMeshing basicOpaqueMeshing;
    private readonly IMeshing basicTransparentMeshing;
    private readonly (ColorS block, ColorS fluid)[,] colors;
    private readonly Section current;
    private readonly SideArray<MeshFaceHolder> fluidMeshFaceHolders;
    private readonly IMeshing fluidMeshing;
    private readonly IMeshing foliageMeshing;
    private readonly SideArray<Section?> neighbors;
    private readonly SideArray<MeshFaceHolder> opaqueFullBlockMeshFaceHolders;
    private readonly SideArray<MeshFaceHolder> opaqueVaryingHeightBlockMeshFaceHolders;
    private readonly SideArray<MeshFaceHolder> transparentFullBlockMeshFaceHolders;
    private readonly SideArray<MeshFaceHolder> transparentVaryingHeightBlockMeshFaceHolders;

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
        colors = GetTintColors(position, context);

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
    public ColorS GetBlockTint(Vector3i position)
    {
        return colors[position.X, position.Z].block;
    }

    /// <summary>
    ///     Get current fluid tint, used when the tint is set to neutral.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    public ColorS GetFluidTint(Vector3i position)
    {
        return colors[position.X, position.Z].fluid;
    }

    private static SideArray<Section?> GetNeighborSections(SectionPosition position, IChunkMeshingContext context)
    {
        SideArray<Section?> neighborSections = new();

        foreach (Side side in Side.All.Sides())
            neighborSections[side] = context.GetSection(side.Offset(position));

        return neighborSections;
    }

    private static (ColorS block, ColorS fluid)[,] GetTintColors(SectionPosition position, IChunkMeshingContext context)
    {
        var colors = new (ColorS block, ColorS fluid)[Section.Size, Section.Size];

        for (var x = 0; x < Section.Size; x++)
        for (var z = 0; z < Section.Size; z++)
            colors[x, z] = context.GetPositionTint(position.FirstBlock + new Vector3i(x, y: 0, z));

        return colors;
    }

    private static SideArray<MeshFaceHolder> CreateMeshFaceHolders(Single inset = 0.0f)
    {
        SideArray<MeshFaceHolder> holders = new();

        foreach (Side side in Side.All.Sides()) holders[side] = new MeshFaceHolder(side, inset);

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
    public MeshFaceHolder GetFullBlockMeshFaceHolder(Side side, Boolean isOpaque)
    {
        return isOpaque ? opaqueFullBlockMeshFaceHolders[side] : transparentFullBlockMeshFaceHolders[side];
    }

    /// <summary>
    ///     Get the block mesh face holder for varying height faces, given a block side.
    ///     This considers the side and whether it is opaque or not.
    /// </summary>
    public MeshFaceHolder GetVaryingHeightBlockMeshFaceHolder(Side side, Boolean isOpaque)
    {
        return isOpaque ? opaqueVaryingHeightBlockMeshFaceHolders[side] : transparentVaryingHeightBlockMeshFaceHolders[side];
    }

    /// <summary>
    ///     Get the fluid mesh face holders for varying height faces.
    /// </summary>
    public SideArray<MeshFaceHolder> GetFluidMeshFaceHolders()
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
    public (State block, FluidInstance fluid)? GetBlockAndFluid(Vector3i position, Side side)
    {
        (State block, FluidInstance fluid)? result;

        if (Section.IsInBounds((position.X, position.Y, position.Z)))
        {
            State block = current.GetBlock(position);
            FluidInstance fluid = current.GetFluid(position);

            result = (block, fluid);
        }
        else
        {
            position = Section.ToLocalPosition(position);

            Section? neighbor = neighbors[side];
            State? block = neighbor?.GetBlock(position);
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
    public State? GetBlock(Vector3i position, Side side)
    {
        State? block;

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

    private static void GenerateMesh(SideArray<MeshFaceHolder> holders, IMeshing meshing)
    {
        foreach (MeshFaceHolder holder in holders) holder.GenerateMesh(meshing);
    }

    private static void ReturnToPool(SideArray<MeshFaceHolder> holders)
    {
        foreach (MeshFaceHolder holder in holders) holder.ReturnToPool();
    }
}

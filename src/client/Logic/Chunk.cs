﻿// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A chunk of the world, specifically for the client.
/// </summary>
[Serializable]
public partial class Chunk : Core.Logic.Chunk
{
    private const int MaxMeshDataStep = 16;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    [NonSerialized] private bool hasMeshData;
    [NonSerialized] private int meshDataIndex;
    [NonSerialized] private BlockSides meshedSides;

    /// <summary>
    ///     Create a new client chunk.
    /// </summary>
    /// <param name="world">The world that contains the chunk.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="context">The context of the chunk.</param>
    public Chunk(Core.Logic.World world, ChunkPosition position, ChunkContext context) : base(world, position, context, CreateSection) {}

    /// <summary>
    ///     Get the client world this chunk is in.
    /// </summary>
    public new World World => base.World.Cast();

    private Section GetSection(int index)
    {
        return GetSectionByIndex(index).Cast();
    }

    /// <summary>
    ///     Begin meshing the chunk.
    /// </summary>
    public void BeginMeshing()
    {
        if (!IsFullyDecorated) return;

        State.RequestNextState<Meshing>(new ChunkState.RequestDescription
        {
            AllowDuplicateTypes = false,
            AllowSkipOnDeactivation = true,
            AllowDiscardOnLoop = false
        });
    }

    private static Core.Logic.Section CreateSection()
    {
        return new Section();
    }

    /// <summary>
    ///     Create a mesh for a section of this chunk and activate it.
    ///     This method should only be called from the main thread.
    /// </summary>
    /// <param name="x">The x position of the section relative in this chunk.</param>
    /// <param name="y">The y position of the section relative in this chunk.</param>
    /// <param name="z">The z position of the section relative in this chunk.</param>
    /// <param name="context">The chunk meshing context.</param>
    public void CreateAndSetMesh(int x, int y, int z, ChunkMeshingContext context)
    {
        GetSection(LocalSectionToIndex(x, y, z)).CreateAndSetMesh(
            SectionPosition.From(Position, (x, y, z)),
            context);
    }

    /// <summary>
    ///     Process a chance to mesh the entire chunk.
    /// </summary>
    /// <returns>A target state if the chunk would like to mesh, null otherwise.</returns>
    public ChunkState? ProcessMeshingOption()
    {
        BlockSides sides = ChunkMeshingContext.DetermineImprovementSides(this, meshedSides);

        if (sides == BlockSides.None) return null;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            BlockSides current = side.ToFlag();

            if (!sides.HasFlag(current) || meshedSides.HasFlag(current) || !World.TryGetChunk(side.Offset(Position), out Core.Logic.Chunk? chunk)) continue;

            chunk.Cast().BeginMeshing();
        }

        return new Meshing();
    }

    /// <inheritdoc />
    protected override void OnActivation()
    {
        RecreateIncompleteSectionMeshes();
    }

    /// <inheritdoc />
    protected override void OnNeighborActivation(Core.Logic.Chunk neighbor)
    {
        RecreateIncompleteSectionMeshes();
    }

    private void RecreateIncompleteSectionMeshes()
    {
        ChunkMeshingContext context = ChunkMeshingContext.UsingActive(this);

        for (var s = 0; s < SectionCount; s++)
        {
            (int x, int y, int z) = IndexToLocalSection(s);
            GetSection(s).RecreateIncompleteMesh(SectionPosition.From(Position, (x, y, z)), context);
        }
    }

    /// <summary>
    ///     Start a task that will create mesh data for this chunk.
    /// </summary>
    /// <param name="context">The chunk meshing context.</param>
    /// <returns>The meshing task.</returns>
    public Task<ChunkMeshData> CreateMeshDataAsync(ChunkMeshingContext context)
    {
        return Task.Run(() => CreateMeshData(context));
    }

    private ChunkMeshData CreateMeshData(ChunkMeshingContext context)
    {
        logger.LogDebug(Events.ChunkOperation, "Started creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", Position, context.AvailableSides.ToCompactString());

        var sectionMeshes = new SectionMeshData[SectionCount];

        for (var s = 0; s < SectionCount; s++)
        {
            (int x, int y, int z) = IndexToLocalSection(s);
            sectionMeshes[s] = GetSection(s).CreateMeshData(SectionPosition.From(Position, (x, y, z)), context);
        }

        meshDataIndex = 0;

        logger.LogDebug(Events.ChunkOperation, "Finished creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", Position, context.AvailableSides.ToCompactString());

        return new ChunkMeshData(sectionMeshes, context.AvailableSides);
    }

    /// <summary>
    ///     Do a mesh data set-step. This will apply a part of the mesh data and activate the part.
    /// </summary>
    /// <param name="meshData">The mesh data to apply.</param>
    /// <returns>True if this step was the final step.</returns>
    public bool DoMeshDataSetStep(ChunkMeshData meshData)
    {
        hasMeshData = false;
        meshedSides = meshData.Sides;

        for (var count = 0; count < MaxMeshDataStep; count++)
        {
            GetSection(meshDataIndex).SetMeshData(meshData.SectionMeshData[meshDataIndex]);

            // The index has reached the end, all sections have received their mesh data.
            if (meshDataIndex == SectionCount - 1)
            {
                hasMeshData = true;
                meshDataIndex = 0;

                return true;
            }

            meshDataIndex++;
        }

        return false;
    }

    /// <summary>
    ///     Adds all sections inside of the frustum to the render list.
    /// </summary>
    /// <param name="frustum">The view frustum to use for culling.</param>
    /// <param name="renderList">The list to add the chunks and positions too.</param>
    public void AddCulledToRenderList(Frustum frustum,
        ICollection<(Section section, Vector3d position)> renderList)
    {
        Box3d chunkBox = VMath.CreateBox3(ChunkPoint, ChunkExtents);

        if (!hasMeshData || !frustum.IsBoxVisible(chunkBox)) return;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            SectionPosition sectionPosition = SectionPosition.From(Position, (x, y, z));
            Vector3d position = sectionPosition.FirstBlock;

            Box3d sectionBox = VMath.CreateBox3(position + Core.Logic.Section.Extents, Core.Logic.Section.Extents);

            if (!frustum.IsBoxVisible(sectionBox)) continue;

            renderList.Add((GetSection(LocalSectionToIndex(x, y, z)), position));
        }
    }
}
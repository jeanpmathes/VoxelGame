// <copyright file="SectionMeshing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Data;
using VoxelGame.Logging;
using Chunk = VoxelGame.Client.Logic.Chunks.Chunk;

namespace VoxelGame.Client.Logic;

/// <summary>
///     Handles meshing of individual sections in the world.
/// </summary>
public partial class SectionMeshing : WorldComponent
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionMeshing>();

    #endregion LOGGING

    private readonly HashSet<(Chunk chunk, (Int32 x, Int32 y, Int32 z))> sectionsToMesh = [];

    [Constructible]
    private SectionMeshing(Core.Logic.World subject) : base(subject)
    {
        Subject.SectionChanged += (_, args) =>
        {
            EnqueueMeshingForAllAffectedSections(args.Chunk, args.Position);
        };
    }

    /// <inheritdoc />
    public override void OnLogicUpdateInActiveState(Double deltaTime, Timer? updateTimer)
    {
        using (logger.BeginTimedSubScoped("Section Meshing", updateTimer))
        {
            MeshAndClearSectionList();
        }
    }

    private void MeshAndClearSectionList()
    {
        foreach ((Chunk chunk, (Int32 x, Int32 y, Int32 z)) in sectionsToMesh)
        {
            using ChunkMeshingContext context = ChunkMeshingContext.UsingActive(chunk, SpatialMeshingFactory.Shared);
            chunk.CreateAndSetMesh(x, y, z, context);
        }

        sectionsToMesh.Clear();
    }

    /// <summary>
    ///     Find all sections that need to be meshed because of a block change in a section.
    ///     If the block position is on the edge of a section, the neighbor is also considered to be affected.
    /// </summary>
    /// <param name="chunk">The chunk in which the block change happened.</param>
    /// <param name="position">The position of the block change, in block coordinates.</param>
    private void EnqueueMeshingForAllAffectedSections(Core.Logic.Chunks.Chunk chunk, Vector3i position)
    {
        Enqueue(chunk.Cast(), position);

        CheckAxis(axis: 0);
        CheckAxis(axis: 1);
        CheckAxis(axis: 2);

        void CheckAxis(Int32 axis)
        {
            Int32 axisSectionPosition = position[axis] & (Section.Size - 1);

            Vector3i direction = new()
            {
                [axis] = 1
            };

            if (axisSectionPosition == 0) CheckNeighbor(direction * -1);
            else if (axisSectionPosition == Section.Size - 1) CheckNeighbor(direction);
        }

        void CheckNeighbor(Vector3i direction)
        {
            Vector3i neighborPosition = position + direction;

            if (!Subject.TryGetChunk(ChunkPosition.From(neighborPosition), out Core.Logic.Chunks.Chunk? neighbor)) return;

            if (neighbor.IsActive)
            {
                Enqueue(neighbor.Cast(), neighborPosition);
            }
            else
            {
                // We set the section as incomplete.
                // The next time the neighbor chunk is activated (if it is), the section will be meshed.

                Sides missing = direction.ToSide().ToFlag();
                neighbor.Cast().SetSectionAsIncomplete(SectionPosition.From(neighborPosition).Local, missing);
            }
        }
    }

    private void Enqueue(Chunk chunk, Vector3i blockPosition)
    {
        sectionsToMesh.Add((chunk, SectionPosition.From(blockPosition).Local));
    }
}

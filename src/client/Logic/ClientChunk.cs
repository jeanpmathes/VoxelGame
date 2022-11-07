// <copyright file="ClientChunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
public partial class ClientChunk : Chunk
{
    private const int MaxMeshDataStep = 16;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ClientChunk>();

    [NonSerialized] private bool hasMeshData;
    [NonSerialized] private int meshDataIndex;

    /// <summary>
    ///     Create a new client chunk.
    /// </summary>
    /// <param name="world">The world that contains the chunk.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="context">The context of the chunk.</param>
    public ClientChunk(World world, ChunkPosition position, ChunkContext context) : base(world, position, context) {}

    /// <summary>
    ///     Begin meshing the chunk.
    /// </summary>
    public void BeginMeshing()
    {
        if (!IsFullyDecorated) return;

        state.RequestNextState<Meshing>(new ChunkState.RequestDescription
        {
            AllowDuplicateTypes = false,
            AllowSkipOnDeactivation = true,
            AllowDiscardOnLoop = true
        });
    }

    /// <inheritdoc />
    protected override Section CreateSection()
    {
        return new ClientSection();
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
        ((ClientSection) sections[LocalSectionToIndex(x, y, z)]).CreateAndSetMesh(
            SectionPosition.From(Position, (x, y, z)),
            context);
    }

    /// <summary>
    ///     Start a task that will create mesh data for this chunk.
    /// </summary>
    /// <param name="context">The chunk meshing context.</param>
    /// <returns>The meshing task.</returns>
    public Task<SectionMeshData[]> CreateMeshDataAsync(ChunkMeshingContext context)
    {
        return Task.Run(() => CreateMeshData(context));
    }

    private SectionMeshData[] CreateMeshData(ChunkMeshingContext context)
    {
        logger.LogDebug(Events.ChunkOperation, "Started creating mesh data for chunk {Position} using {NeighborCount} neighbors", Position, context.NeighborCount);

        var sectionMeshes = new SectionMeshData[SectionCount];

        for (var s = 0; s < SectionCount; s++)
        {
            (int x, int y, int z) = IndexToLocalSection(s);
            sectionMeshes[s] = ((ClientSection) sections[s]).CreateMeshData(SectionPosition.From(Position, (x, y, z)), context);
        }

        meshDataIndex = 0;

        logger.LogDebug(Events.ChunkOperation, "Finished creating mesh data for chunk {Position} using {NeighborCount} neighbors", Position, context.NeighborCount);

        return sectionMeshes;
    }

    /// <summary>
    ///     Reset the mesh data set-step.
    /// </summary>
    public void ResetMeshDataSetSteps()
    {
        hasMeshData = false;
        meshDataIndex = 0;
    }

    /// <summary>
    ///     Do a mesh data set-step. This will apply a part of the mesh data and activate the part.
    /// </summary>
    /// <param name="sectionMeshes">The mesh data to apply.</param>
    /// <returns>True if this step was the final step.</returns>
    public bool DoMeshDataSetStep(SectionMeshData[] sectionMeshes)
    {
        hasMeshData = false;

        for (var count = 0; count < MaxMeshDataStep; count++)
        {
            ((ClientSection) sections[meshDataIndex]).SetMeshData(sectionMeshes[meshDataIndex]);

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
        ICollection<(ClientSection section, Vector3d position)> renderList)
    {
        if (!hasMeshData || !frustum.IsBoxInFrustum(VMath.CreateBox3(ChunkPoint, ChunkExtents))) return;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            SectionPosition sectionPosition = SectionPosition.From(Position, (x, y, z));
            Vector3d position = sectionPosition.FirstBlock;

            if (frustum.IsBoxInFrustum(
                    VMath.CreateBox3(position + Section.Extents, Section.Extents)))
            {
                renderList.Add(((ClientSection) sections[LocalSectionToIndex(x, y, z)], position));
            }
        }
    }
}

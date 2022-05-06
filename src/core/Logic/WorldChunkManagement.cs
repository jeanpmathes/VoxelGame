// <copyright file="WorldChunkManagement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

public abstract partial class World
{
    /// <summary>
    ///     A dictionary that contains all active chunks.
    /// </summary>
    private readonly Dictionary<(int x, int y, int z), Chunk> activeChunks;

    /// <summary>
    ///     A list of chunk generation tasks.
    /// </summary>
    private readonly List<Task> chunkGenerateTasks;

    /// <summary>
    ///     A list of chunk loading tasks.
    /// </summary>
    private readonly List<Task<Chunk?>> chunkLoadingTasks;

    /// <summary>
    ///     A list of chunk saving tasks.
    /// </summary>
    private readonly List<Task> chunkSavingTasks;

    /// <summary>
    ///     A dictionary containing all chunks that are currently generated, with the task id of their generating task as key.
    /// </summary>
    private readonly Dictionary<int, Chunk> chunksGenerating;

    /// <summary>
    ///     A dictionary containing all chunks that are currently saved, with the task id of their saving task as key.
    /// </summary>
    private readonly Dictionary<int, Chunk> chunksSaving;

    /// <summary>
    ///     A queue that contains all chunks that have to be generated.
    /// </summary>
    private readonly UniqueQueue<Chunk> chunksToGenerate;

    /// <summary>
    ///     A queue of chunks that should be saved and disposed.
    /// </summary>
    private readonly UniqueQueue<Chunk> chunksToSave;

    /// <summary>
    ///     A set of chunk positions that are currently being activated. No new chunks for these positions should be created.
    /// </summary>
    private readonly HashSet<(int x, int y, int z)> positionsActivating;

    /// <summary>
    ///     A set of positions that have no task activating them and have to be activated by the saving code.
    /// </summary>
    private readonly HashSet<(int x, int y, int z)> positionsActivatingThroughSaving;

    /// <summary>
    ///     A dictionary containing all chunk positions that are currently loaded, with the task id of their loading task as
    ///     key.
    /// </summary>
    private readonly Dictionary<int, (int x, int y, int z)> positionsLoading;

    /// <summary>
    ///     A set containing all positions that are currently saved.
    /// </summary>
    private readonly HashSet<(int x, int y, int z)> positionsSaving;

    /// <summary>
    ///     A set of chunk positions which are currently not active and should either be loaded or generated.
    /// </summary>
    private readonly HashSet<(int x, int y, int z)> positionsToActivate;

    /// <summary>
    ///     A queue that contains all positions that have to be loaded.
    /// </summary>
    private readonly UniqueQueue<(int x, int y, int z)> positionsToLoad;

    /// <summary>
    ///     A set of chunk positions that should be released on their activation.
    /// </summary>
    private readonly HashSet<(int x, int y, int z)> positionsToReleaseOnActivation;

    /// <summary>
    ///     Get the active chunk count.
    /// </summary>
    protected int ActiveChunkCount => activeChunks.Count;

    /// <summary>
    ///     All active chunks.
    /// </summary>
    protected IEnumerable<Chunk> ActiveChunks => activeChunks.Values;

    /// <summary>
    ///     Creates a chunk for a chunk position.
    /// </summary>
    protected abstract Chunk CreateChunk(int x, int y, int z);

    /// <summary>
    ///     Start activating chunks. This will either load or generate chunks that are set to be activated.
    /// </summary>
    protected void StartActivatingChunks()
    {
        foreach ((int x, int y, int z) in positionsToActivate)
            if (!positionsActivating.Contains((x, y, z)) && !activeChunks.ContainsKey((x, y, z)))
            {
                string pathToChunk = Path.Combine(ChunkDirectory, Chunk.GetChunkFileName(x, y, z));

                bool isActivating = File.Exists(pathToChunk)
                    ? positionsToLoad.Enqueue((x, y, z))
                    : chunksToGenerate.Enqueue(CreateChunk(x, y, z));

                if (isActivating) positionsActivating.Add((x, y, z));
            }

        positionsToActivate.Clear();
    }

    /// <summary>
    ///     Finish generating chunks. This will check the generation result of chunks that have been generated.
    /// </summary>
    protected void FinishGeneratingChunks()
    {
        if (chunkGenerateTasks.Count > 0)
            for (int i = chunkGenerateTasks.Count - 1; i >= 0; i--)
                if (chunkGenerateTasks[i].IsCompleted)
                {
                    Task completed = chunkGenerateTasks[i];
                    Chunk generatedChunk = chunksGenerating[completed.Id];

                    chunkGenerateTasks.RemoveAt(i);
                    chunksGenerating.Remove(completed.Id);

                    positionsActivating.Remove((generatedChunk.X, generatedChunk.Y, generatedChunk.Z));

                    if (completed.IsFaulted)
                        throw completed.Exception?.GetBaseException() ?? new NullReferenceException();

                    if (!activeChunks.ContainsKey((generatedChunk.X, generatedChunk.Y, generatedChunk.Z)) &&
                        !positionsToReleaseOnActivation.Remove((generatedChunk.X, generatedChunk.Y, generatedChunk.Z)))
                    {
                        activeChunks.Add((generatedChunk.X, generatedChunk.Y, generatedChunk.Z), generatedChunk);

                        ProcessNewlyActivatedChunk(generatedChunk);
                    }
                    else
                    {
                        generatedChunk.Dispose();
                    }
                }
    }

    /// <summary>
    ///     Start generating chunks. This will start generation tasks for chunks that are set to be generated.
    /// </summary>
    protected void StartGeneratingChunks()
    {
        while (chunksToGenerate.Count > 0 && chunkGenerateTasks.Count < MaxGenerationTasks)
        {
            Chunk current = chunksToGenerate.Dequeue();
            Task currentTask = current.GenerateAsync(generator);

            chunkGenerateTasks.Add(currentTask);
            chunksGenerating.Add(currentTask.Id, current);
        }
    }

    /// <summary>
    ///     Finish loading chunks. This will check the loading result of chunks that have been loaded.
    /// </summary>
    protected void FinishLoadingChunks()
    {
        if (chunkLoadingTasks.Count > 0)
            for (int i = chunkLoadingTasks.Count - 1; i >= 0; i--)
                if (chunkLoadingTasks[i].IsCompleted)
                {
                    Task<Chunk?> completed = chunkLoadingTasks[i];
                    (int x, int y, int z) = positionsLoading[completed.Id];

                    chunkLoadingTasks.RemoveAt(i);
                    positionsLoading.Remove(completed.Id);

                    positionsActivating.Remove((x, y, z));

                    if (completed.IsFaulted)
                    {
                        if (!positionsToReleaseOnActivation.Remove((x, y, z)) || !activeChunks.ContainsKey((x, y, z)))
                        {
                            logger.LogError(
                                Events.ChunkLoadingError,
                                completed.Exception!.GetBaseException(),
                                "An exception occurred when loading the chunk ({X}|{Y}|{Z}). " +
                                "The chunk has been scheduled for generation",
                                x,
                                y,
                                z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                            if (chunksToGenerate.Enqueue(CreateChunk(x, y, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                positionsActivating.Add((x, y, z));
                        }
                    }
                    else
                    {
                        Chunk? loadedChunk = completed.Result;

                        if (loadedChunk != null && !activeChunks.ContainsKey((x, y, z)))
                        {
                            if (!positionsToReleaseOnActivation.Remove((loadedChunk.X, loadedChunk.Y, loadedChunk.Z)))
                            {
                                loadedChunk.Setup(this, UpdateCounter);
                                activeChunks.Add((x, y, z), loadedChunk);

                                ProcessNewlyActivatedChunk(loadedChunk);
                            }
                            else
                            {
                                loadedChunk.Dispose();
                            }
                        }
                        else
                        {
                            logger.LogError(
                                Events.ChunkLoadingError,
                                "Position of the loaded chunk file for position ({X}|{Y}|{Z}) did not match the requested position, " +
                                "which can be caused by a renamed chunk file. " +
                                "Position will be scheduled for generation",
                                x,
                                y,
                                z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                            if (chunksToGenerate.Enqueue(CreateChunk(x, y, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                positionsActivating.Add((x, y, z));
                        }
                    }
                }
    }

    /// <summary>
    ///     Start loading chunks. This will load chunks that are scheduled for loading.
    /// </summary>
    protected void StartLoadingChunks()
    {
        while (positionsToLoad.Count > 0 && chunkLoadingTasks.Count < MaxLoadingTasks)
        {
            (int x, int y, int z) = positionsToLoad.Dequeue();

            // If a chunk is already being loaded or saved no new loading task is needed
            if (!positionsLoading.ContainsValue((x, y, z)))
            {
                if (!positionsSaving.Contains((x, y, z)))
                {
                    string pathToChunk = Path.Combine(ChunkDirectory, Chunk.GetChunkFileName(x, y, z));
                    Task<Chunk?> currentTask = Chunk.LoadAsync(pathToChunk, x, y, z);

                    chunkLoadingTasks.Add(currentTask);
                    positionsLoading.Add(currentTask.Id, (x, y, z));
                }
                else
                {
                    positionsActivatingThroughSaving.Add((x, y, z));
                }
            }
        }
    }

    /// <summary>
    ///     Finish saving chunks. This will check the result of the chunk saving tasks.
    /// </summary>
    protected void FinishSavingChunks()
    {
        if (chunkSavingTasks.Count > 0)
            for (int i = chunkSavingTasks.Count - 1; i >= 0; i--)
                if (chunkSavingTasks[i].IsCompleted)
                {
                    Task completed = chunkSavingTasks[i];
                    Chunk completedChunk = chunksSaving[completed.Id];

                    chunkSavingTasks.RemoveAt(i);
                    chunksSaving.Remove(completed.Id);
                    positionsSaving.Remove((completedChunk.X, completedChunk.Y, completedChunk.Z));

                    // Check if the chunk should be activated and is not active and not requested to be released on activation; if true, the chunk will not be disposed
                    if ((positionsToActivate.Contains((completedChunk.X, completedChunk.Y, completedChunk.Z)) ||
                         positionsActivating.Contains((completedChunk.X, completedChunk.Y, completedChunk.Z)))
                        && !activeChunks.ContainsKey((completedChunk.X, completedChunk.Y, completedChunk.Z))
                        && !positionsToReleaseOnActivation.Contains(
                            (completedChunk.X, completedChunk.Y, completedChunk.Z)))
                    {
                        positionsToActivate.Remove((completedChunk.X, completedChunk.Y, completedChunk.Z));

                        if (positionsActivatingThroughSaving.Remove(
                                (completedChunk.X, completedChunk.Y, completedChunk.Z)))
                            positionsActivating.Remove((completedChunk.X, completedChunk.Y, completedChunk.Z));

                        activeChunks.Add((completedChunk.X, completedChunk.Y, completedChunk.Z), completedChunk);

                        ProcessNewlyActivatedChunk(completedChunk);
                    }
                    else
                    {
                        if (completed.IsFaulted)
                            logger.LogError(
                                Events.ChunkSavingError,
                                completed.Exception!.GetBaseException(),
                                "An exception occurred when saving chunk ({X}|{Y}|{Z}). " +
                                "Chunk will be disposed without saving",
                                completedChunk.X,
                                completedChunk.Y,
                                completedChunk.Z);

                        if (positionsActivatingThroughSaving.Remove(
                                (completedChunk.X, completedChunk.Y, completedChunk.Z)))
                            positionsActivating.Remove((completedChunk.X, completedChunk.Y, completedChunk.Z));

                        positionsToReleaseOnActivation.Remove((completedChunk.X, completedChunk.Y, completedChunk.Z));

                        completedChunk.Dispose();
                    }
                }
    }

    /// <summary>
    ///     Process a chunk that has been just activated.
    /// </summary>
    protected abstract void ProcessNewlyActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Start saving chunks. This will start saving tasks for chunks selected for saving.
    /// </summary>
    protected void StartSavingChunks()
    {
        while (chunksToSave.Count > 0 && chunkSavingTasks.Count < MaxSavingTasks)
        {
            Chunk current = chunksToSave.Dequeue();
            Task currentTask = current.SaveAsync(ChunkDirectory);

            chunkSavingTasks.Add(currentTask);
            chunksSaving.Add(currentTask.Id, current);
            positionsSaving.Add((current.X, current.Y, current.Z));
        }
    }

    /// <summary>
    ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
    /// </summary>
    /// <param name="x">The x coordinates in chunk coordinates.</param>
    /// <param name="y">The y coordinates in chunk coordinates.</param>
    /// <param name="z">The z coordinates in chunk coordinates.</param>
    public void RequestChunk(int x, int y, int z)
    {
        positionsToReleaseOnActivation.Remove((x, y, z));

        if (positionsActivating.Contains((x, y, z)) || activeChunks.ContainsKey((x, y, z))) return;
        positionsToActivate.Add((x, y, z));

        logger.LogDebug(Events.ChunkRequest, "Chunk ({X}|{Y}|{Z}) has been requested successfully", x, y, z);
    }

    /// <summary>
    ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
    /// </summary>
    /// <param name="x">The x coordinates in chunk coordinates.</param>
    /// <param name="y">The y coordinates in chunk coordinates.</param>
    /// <param name="z">The z coordinates in chunk coordinates.</param>
    /// <returns>true if the chunk will be released; false if not.</returns>
    public bool ReleaseChunk(int x, int y, int z)
    {
        // Check if the chunk can be released
        if (x == 0 && y == 0 && z == 0) return false; // The chunk at (0|0|0) cannot be released.

        var canRelease = false;

        // Check if the chunk exists
        if (activeChunks.TryGetValue((x, y, z), out Chunk? chunk))
        {
            activeChunks.Remove((x, y, z));
            chunksToSave.Enqueue(chunk);

            logger.LogDebug(Events.ChunkRelease, "Released chunk ({X}|{Y}|{Z})", x, y, z);

            canRelease = true;
        }

        if (positionsActivating.Contains((x, y, z)))
        {
            positionsToReleaseOnActivation.Add((x, y, z));

            logger.LogDebug(Events.ChunkRelease, "Scheduled to release chunk ({X}|{Y}|{Z}) after activation", x, y, z);

            canRelease = true;
        }

        if (positionsToActivate.Contains((x, y, z)))
        {
            positionsToActivate.Remove((x, y, z));

            logger.LogDebug(Events.ChunkRelease, "Removed chunk ({X}|{Y}|{Z}) from activation list", x, y, z);

            canRelease = true;
        }

        return canRelease;
    }

    /// <summary>
    ///     Gets an active chunk.
    /// </summary>
    /// <param name="x">The x position of the chunk in chunk coordinates.</param>
    /// <param name="y">The y position of the chunk in chunk coordinates.</param>
    /// <param name="z">The z position of the chunk in chunk coordinates.</param>
    /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
    public Chunk? GetChunk(int x, int y, int z)
    {
        activeChunks.TryGetValue((x, y, z), out Chunk? chunk);

        return chunk;
    }

    /// <summary>
    ///     Check if a chunk is active.
    /// </summary>
    /// <param name="x">The x position of the chunk in chunk coordinates.</param>
    /// <param name="y">The y position of the chunk in chunk coordinates.</param>
    /// <param name="z">The z position of the chunk in chunk coordinates.</param>
    /// <returns>True if the chunk is active.</returns>
    protected bool IsChunkActive(int x, int y, int z)
    {
        return activeChunks.ContainsKey((x, y, z));
    }

    /// <summary>
    ///     Try to get an active chunk.
    /// </summary>
    /// <param name="x">The x position of the chunk in chunk coordinates.</param>
    /// <param name="y">The y position of the chunk in chunk coordinates.</param>
    /// <param name="z">The z position of the chunk in chunk coordinates.</param>
    /// <param name="chunk">The chunk at the given position or null if no active chunk was found.</param>
    /// <returns>True if an active chunk was found.</returns>
    protected bool TryGetChunk(int x, int y, int z, [NotNullWhen(returnValue: true)] out Chunk? chunk)
    {
        return activeChunks.TryGetValue((x, y, z), out chunk);
    }

    /// <summary>
    ///     Gets a section of an active chunk.
    /// </summary>
    /// <param name="sectionPosition">The position of the section, in section coordinates.</param>
    /// <returns>The section at the given position or null if no section was found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Section? GetSection(Vector3i sectionPosition)
    {
        (int x, int y, int z) = sectionPosition;

        int chunkX = x >> Chunk.SizeExp;
        int chunkY = y >> Chunk.SizeExp;
        int chunkZ = z >> Chunk.SizeExp;

        int sectionX = x & (Chunk.Size - 1);
        int sectionY = y & (Chunk.Size - 1);
        int sectionZ = z & (Chunk.Size - 1);

        return activeChunks.TryGetValue((chunkX, chunkY, chunkZ), out Chunk? chunk)
            ? chunk.GetSection(sectionX, sectionY, sectionZ)
            : null;

    }

    /// <summary>
    ///     Get the chunk that contains the specified block/fluid position.
    /// </summary>
    /// <param name="position">The block/fluid position.</param>
    /// <returns>The chunk, or null the position is not in an active chunk.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Chunk? GetChunkWithPosition(Vector3i position)
    {
        (int x, int y, int z) = position;

        int chunkX = x >> Chunk.BlockSizeExp;
        int chunkY = y >> Chunk.BlockSizeExp;
        int chunkZ = z >> Chunk.BlockSizeExp;

        bool exists = activeChunks.TryGetValue((chunkX, chunkY, chunkZ), out Chunk? chunk);

        return !exists ? null : chunk;

    }
}

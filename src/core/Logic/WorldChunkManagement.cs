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
    private readonly Dictionary<ChunkPosition, Chunk> activeChunks;

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
    private readonly HashSet<ChunkPosition> positionsActivating;

    /// <summary>
    ///     A set of positions that have no task activating them and have to be activated by the saving code.
    /// </summary>
    private readonly HashSet<ChunkPosition> positionsActivatingThroughSaving;

    /// <summary>
    ///     A dictionary containing all chunk positions that are currently loaded, with the task id of their loading task as
    ///     key.
    /// </summary>
    private readonly Dictionary<int, ChunkPosition> positionsLoading;

    /// <summary>
    ///     A set containing all positions that are currently saved.
    /// </summary>
    private readonly HashSet<ChunkPosition> positionsSaving;

    /// <summary>
    ///     A set of chunk positions which are currently not active and should either be loaded or generated.
    /// </summary>
    private readonly HashSet<ChunkPosition> positionsToActivate;

    /// <summary>
    ///     A queue that contains all positions that have to be loaded.
    /// </summary>
    private readonly UniqueQueue<ChunkPosition> positionsToLoad;

    /// <summary>
    ///     A set of chunk positions that should be released on their activation.
    /// </summary>
    private readonly HashSet<ChunkPosition> positionsToReleaseOnActivation;

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
    protected abstract Chunk CreateChunk(ChunkPosition position);

    /// <summary>
    ///     Start activating chunks. This will either load or generate chunks that are set to be activated.
    /// </summary>
    protected void StartActivatingChunks()
    {
        foreach (ChunkPosition position in positionsToActivate)
            if (!positionsActivating.Contains(position) && !activeChunks.ContainsKey(position))
            {
                string pathToChunk = Path.Combine(ChunkDirectory, Chunk.GetChunkFileName(position));

                bool isActivating = File.Exists(pathToChunk)
                    ? positionsToLoad.Enqueue(position)
                    : chunksToGenerate.Enqueue(CreateChunk(position));

                if (isActivating) positionsActivating.Add(position);
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

                    positionsActivating.Remove(generatedChunk.Position);

                    if (completed.IsFaulted)
                        throw completed.Exception?.GetBaseException() ?? new NullReferenceException();

                    if (!activeChunks.ContainsKey(generatedChunk.Position) &&
                        !positionsToReleaseOnActivation.Remove(generatedChunk.Position))
                    {
                        activeChunks.Add(generatedChunk.Position, generatedChunk);

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
                    ChunkPosition position = positionsLoading[completed.Id];

                    chunkLoadingTasks.RemoveAt(i);
                    positionsLoading.Remove(completed.Id);

                    positionsActivating.Remove(position);

                    if (completed.IsFaulted)
                    {
                        if (!positionsToReleaseOnActivation.Remove(position) || !activeChunks.ContainsKey(position))
                        {
                            logger.LogError(
                                Events.ChunkLoadingError,
                                completed.Exception!.GetBaseException(),
                                "An exception occurred when loading the chunk {Position}. " +
                                "The chunk has been scheduled for generation",
                                position);

#pragma warning disable CA2000 // Dispose objects before losing scope
                            if (chunksToGenerate.Enqueue(CreateChunk(position)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                positionsActivating.Add(position);
                        }
                    }
                    else
                    {
                        Chunk? loadedChunk = completed.Result;

                        if (loadedChunk != null && !activeChunks.ContainsKey(position))
                        {
                            if (!positionsToReleaseOnActivation.Remove(loadedChunk.Position))
                            {
                                loadedChunk.Setup(this, UpdateCounter);
                                activeChunks.Add(position, loadedChunk);

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
                                "Position of the loaded chunk file for position {Position} did not match the requested position, " +
                                "which can be caused by a renamed chunk file. " +
                                "Position will be scheduled for generation",
                                position);

#pragma warning disable CA2000 // Dispose objects before losing scope
                            if (chunksToGenerate.Enqueue(CreateChunk(position)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                positionsActivating.Add(position);
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
            ChunkPosition position = positionsToLoad.Dequeue();

            // If a chunk is already being loaded or saved no new loading task is needed
            if (!positionsLoading.ContainsValue(position))
            {
                if (!positionsSaving.Contains(position))
                {
                    string pathToChunk = Path.Combine(ChunkDirectory, Chunk.GetChunkFileName(position));
                    Task<Chunk?> currentTask = Chunk.LoadAsync(pathToChunk, position);

                    chunkLoadingTasks.Add(currentTask);
                    positionsLoading.Add(currentTask.Id, position);
                }
                else
                {
                    positionsActivatingThroughSaving.Add(position);
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
                    positionsSaving.Remove(completedChunk.Position);

                    // Check if the chunk should be activated and is not active and not requested to be released on activation; if true, the chunk will not be disposed
                    if ((positionsToActivate.Contains(completedChunk.Position) ||
                         positionsActivating.Contains(completedChunk.Position))
                        && !activeChunks.ContainsKey(completedChunk.Position)
                        && !positionsToReleaseOnActivation.Contains(
                            completedChunk.Position))
                    {
                        positionsToActivate.Remove(completedChunk.Position);

                        if (positionsActivatingThroughSaving.Remove(
                                completedChunk.Position))
                            positionsActivating.Remove(completedChunk.Position);

                        activeChunks.Add(completedChunk.Position, completedChunk);

                        ProcessNewlyActivatedChunk(completedChunk);
                    }
                    else
                    {
                        if (completed.IsFaulted)
                            logger.LogError(
                                Events.ChunkSavingError,
                                completed.Exception!.GetBaseException(),
                                "An exception occurred when saving chunk {Position}. " +
                                "Chunk will be disposed without saving",
                                completedChunk.Position);

                        if (positionsActivatingThroughSaving.Remove(
                                completedChunk.Position))
                            positionsActivating.Remove(completedChunk.Position);

                        positionsToReleaseOnActivation.Remove(completedChunk.Position);

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
            positionsSaving.Add(current.Position);
        }
    }

    /// <summary>
    ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void RequestChunk(ChunkPosition position)
    {
        positionsToReleaseOnActivation.Remove(position);

        if (positionsActivating.Contains(position) || activeChunks.ContainsKey(position)) return;
        positionsToActivate.Add(position);

        logger.LogDebug(Events.ChunkRequest, "Chunk {Position} has been requested successfully", position);
    }

    /// <summary>
    ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>true if the chunk will be released; false if not.</returns>
    public bool ReleaseChunk(ChunkPosition position)
    {
        // Check if the chunk can be released
        if (position == ChunkPosition.Origin) return false; // The chunk at (0|0|0) cannot be released.

        var canRelease = false;

        // Check if the chunk exists
        if (activeChunks.TryGetValue(position, out Chunk? chunk))
        {
            activeChunks.Remove(position);
            chunksToSave.Enqueue(chunk);

            logger.LogDebug(Events.ChunkRelease, "Released chunk {Position}", position);

            canRelease = true;
        }

        if (positionsActivating.Contains(position))
        {
            positionsToReleaseOnActivation.Add(position);

            logger.LogDebug(Events.ChunkRelease, "Scheduled to release chunk {Position} after activation", position);

            canRelease = true;
        }

        if (positionsToActivate.Contains(position))
        {
            positionsToActivate.Remove(position);

            logger.LogDebug(Events.ChunkRelease, "Removed chunk {Position} from activation list", position);

            canRelease = true;
        }

        return canRelease;
    }

    /// <summary>
    ///     Gets an active chunk.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
    public Chunk? GetChunk(ChunkPosition position)
    {
        activeChunks.TryGetValue(position, out Chunk? chunk);

        return chunk;
    }

    /// <summary>
    ///     Get the chunk that contains the specified block/fluid position.
    /// </summary>
    /// <param name="position">The block/fluid position.</param>
    /// <returns>The chunk, or null the position is not in an active chunk.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Chunk? GetChunk(Vector3i position)
    {
        bool exists = activeChunks.TryGetValue(ChunkPosition.From(position), out Chunk? chunk);

        return !exists ? null : chunk;
    }

    /// <summary>
    ///     Check if a chunk is active.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>True if the chunk is active.</returns>
    protected bool IsChunkActive(ChunkPosition position)
    {
        return activeChunks.ContainsKey(position);
    }

    /// <summary>
    ///     Try to get an active chunk.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="chunk">The chunk at the given position or null if no active chunk was found.</param>
    /// <returns>True if an active chunk was found.</returns>
    protected bool TryGetChunk(ChunkPosition position, [NotNullWhen(returnValue: true)] out Chunk? chunk)
    {
        return activeChunks.TryGetValue(position, out chunk);
    }

    /// <summary>
    ///     Gets a section of an active chunk.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The section at the given position or null if no section was found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Section? GetSection(SectionPosition position)
    {
        return activeChunks.TryGetValue(position.GetChunk(), out Chunk? chunk)
            ? chunk.GetSection(position)
            : null;
    }
}

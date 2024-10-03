// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Represents the world. Contains everything that is in the world, e.g. chunks, entities, etc.
/// </summary>
public abstract partial class World : IDisposable, IGrid
{
    /// <summary>
    ///     The highest absolute value of a block position coordinate component.
    ///     This value also describes the word extents in blocks, thus the world size is two times this value.
    ///     The actual active size of the world can be smaller, but never larger.
    /// </summary>
    public const UInt32 BlockLimit = 10_000_000;

    private const UInt32 ChunkLimit = BlockLimit / Chunk.BlockSize;

    /// <summary>
    ///     The limit of the world extents, in sections.
    /// </summary>
    public const UInt32 SectionLimit = BlockLimit / Section.Size;

    private readonly ChunkSet chunks;

    /// <summary>
    ///     A timer to profile different states and world operations.
    ///     Will be started on world creation, inheritors are free to stop, override or restart it.
    /// </summary>
    protected Timer? timer;

    private State currentState = State.Activating;

    private (Future saving, Action callback)? deactivation;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    protected World(DirectoryInfo path, String name, (Int32 upper, Int32 lower) seed) :
        this(
            new WorldData(new WorldInformation
                {
                    Name = name,
                    UpperSeed = seed.upper,
                    LowerSeed = seed.lower,
                    Creation = DateTime.UtcNow,
                    Version = ApplicationInformation.Instance.Version
                },
                path),
            isNew: true)
    {
        Data.Save();

        LogCreatedNewWorld(logger);
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    protected World(WorldData data) :
        this(data, isNew: false)
    {
        LogLoadedExistingWorld(logger);
    }

    /// <summary>
    ///     Set up of readonly fields and non-optional steps.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private World(WorldData data, Boolean isNew)
    {
        timer = Timer.Start("World Setup", TimingStyle.Once, Profile.GetSingleUseActiveProfiler());

        Data = data;

        Data.EnsureValidDirectory();
        Data.EnsureValidInformation();

        IWorldGenerator generator = GetAndInitializeGenerator(this, timer);

        ChunkContext = new ChunkContext(generator, CreateChunk, ProcessNewlyActivatedChunk, ProcessActivatedChunk, UnloadChunk);

        chunks = new ChunkSet(this, ChunkContext);
    }

    /// <summary>
    ///     Get all currently existing chunks.
    /// </summary>
    public IEnumerable<Chunk> Chunks => chunks.All;

    /// <summary>
    ///     Set up the chunk context.
    /// </summary>
    protected ChunkContext ChunkContext { get; }

    /// <summary>
    ///     Get the stored world data.
    /// </summary>
    public WorldData Data { get; }

    /// <summary>
    ///     Get the world creation seed.
    /// </summary>
    public (Int32 upper, Int32 lower) Seed => (Data.Information.UpperSeed, Data.Information.LowerSeed);

    /// <summary>
    ///     Get whether the world is active.
    /// </summary>
    public Boolean IsActive => CurrentState == State.Active;

    /// <summary>
    ///     Get the world state.
    /// </summary>
    protected State CurrentState
    {
        get => currentState;
        set
        {
            State oldState = currentState;
            currentState = value;

            if (oldState != currentState)
                StateChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     The number of chunk state updates that have been performed in the last update cycle.
    ///     Is initialized to <c>-1</c> before the first update cycle.
    /// </summary>
    public Int32 ChunkStateUpdateCount { get; private set; } = -1;

    /// <summary>
    ///     Get or set the spawn position in this world.
    /// </summary>
    public Vector3d SpawnPosition
    {
        get => Data.Information.SpawnInformation.Position;
        set
        {
            Data.Information.SpawnInformation = new SpawnInformation(value);
            LogWorldSpawnPositionSet(logger, value);
        }
    }

    /// <summary>
    ///     Get or set the world size in blocks.
    /// </summary>
    public UInt32 SizeInBlocks
    {
        get => Data.Information.Size;
        set
        {
            UInt32 oldSize = Data.Information.Size;
            Data.Information.Size = value;

            Data.EnsureValidInformation();

            if (oldSize != Data.Information.Size) LogWorldSizeSet(logger, Data.Information.Size);
        }
    }

    /// <summary>
    ///     Get the extents of the world. This mark the reachable area of the world.
    /// </summary>
    public Vector3i Extents => new((Int32) SizeInBlocks, (Int32) SizeInBlocks, (Int32) SizeInBlocks);

    /// <summary>
    ///     Get the info map of this world.
    /// </summary>
    public IMap Map => ChunkContext.Generator.Map;

    /// <summary>
    ///     Get the active chunk count.
    /// </summary>
    protected Int32 ActiveChunkCount => chunks.ActiveCount;

    /// <summary>
    ///     All active chunks.
    /// </summary>
    protected IEnumerable<Chunk> ActiveChunks => chunks.AllActive;

    /// <summary>
    ///     Get both the fluid and block instance at a given position.
    ///     The content can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>The content, if there is any.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Content? GetContent(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        RetrieveContent(position, out Content? content);

        return content;
    }

    /// <summary>
    ///     Set the content of a world position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetContent(Content content, Vector3i position)
    {
        Throw.IfDisposed(disposed);

        SetContent(content, position, tickFluid: true);
    }

    /// <summary>
    ///     Begin deactivating the world, saving all chunks and the meta information.
    /// </summary>
    /// <param name="onFinished">The action to be called when the world is deactivated.</param>
    public void BeginDeactivating(Action onFinished)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(CurrentState == State.Active);
        CurrentState = State.Deactivating;

        LogUnloadingWorld(logger);

        OnDeactivation();

        chunks.BeginSaving();

        Data.Information.Version = ApplicationInformation.Instance.Version;
        var saving = Future.Create(Data.Save);

        deactivation = (saving, onFinished);
    }

    /// <summary>
    ///     Process the deactivation, assuming it has been started.
    /// </summary>
    /// <returns>Whether the deactivation is finished.</returns>
    protected Boolean ProcessDeactivation()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(deactivation != null);

        (Future saving, Action callback) = deactivation.Value;

        Boolean done = saving.IsCompleted && chunks.IsEmpty;

        if (!done) return false;

        LogUnloadedWorld(logger);
        callback();

        if (saving.Exception is {} exception)
            LogFailedToSaveWorldMetaInformation(logger, exception);

        return true;
    }

    /// <summary>
    ///     Called when the world deactivates.
    /// </summary>
    protected virtual void OnDeactivation() {}

    private void UnloadChunk(Chunk chunk)
    {
        chunks.Unload(chunk);
    }

    private static IWorldGenerator GetAndInitializeGenerator(World world, Timer? timer)
    {
        return new Generator(new WorldGeneratorContext(world, timer));
    }

    /// <summary>
    ///     Emit views of global world data for debugging.
    /// </summary>
    public void EmitViews(DirectoryInfo directory)
    {
        Throw.IfDisposed(disposed);

        ChunkContext.Generator.EmitViews(directory);
    }

    /// <summary>
    ///     Search for named generated elements, such as structures.
    ///     The search is performed on enumeration.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="name">The name of the element.</param>
    /// <param name="maxDistance">The maximum distance to search.</param>
    /// <returns>The positions of the elements, or null if the name is not valid.</returns>
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        Throw.IfDisposed(disposed);

        return ChunkContext.Generator.SearchNamedGeneratedElements(start, name, maxDistance);
    }

    /// <summary>
    ///     Let all chunks that need it run their state updates.
    /// </summary>
    protected void UpdateChunkStates()
    {
        Profile.Instance?.UpdateStateDurations(nameof(Chunk));

        ChunkStateUpdateCount = ChunkContext.UpdateList.Update();
    }

    /// <summary>
    ///     Returns the block instance at a given position in block coordinates. The block is only searched in active chunks.
    /// </summary>
    /// <param name="position">The block position.</param>
    /// <returns>The block instance at the given position or null if the block was not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance? GetBlock(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        RetrieveContent(position, out Content? content);

        return content?.Block;
    }

    /// <summary>
    ///     Retrieve the content at a given position. The content can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="potentialContent">The potential content at the given position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RetrieveContent(Vector3i position, out Content? potentialContent)
    {
        Chunk? chunk = GetActiveChunk(position);

        if (chunk != null)
        {
            UInt32 val = chunk.GetSection(position).GetContent(position);
            Section.Decode(val, out Content content);

            potentialContent = content;

            return;
        }

        potentialContent = null;
    }

    /// <summary>
    ///     Get the fluid at a given position. The fluid can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The position in the world.</param>
    /// <returns>The fluid instance, if there is any.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluidInstance? GetFluid(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        RetrieveContent(position, out Content? content);

        return content?.Fluid;
    }

    /// <summary>
    ///     Sets a block in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of
    ///     the changed block. The fluid at the position is preserved.
    /// </summary>
    /// <param name="block">The block which should be set at the position.</param>
    /// <param name="position">The block position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(BlockInstance block, Vector3i position)
    {
        Throw.IfDisposed(disposed);

        FluidInstance? potentialFluid = GetFluid(position);

        if (potentialFluid is not {} fluid) return;

        SetContent(new Content(block, fluid), position, tickFluid: true);
    }

    /// <summary>
    ///     Sets a fluid in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of the
    ///     changed block. The block at the position is preserved.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFluid(FluidInstance fluid, Vector3i position)
    {
        Throw.IfDisposed(disposed);

        BlockInstance? potentialBlock = GetBlock(position);

        if (potentialBlock is not {} block) return;

        SetContent(new Content(block, fluid), position, tickFluid: false);
    }

    /// <summary>
    ///     Set the <c>isStatic</c> flag of a fluid without causing any updates around this fluid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ModifyFluid(Boolean isStatic, Vector3i position)
    {
        ModifyWorldData(position, ~Section.StaticMask, isStatic ? Section.StaticMask : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetContent(in Content content, Vector3i position, Boolean tickFluid)
    {
        Chunk? chunk = GetActiveChunk(position);

        if (chunk == null) return;

        UInt32 val = Section.Encode(content);

        chunk.GetSection(position).SetContent(position, val);

        content.Block.Block.ContentUpdate(this, position, content);
        if (tickFluid) content.Fluid.Fluid.TickNow(this, position, content.Fluid);

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i neighborPosition = side.Offset(position);

            Content? neighborContent = GetContent(neighborPosition);

            if (neighborContent == null) continue;

            (BlockInstance blockNeighbor, FluidInstance fluidNeighbor) = neighborContent.Value;

            // Side is passed out of the perspective of the block receiving the block update.
            blockNeighbor.Block.NeighborUpdate(this, neighborPosition, blockNeighbor.Data, side.Opposite());
            fluidNeighbor.Fluid.TickSoon(this, neighborPosition, fluidNeighbor.IsStatic);
        }

        ProcessChangedSection(chunk, position);
    }

    /// <summary>
    ///     Process that a section was changed.
    /// </summary>
    /// <param name="chunk">The chunk containing the section.</param>
    /// <param name="position">The position of the block that caused the section change.</param>
    protected abstract void ProcessChangedSection(Chunk chunk, Vector3i position);

    /// <summary>
    ///     Modify the data of a position, without causing any updates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ModifyWorldData(Vector3i position, UInt32 clearMask, UInt32 addMask)
    {
        Chunk? chunk = GetActiveChunk(position);

        if (chunk == null) return;

        UInt32 val = chunk.GetSection(position).GetContent(position);

        val &= clearMask;
        val |= addMask;

        chunk.GetSection(position).SetContent(position, val);

        ProcessChangedSection(chunk, position);
    }

    /// <summary>
    ///     Set a position to the default block.
    /// </summary>
    public void SetDefaultBlock(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        SetBlock(BlockInstance.Default, position);
    }

    /// <summary>
    ///     Set a position to the default fluid.
    /// </summary>
    public void SetDefaultFluid(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        SetFluid(FluidInstance.Default, position);
    }

    /// <summary>
    ///     Force a random update at a position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>True if both the fluid and block at the position received a random update.</returns>
    public Boolean DoRandomUpdate(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        Content? content = GetContent(position);

        if (content == null) return false;

        (BlockInstance block, FluidInstance fluid) = content.Value;

        block.Block.RandomUpdate(this, position, block.Data);
        fluid.Fluid.RandomUpdate(this, position, fluid.Level, fluid.IsStatic);

        return true;
    }

    /// <summary>
    ///     Get whether a chunk position is in the maximum allowed world limits.
    ///     Such a position can still be outside the reachable <see cref="Extents" />.
    /// </summary>
    private static Boolean IsInLimits(ChunkPosition position)
    {
        return Math.Abs(position.X) <= ChunkLimit && Math.Abs(position.Y) <= ChunkLimit && Math.Abs(position.Z) <= ChunkLimit;
    }

    /// <summary>
    ///     Get whether a section position is in the maximum allowed world limits.
    ///     Such a position can still be outside the reachable <see cref="Extents" />.
    /// </summary>
    public static Boolean IsInLimits(SectionPosition position)
    {
        return Math.Abs(position.X) <= SectionLimit && Math.Abs(position.Y) <= SectionLimit && Math.Abs(position.Z) <= SectionLimit;
    }

    /// <summary>
    ///     Get whether a block position is in the maximum allowed world limits.
    ///     Such a position can still be outside of the reachable <see cref="Extents" />.
    /// </summary>
    private static Boolean IsInLimits(Vector3i position)
    {
        if (position.X is Int32.MinValue) return false;
        if (position.Y is Int32.MinValue) return false;
        if (position.Z is Int32.MinValue) return false;

        return Math.Abs(position.X) <= BlockLimit && Math.Abs(position.Y) <= BlockLimit && Math.Abs(position.Z) <= BlockLimit;
    }

    /// <summary>
    ///     Factory method that creates a new chunk.
    /// </summary>
    /// <param name="context">The context of the chunk.</param>
    /// <returns>The new chunk.</returns>
    protected abstract Chunk CreateChunk(ChunkContext context);

    /// <summary>
    ///     Process a chunk that has been just activated.
    ///     This method is not allowed to return the hidden state.
    /// </summary>
    /// <returns>The next state of the chunk, or <c>null</c> if no activation is currently possible.</returns>
    protected abstract ChunkState? ProcessNewlyActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Process a chunk that has just switched to the active state through a weak activation.
    ///     This method is not allowed to return the hidden state.
    /// </summary>
    /// <returns>The next state of the chunk, or <c>null</c> if no activation is currently possible.</returns>
    protected abstract ChunkState? ProcessActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void RequestChunk(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(CurrentState != State.Deactivating);

        if (!IsInLimits(position)) return;

        chunks.Request(position);

        LogChunkRequested(logger, position);
    }

    /// <summary>
    ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void ReleaseChunk(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(CurrentState != State.Deactivating);

        if (!IsInLimits(position)) return;

        chunks.Release(position);

        LogChunkReleased(logger, position);
    }

    /// <summary>
    ///     Gets an active chunk.
    ///     See <see cref="ChunkSet.GetActive" /> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
    public Chunk? GetActiveChunk(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        return !IsInLimits(position) ? null : chunks.GetActive(position);
    }

    /// <summary>
    ///     Get the chunk that contains the specified block/fluid position.
    ///     See <see cref="ChunkSet.GetActive" /> for the restrictions.
    /// </summary>
    /// <param name="position">The block/fluid position.</param>
    /// <returns>The chunk, or null the position is not in an active chunk.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Chunk? GetActiveChunk(Vector3i position)
    {
        Throw.IfDisposed(disposed);

        return IsInLimits(position) ? GetActiveChunk(ChunkPosition.From(position)) : null;
    }

    /// <summary>
    ///     Check if a chunk is active.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>True if the chunk is active.</returns>
    protected Boolean IsChunkActive(ChunkPosition position)
    {
        Throw.IfDisposed(disposed);

        return GetActiveChunk(position) != null;
    }

    /// <summary>
    ///     Try to get a chunk. The chunk is possibly not active.
    ///     See <see cref="ChunkSet.GetAny" /> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="chunk">The chunk at the given position or null if no chunk was found.</param>
    /// <returns>True if a chunk was found.</returns>
    public Boolean TryGetChunk(ChunkPosition position, [NotNullWhen(returnValue: true)] out Chunk? chunk)
    {
        Throw.IfDisposed(disposed);

        chunk = chunks.GetAny(position);

        return chunk != null;
    }

    /// <summary>
    ///     Fired anytime the world switches to the ready-state.
    /// </summary>
    public event EventHandler<EventArgs> StateChanged = delegate {};

    /// <summary>
    ///     The world state.
    /// </summary>
    protected enum State
    {
        /// <summary>
        ///     The initial state.
        /// </summary>
        Activating,

        /// <summary>
        ///     In the active state, normal operations like physics are performed.
        /// </summary>
        Active,

        /// <summary>
        ///     The final state, the world is being deactivated.
        /// </summary>
        Deactivating
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    [LoggerMessage(EventId = Events.WorldIO, Level = LogLevel.Information, Message = "Created new world")]
    private static partial void LogCreatedNewWorld(ILogger logger);

    [LoggerMessage(EventId = Events.WorldIO, Level = LogLevel.Information, Message = "Loaded existing world")]
    private static partial void LogLoadedExistingWorld(ILogger logger);

    [LoggerMessage(EventId = Events.WorldIO, Level = LogLevel.Information, Message = "Unloading world")]
    private static partial void LogUnloadingWorld(ILogger logger);

    [LoggerMessage(EventId = Events.WorldIO, Level = LogLevel.Information, Message = "Unloaded world")]
    private static partial void LogUnloadedWorld(ILogger logger);

    [LoggerMessage(EventId = Events.WorldSavingError, Level = LogLevel.Error, Message = "Failed to save world meta information")]
    private static partial void LogFailedToSaveWorldMetaInformation(ILogger logger, Exception exception);

    [LoggerMessage(EventId = Events.WorldState, Level = LogLevel.Information, Message = "World spawn position has been set to: {Position}")]
    private static partial void LogWorldSpawnPositionSet(ILogger logger, Vector3d position);

    [LoggerMessage(EventId = Events.WorldState, Level = LogLevel.Information, Message = "World size has been set to: {Size}")]
    private static partial void LogWorldSizeSet(ILogger logger, UInt32 size);

    [LoggerMessage(EventId = Events.ChunkRequest, Level = LogLevel.Debug, Message = "Chunk {Position} has been requested")]
    private static partial void LogChunkRequested(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkRelease, Level = LogLevel.Debug, Message = "Released chunk {Position}")]
    private static partial void LogChunkReleased(ILogger logger, ChunkPosition position);

    #endregion LOGGING

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            chunks.Dispose();

            ChunkContext.Dispose();

            timer?.Dispose();
        }

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~World()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}

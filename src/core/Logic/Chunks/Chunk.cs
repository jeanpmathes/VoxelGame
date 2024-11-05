// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     A chunk, a cubic group of sections.
/// </summary>
public partial class Chunk : IDisposable, IEntity
{
    /// <summary>
    ///     Creates a section.
    /// </summary>
    public delegate Section SectionFactory(ArraySegment<UInt32> blocks);

    /// <summary>
    ///     Result status of loading a chunk.
    /// </summary>
    public enum LoadingResult
    {
        /// <summary>
        ///     No error occurred.
        /// </summary>
        Success,

        /// <summary>
        ///     An IO-related error occurred.
        /// </summary>
        IOError,

        /// <summary>
        ///     A format-related error occurred.
        /// </summary>
        FormatError,

        /// <summary>
        ///     A validation error occurred, meaning the data is logically invalid.
        /// </summary>
        ValidationError
    }

    private const String FileSignature = "VG_CHUNK";

    /// <summary>
    ///     The number of sections in a chunk along every axis.
    /// </summary>
    public const Int32 Size = 4;

    /// <summary>
    ///     The number of blocks in a chunk along every axis.
    /// </summary>
    public const Int32 BlockSize = Size * Section.Size;

    /// <summary>
    ///     The number of sections per chunk.
    /// </summary>
    public const Int32 SectionCount = Size * Size * Size;

    /// <summary>
    ///     The maximum decoration stage.
    ///     It is <c>6</c> because the cube of 3x3x3 chunks is decorated using just two diagonals.
    /// </summary>
    private const Int32 MaxDecorationStage = 6;

    /// <summary>
    ///     Result of <c>lb(Size)</c> as int.
    /// </summary>
    public static readonly Int32 SizeExp = BitOperations.Log2(Size);

    /// <summary>
    ///     Result of <c>lb(Size) * 2</c> as int.
    /// </summary>
    public static readonly Int32 SizeExp2 = SizeExp * 2;

    /// <summary>
    ///     Result of <c>lb(BlockSize)</c> as int.
    /// </summary>
    public static readonly Int32 BlockSizeExp = BitOperations.Log2(BlockSize);

    /// <summary>
    ///     Result of <c>lb(BlockSize) * 2</c> as int.
    /// </summary>
    public static readonly Int32 BlockSizeExp2 = BlockSizeExp * 2;

    private readonly StateTracker tracker = new(nameof(Chunk));

    /// <summary>
    ///     The sections in this chunk. Provide views into the block data.
    /// </summary>
    private readonly Section[] sections = new Section[SectionCount];

    /// <summary>
    ///     The core resource of a chunk are its sections and their blocks.
    /// </summary>
    private readonly Resource coreResource = new($"{nameof(Chunk)}Core");

    /// <summary>
    ///     Extended resources are defined by users of core, like a client or a server.
    ///     An example for extended resources are meshes and renderers.
    /// </summary>
    private readonly Resource extendedResource = new($"{nameof(Chunk)}Extended");

    /// <summary>
    ///     Using a local counter allows to use the tick managers after normalization without having to revert that.
    /// </summary>
    private readonly UpdateCounter localUpdateCounter = new();

    private readonly ScheduledTickManager<Block.BlockTick> blockTickManager;
    private readonly ScheduledTickManager<Fluid.FluidTick> fluidTickManager;

    /// <summary>
    ///     The block data of this chunk.
    ///     Storage layout is defined by <see cref="Section"/>.
    /// </summary>
    private UInt32[] blocks = new UInt32[BlockSize * BlockSize * BlockSize];

    private DecorationLevels decoration = DecorationLevels.None;

    private ChunkPosition location;

    private ChunkState state = null!;

    private Int32? updateIndex;
    private Int32? activeIndex;
    private Int32? completeIndex;

    /// <summary>
    ///     Create a new chunk. The chunk is not initialized.
    /// </summary>
    /// <param name="context">The chunk context.</param>
    /// <param name="createSection">The section factory.</param>
    public Chunk(ChunkContext context, SectionFactory createSection)
    {
        Context = context;

        for (var index = 0; index < SectionCount; index++)
        {
            ArraySegment<UInt32> segment = new(blocks, index * Section.Count, Section.Count);
            sections[index] = createSection(segment);
        }

        blockTickManager = new ScheduledTickManager<Block.BlockTick>(
            Block.MaxBlockTicksPerFrameAndChunk,
            localUpdateCounter);

        fluidTickManager = new ScheduledTickManager<Fluid.FluidTick>(
            Fluid.MaxFluidTicksPerFrameAndChunk,
            localUpdateCounter);

        coreResource.Released += OnResourceReleased;
        extendedResource.Released += OnResourceReleased;

        void OnResourceReleased(Object? sender, EventArgs e)
        {
            State.OnChunkResourceReleased();
        }
    }

    /// <summary>
    ///     The context in which this chunk is created.
    /// </summary>
    private ChunkContext Context { get; }

    /// <summary>
    ///     Get the decoration flags of this chunk.
    /// </summary>
    internal DecorationLevels Decoration => decoration;

    /// <summary>
    ///     Whether this chunk is generated.
    /// </summary>
    public Boolean IsGenerated => decoration.HasFlag(DecorationLevels.Center);

    /// <summary>
    ///     Whether this chunk has activated at least once since creation.
    /// </summary>
    internal Boolean HasBeenActive => completeIndex.HasValue;

    /// <summary>
    ///     Whether the chunk is currently active.
    ///     An active can write to all resources and allows sharing its access for the duration of one update.
    /// </summary>
    public Boolean IsActive
    {
        get
        {
            Debug.Assert(state != null);

            return state.IsChunkActive;
        }
    }

    /// <summary>
    /// The requests for this chunk.
    /// </summary>
    public Requests Requests { get; private set; } = null!;

    /// <summary>
    ///     Whether this chunk is requested to be loaded or generated.
    ///     It will rest in the hidden state after loading or generation.
    /// </summary>
    public Boolean IsRequestedToLoad => Requests.Level.IsLoaded;

    /// <summary>
    /// Whether this chunk is requested to be active.
    /// It will attempt to enter the active state after loading or generation.
    /// </summary>
    public Boolean IsRequestedToActivate => Requests.Level.IsActive;

    /// <summary>
    ///     Whether this chunk is requested to be simulated.
    /// </summary>
    public Boolean IsRequestedToSimulate => Requests.Level.IsSimulated;

    /// <summary>
    ///     Get the position of this chunk.
    /// </summary>
    public ChunkPosition Position => location;

    /// <summary>
    ///     The extents of a chunk.
    /// </summary>
    public static Vector3d Extents => new(BlockSize / 2f, BlockSize / 2f, BlockSize / 2f);

    /// <summary>
    ///     The world this chunk is in.
    /// </summary>
    public World World { get; private set; } = null!;

    /// <summary>
    ///     Get whether this chunk is fully decorated.
    /// </summary>
    public Boolean IsFullyDecorated => decoration == DecorationLevels.All;

    /// <summary>
    ///     The internal chunk state.
    ///     If the chunk is transitioning, it might not actually have entered the state yet.
    /// </summary>
    protected internal ChunkState State => state;

    /// <summary>
    ///     Decoration is intended to happen in global steps.
    ///     A chunk will only decorate if all its neighbors that are in a lower stage are fully decorated.
    ///     The seven stages are defined for a cube of 3x3x3 chunks in the following way:
    ///     First the three chunks in the bottom diagonal, then the three chunks in the middle diagonal.
    ///     All other chunks will then be already decorated.
    ///     Therefore, all chunks that are not on one of the two diagonals have max stage.
    /// </summary>
    internal Int32 DecorationStage
    {
        get
        {
            Int32 x = VMath.Mod(Position.X, m: 3);
            Int32 y = VMath.Mod(Position.Y, m: 3);
            Int32 z = VMath.Mod(Position.Z, m: 3);

            return x == z ? Math.Min(x + y * 3, MaxDecorationStage) : MaxDecorationStage;
        }
    }

    /// <inheritdoc />
    public static Int32 Version => 1;

    /// <inheritdoc />
    public void Serialize(Serializer serializer, IEntity.Header header)
    {
        serializer.SerializeValue(ref location);
        serializer.Serialize(ref blocks);
        serializer.Serialize(ref decoration);
        serializer.SerializeEntity(blockTickManager);
        serializer.SerializeEntity(fluidTickManager);
    }

    /// <summary>
    ///     Add a decoration level to this chunk.
    /// </summary>
    /// <param name="level">The level to add.</param>
    internal void AddDecorationLevel(DecorationLevels level)
    {
        Debug.Assert((level != DecorationLevels.Center).Implies(IsGenerated));

        decoration |= level;
    }

    /// <summary>
    ///     Initialize the chunk.
    /// </summary>
    /// <param name="world">The world in which the chunk is placed.</param>
    /// <param name="position">The position of the chunk.</param>
    public virtual void Initialize(World world, ChunkPosition position)
    {
        World = world;
        Requests = new Requests(this);

        location = position;

        blockTickManager.SetWorld(world);
        fluidTickManager.SetWorld(world);

        ChunkState.Initialize(out state, this, Context);
        tracker.Transition(from: null, state);

        for (var index = 0; index < SectionCount; index++)
            sections[index].Initialize(SectionPosition.From(Position, IndexToLocalSection(index)));

        decoration = DecorationLevels.None;

        activeIndex = null;
        completeIndex = null;
    }

    /// <summary>
    ///     Reset all state of the chunk.
    ///     This allows the chunk to be reused.
    /// </summary>
    public virtual void Reset()
    {
        Debug.Assert(!IsActive);
        Debug.Assert(!coreResource.IsAcquired);
        Debug.Assert(!extendedResource.IsAcquired);

        blockTickManager.Clear();
        blockTickManager.SetWorld(newWorld: null);

        fluidTickManager.Clear();
        fluidTickManager.SetWorld(newWorld: null);

        tracker.Transition(state, to: null);

        localUpdateCounter.Reset();

        World = null!;
        state = null!;

        location = default;

        foreach (Section section in sections)
            section.Reset();
    }

    /// <summary>
    ///     Acquire the core resource, possibly stealing it.
    ///     The core resource of a chunk are its sections and their blocks.
    /// </summary>
    /// <param name="access">The access to acquire. Must not be <see cref="Access.None" />.</param>
    /// <returns>The guard, or null if the resource could not be acquired.</returns>
    public Guard? AcquireCore(Access access)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(access != Access.None);

        (Guard core, Guard extended)? guards = ChunkState.TryStealAccess(ref state);

        if (guards is not {core: {} core, extended: {} extended}) return coreResource.TryAcquire(access);

        extended.Dispose();

        if (access == Access.Write) return core;

        // We downgrade our access to read, as stealing always gives us write access.
        core.Dispose();
        core = coreResource.TryAcquire(access);
        Debug.Assert(core != null);

        return core;
    }

    /// <summary>
    ///     Whether it is possible to acquire the core resource.
    /// </summary>
    public Boolean CanAcquireCore(Access access)
    {
        Throw.IfDisposed(disposed);

        return state.CanStealAccess || coreResource.CanAcquire(access);
    }

    /// <summary>
    ///     Check if core is held with specific access by a given guard.
    /// </summary>
    public Boolean IsCoreHeldBy(Guard guard, Access access)
    {
        Throw.IfDisposed(disposed);

        return coreResource.IsHeldBy(guard, access);
    }

    /// <summary>
    ///     Acquire the extended resource, possibly stealing it.
    ///     Extended resources are defined by users of core, like a client or a server.
    ///     An example for extended resources are meshes and renderers.
    /// </summary>
    /// <param name="access">The access to acquire. Must not be <see cref="Access.None" />.</param>
    /// <returns>The guard, or null if the resource could not be acquired.</returns>
    public Guard? AcquireExtended(Access access)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(access != Access.None);

        (Guard core, Guard extended)? guards = ChunkState.TryStealAccess(ref state);

        if (guards is not {core: {} core, extended: {} extended}) return extendedResource.TryAcquire(access);

        core.Dispose();

        if (access == Access.Write) return extended;

        // We downgrade our access to read, as stealing always gives us write access.
        extended.Dispose();
        extended = extendedResource.TryAcquire(access);
        Debug.Assert(extended != null);

        return extended;
    }

    /// <summary>
    ///     Whether it is possible to acquire the extended resource.
    /// </summary>
    public Boolean CanAcquireExtended(Access access)
    {
        Throw.IfDisposed(disposed);

        return state.CanStealAccess || extendedResource.CanAcquire(access);
    }

    /// <summary>
    ///     Check if extended is held with specific access by a given guard.
    /// </summary>
    public Boolean IsExtendedHeldBy(Guard guard, Access access)
    {
        Throw.IfDisposed(disposed);

        return extendedResource.IsHeldBy(guard, access);
    }

    /// <summary>
    ///     Used by <see cref="ChunkUpdateList" />.
    /// </summary>
    internal void SetUpdateIndex(Int32 index)
    {
        Debug.Assert(updateIndex == null);

        updateIndex = index;
    }

    /// <summary>
    ///     Used by <see cref="ChunkUpdateList" />.
    /// </summary>
    internal Boolean HasUpdateIndex()
    {
        return updateIndex != null;
    }

    /// <summary>
    ///     Used by <see cref="ChunkUpdateList" />.
    /// </summary>
    internal Int32? ClearUpdateIndex()
    {
        Int32? index = updateIndex;
        updateIndex = null;

        return index;
    }

    /// <summary>
    /// Called by <see cref="Requests"/>.
    /// </summary>
    internal void OnRequestLevelApplied()
    {
        Throw.IfDisposed(disposed);

        if (!IsRequestedToLoad) BeginSaving();
        else if (!IsRequestedToActivate) BeginHiding();
    }

    /// <summary>
    ///     Loads a chunk from a file specified by the path.
    ///     If the loaded chunk does not fit the requested x, y and z parameters, it is considered invalid.
    /// </summary>
    /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
    /// <param name="chunk">The chunk to load into.</param>
    /// <returns>The result type of the loading operation.</returns>
    private static LoadingResult Load(FileInfo path, Chunk chunk)
    {
        // Serialization might change the position of the chunk, so we need to store it before loading.
        ChunkPosition position = chunk.Position;

        LogStartedLoadingChunk(logger, position);

        Exception? exception = Serialization.Serialize.LoadBinary(path, chunk, FileSignature);

        if (exception is FileFormatException)
        {
            LogInvalidChunkFormatError(logger, position);

            return LoadingResult.FormatError;
        }

        if (exception != null)
        {
            // Because there is no check whether the file exists, IO exceptions are expected.
            // Thus, they are not logged as errors or warnings.

            LogChunkLoadError(logger, position, exception.Message);

            return LoadingResult.IOError;
        }

        LogFinishedLoadingChunk(logger, position);

        if (chunk.Position != position)
        {
            LogInvalidChunkPosition(logger, position);

            return LoadingResult.ValidationError;
        }

        LogValidChunkFile(logger, position);

        return LoadingResult.Success;
    }

    /// <summary>
    ///     Get the file name of a chunk.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The file name of the chunk.</returns>
    public static String GetChunkFileName(ChunkPosition position)
    {
        return $"x{position.X}y{position.Y}z{position.Z}.chunk";
    }

    /// <summary>
    ///     Begin saving the chunk.
    /// </summary>
    public void BeginSaving()
    {
        State.RequestNextState<Saving>();
    }

    /// <summary>
    ///     Begin to hide the chunk, so it is no longer active.
    ///     Only has an effect if the chunk is currently active.
    /// </summary>
    public void BeginHiding()
    {
        if (!IsActive) return;

        State.RequestNextState<Hidden>();
    }

    /// <summary>
    ///     Saves this chunk in the directory specified by the path.
    /// </summary>
    /// <param name="path">The path of the directory where this chunk should be saved.</param>
    private void Save(DirectoryInfo path)
    {
        Debug.Assert(IsGenerated);

        Throw.IfDisposed(disposed);

        blockTickManager.Normalize();
        fluidTickManager.Normalize();
        localUpdateCounter.Reset();

        FileInfo chunkFile = path.GetFile(GetChunkFileName(Position));

        LogStartedSavingChunk(logger, Position, chunkFile.FullName);

        chunkFile.Directory?.Create();

        Exception? exception = Serialization.Serialize.SaveBinary(this, chunkFile, FileSignature);

        if (exception != null)
            throw exception;

        LogFinishedSavingChunk(logger, Position, chunkFile.FullName);
    }

    /// <summary>
    ///     Generate the chunk content and perform initial decoration.
    /// </summary>
    public void Generate(IGenerationContext generationContext, IDecorationContext decorationContext)
    {
        Throw.IfDisposed(disposed);

        using Timer? timer = logger.BeginTimedScoped("Chunk Generation");

        LogStartedGeneratingChunk(logger, Position, generationContext.Generator.ToString());

        generationContext.Generate(this);
        decorationContext.DecorateCenter(this);

        LogFinishedGeneratingChunk(logger, Position, generationContext.Generator.ToString());
    }

    /// <summary>
    ///     Decorate the chunk, which should be called after generation.
    /// </summary>
    public void Decorate(Neighborhood<Chunk?> neighbors, IDecorationContext decorationContext)
    {
        Throw.IfDisposed(disposed);

        using Timer? timer = logger.BeginTimedScoped("Chunk Decoration");

        LogStartedDecoratingChunk(logger, Position, decorationContext.Generator.ToString());

        neighbors.Center = this;
        decorationContext.Decorate(neighbors);

        LogFinishedDecoratingChunk(logger, Position, decorationContext.Generator.ToString());
    }

    internal void ScheduleBlockTick(Block.BlockTick tick, UInt32 tickOffset)
    {
        blockTickManager.Add(tick, tickOffset);
    }

    internal void ScheduleFluidTick(Fluid.FluidTick tick, UInt32 tickOffset)
    {
        fluidTickManager.Add(tick, tickOffset);
    }

    /// <summary>
    ///     Update the state and return the new state.
    /// </summary>
    public ChunkState UpdateState()
    {
        ChunkState.Update(ref state, tracker);

        return state;
    }

    /// <summary>
    ///     Send all update events.
    ///     These include requested updates and one random update. 
    /// </summary>
    public void Tick()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(IsActive);

        blockTickManager.Process();
        fluidTickManager.Process();

        localUpdateCounter.Increment();

        Int32 index = NumberGenerator.Random.Next(minValue: 0, SectionCount);
        sections[index].SendRandomUpdate(World);
    }

    /// <summary>
    ///     Get a section of this chunk.
    /// </summary>
    /// <param name="position">The position of the section. Must be in this chunk.</param>
    /// <returns>The section.</returns>
    public Section GetSection(SectionPosition position)
    {
        Throw.IfDisposed(disposed);

        (Int32 x, Int32 y, Int32 z) = position.Local;

        return sections[LocalSectionToIndex(x, y, z)];
    }

    /// <summary>
    ///     Get a section of this chunk, that contains the given world position.
    /// </summary>
    /// <param name="position">The world position. Must be in this chunk.</param>
    /// <returns>The section containing it.</returns>
    public Section GetSection(Vector3i position)
    {
        return GetSection(SectionPosition.From(position));
    }

    /// <summary>
    ///     Get a section using local coordinates.
    /// </summary>
    public Section GetLocalSection(Int32 x, Int32 y, Int32 z)
    {
        return sections[LocalSectionToIndex(x, y, z)];
    }

    /// <summary>
    ///     Convert a three-dimensional section position (in this chunk) to a one-dimensional section index.
    /// </summary>
    protected static Int32 LocalSectionToIndex(Int32 x, Int32 y, Int32 z)
    {
        return (x << SizeExp2) + (y << SizeExp) + z;
    }

    /// <summary>
    ///     Convert a one-dimensional section index to a three-dimensional section position (in this chunk).
    /// </summary>
    public static (Int32 x, Int32 y, Int32 z) IndexToLocalSection(Int32 index)
    {
        Int32 z = index & (Size - 1);
        index = (index - z) >> SizeExp;
        Int32 y = index & (Size - 1);
        index = (index - y) >> SizeExp;
        Int32 x = index;

        return (x, y, z);
    }

    /// <inheritdoc />
    public sealed override String ToString()
    {
        return $"Chunk {Position}";
    }

    /// <inheritdoc />
    public sealed override Boolean Equals(Object? obj)
    {
        if (obj is Chunk other) return Position == other.Position;

        return false;
    }

    /// <inheritdoc />
    public sealed override Int32 GetHashCode()
    {
        return HashCode.Combine(Position);
    }

    /// <summary>
    ///     Allow this chunk to begin decoration.
    ///     The chunk will only try to decorate itself.
    ///     It will however not start decoration if a neighboring chunk is not fully decorated.
    /// </summary>
    /// <returns>The next state, if the chunk needs decoration.</returns>
    public ChunkState? ProcessDecorationOption()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(IsGenerated);

        if (IsFullyDecorated) return null;
        if (!CanAcquireCore(Access.Write)) return null;

        Neighborhood<Chunk?>? needed = IDecorationContext.DecideWhetherToDecorate(this);

        if (needed == null) return null;

        Guard? access = AcquireCore(Access.Write);
        Debug.Assert(access != null);

        var guards = new PooledList<Guard>(Neighborhood.Count);

        foreach (Chunk? chunk in needed)
        {
            if (chunk == null) continue;
            if (ReferenceEquals(chunk, this)) continue;

            Guard? guard = chunk.AcquireCore(Access.Read);
            Debug.Assert(guard != null);

            guards.Add(guard);
        }

        return new Decorating(access, guards, needed);
    }

    /// <summary>
    ///     Called after any usable state was entered.
    ///     A usable state holds write-access to both resources and allows stealing.
    /// </summary>
    internal void OnUsableState()
    {
        for (Int32 x = -1; x <= 1; x++)
        for (Int32 y = -1; y <= 1; y++)
        for (Int32 z = -1; z <= 1; z++)
            HandleUsableNeighbor(x, y, z);

        void HandleUsableNeighbor(Int32 x, Int32 y, Int32 z)
        {
            if ((x, y, z) == (0, 0, 0)) return;

            if (World.TryGetChunk(Position.Offset(x, y, z), out Chunk? neighbor)) neighbor.State.OnNeighborUsable();
        }
    }

    /// <summary>
    ///     Called after the active state was entered.
    /// </summary>
    private void OnActiveState()
    {
        Debug.Assert(activeIndex == null);

        activeIndex = World.Chunks.RegisterActive(this);
        completeIndex ??= World.Chunks.RegisterComplete(this);

        OnActivation();

        foreach (BlockSide side in BlockSide.All.Sides())
            World.GetActiveChunk(side.Offset(Position))?.OnNeighborActivation();
    }

    /// <summary>
    ///     Called before the active state is left.
    /// </summary>
    private void OnInactiveState()
    {
        Debug.Assert(activeIndex != null);

        World.Chunks.UnregisterActive(activeIndex.Value);
        activeIndex = null;

        OnDeactivation();
    }

    /// <summary>
    ///     Called on the chunk when it is released.
    /// </summary>
    public void OnRelease()
    {
        if (completeIndex == null) return;

        World.Chunks.UnregisterComplete(completeIndex.Value);
        completeIndex = null;
    }

    /// <summary>
    ///     Called after the inactive state was entered.
    /// </summary>
    protected virtual void OnActivation() {}

    /// <summary>
    ///     Called before the inactive state is left.
    /// </summary>
    protected virtual void OnDeactivation() {}

    /// <summary>
    ///     Called when a neighbor chunk was activated.
    ///     Note that this method is called only on the six direct neighbors and not on the diagonal neighbors.
    /// </summary>
    protected virtual void OnNeighborActivation() {}

    /// <summary>
    ///     Get a section by index.
    /// </summary>
    protected Section GetSectionByIndex(Int32 index)
    {
        return sections[index];
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Started loading chunk for position: {Position}")]
    private static partial void LogStartedLoadingChunk(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Error, Message = "File for the chunk at {Position} was invalid: format error")]
    private static partial void LogInvalidChunkFormatError(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Could not load chunk for position {Position}, it probably does not exist yet. Exception: {Message}")]
    private static partial void LogChunkLoadError(ILogger logger, ChunkPosition position, String message);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Finished loading chunk for position: {Position}")]
    private static partial void LogFinishedLoadingChunk(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Warning, Message = "File for the chunk at {Position} was invalid: position did not match")]
    private static partial void LogInvalidChunkPosition(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "File for the chunk at {Position} was valid")]
    private static partial void LogValidChunkFile(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Started saving chunk {Position} to: {Path}")]
    private static partial void LogStartedSavingChunk(ILogger logger, ChunkPosition position, String path);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Finished saving chunk {Position} to: {Path}")]
    private static partial void LogFinishedSavingChunk(ILogger logger, ChunkPosition position, String path);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Started generating chunk {Position} using '{Name}' generator")]
    private static partial void LogStartedGeneratingChunk(ILogger logger, ChunkPosition position, String? name);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Finished generating chunk {Position} using '{Name}' generator")]
    private static partial void LogFinishedGeneratingChunk(ILogger logger, ChunkPosition position, String? name);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Started decorating chunk {Position} using '{Name}' generator")]
    private static partial void LogStartedDecoratingChunk(ILogger logger, ChunkPosition position, String? name);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Finished decorating chunk {Position} using '{Name}' generator")]
    private static partial void LogFinishedDecoratingChunk(ILogger logger, ChunkPosition position, String? name);

    #endregion LOGGING

    #region IDisposable Support

    private Boolean disposed;

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (!disposing) return;

        foreach (Section section in sections) section.Dispose();

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Chunk()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}

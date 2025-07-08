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
using VoxelGame.Core.Actors;
using VoxelGame.Core.App;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Components;
using VoxelGame.Toolkit.Memory;
using VoxelGame.Toolkit.Utilities;
using Generator = VoxelGame.Core.Generation.Worlds.Testing.Generator;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Represents the world. Contains everything that is in the world, e.g. chunks, entities, etc.
/// </summary>
public abstract partial class World : Composed<World, WorldComponent>, IGrid
{
    /// <summary>
    ///     The largest absolute value of a block position coordinate component.
    ///     This value also describes the word extents in blocks, thus the world size is two times this value.
    ///     The actual active size of the world can be smaller, but never larger.
    /// </summary>
    public const UInt32 BlockLimit = 100_000;

    private const UInt32 SectionLimit = BlockLimit / Section.Size;
    private const UInt32 ChunkLimit = BlockLimit / Chunk.BlockSize;

    private readonly SectionChangedEventArgs pooledSectionChangedEventArgs = new(null!, Vector3i.Zero);

    private readonly WorldStateMachine state;

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
                    Version = Application.Instance.Version.ToString()
                },
                path),
            isNew: true)
    {
        Operations.Launch(async token =>
        {
            await Data.SaveAsync(token).InAnyContext();
        });

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
    private World(WorldData data, Boolean isNew)
    {
        Timer? timer = Timer.Start(isNew ? "World Setup (new)" : "World Setup (existing)", TimingStyle.Once, Profile.GetSingleUseActiveProfiler());

        state = new WorldStateMachine(this, timer);

        Data = data;

        Data.EnsureValidDirectory();
        Data.EnsureValidInformation();

        IWorldGenerator generator = GetAndInitializeGenerator(this, timer) ?? throw Exceptions.InvalidOperation("The generator could not be initialized.");

        ChunkContext = new ChunkContext(generator, CreateChunk, ProcessNewlyActivatedChunk, ProcessActivatedChunk, UnloadChunk);
        Chunks = new ChunkSet(this, ChunkContext);

        state.Initialize();

        state.Activating += OnActivate;
        state.Deactivating += OnDeactivate;
        state.Terminating += OnTerminate;

        AddComponent<ChunkSimulator>();
    }

    /// <inheritdoc />
    protected override World Self => this;

    /// <summary>
    ///     Access to the world state.
    /// </summary>
    public IWorldStates State => state;

    /// <summary>
    ///     Get the chunks of this world.
    /// </summary>
    public ChunkSet Chunks { get; }

    /// <summary>
    ///     Get the chunk context of this world.
    /// </summary>
    public ChunkContext ChunkContext { get; }

    /// <summary>
    ///     Get the stored world data.
    /// </summary>
    public WorldData Data { get; }

    /// <summary>
    ///     Get the world creation seed.
    /// </summary>
    public (Int32 upper, Int32 lower) Seed => (Data.Information.UpperSeed, Data.Information.LowerSeed);

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

        SetContent(content, position, updateFluid: true);
    }

    private void OnActivate(Object? sender, EventArgs e)
    {
        foreach (WorldComponent component in Components) component.OnActivate();
    }

    private void OnDeactivate(Object? sender, EventArgs e)
    {
        foreach (WorldComponent component in Components) component.OnDeactivate();
    }

    private void OnTerminate(Object? sender, EventArgs e)
    {
        foreach (WorldComponent component in Components) component.OnTerminate();
    }

    private void UnloadChunk(Chunk chunk)
    {
        Chunks.Unload(chunk);
    }

    private static IWorldGenerator GetAndInitializeGenerator(World world, Timer? timer)
    {
        return Generator.Create(new WorldGeneratorContext(world, timer));
    }

    /// <summary>
    ///     Emit information about of global world data for debugging.
    /// </summary>
    public Operation EmitWorldInfo(DirectoryInfo directory)
    {
        Throw.IfDisposed(disposed);

        return ChunkContext.Generator.EmitWorldInfo(directory);
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
    ///     Process chunk requests and chunk state updates.
    /// </summary>
    protected void UpdateChunks()
    {
        Chunks.ProcessRequests();

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

        SetContent(new Content(block, fluid), position, updateFluid: true);
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

        SetContent(new Content(block, fluid), position, updateFluid: false);
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
    private void SetContent(in Content content, Vector3i position, Boolean updateFluid)
    {
        Chunk? chunk = GetActiveChunk(position);

        if (chunk == null) return;

        UInt32 val = Section.Encode(content);

        chunk.GetSection(position).SetContent(position, val);

        content.Block.Block.ContentUpdate(this, position, content);
        if (updateFluid) content.Fluid.Fluid.UpdateNow(this, position, content.Fluid);

        foreach (Side side in Side.All.Sides())
        {
            Vector3i neighborPosition = side.Offset(position);

            Content? neighborContent = GetContent(neighborPosition);

            if (neighborContent == null) continue;

            (BlockInstance blockNeighbor, FluidInstance fluidNeighbor) = neighborContent.Value;

            // Side is passed out of the perspective of the block receiving the block update.
            blockNeighbor.Block.NeighborUpdate(this, neighborPosition, blockNeighbor.Data, side.Opposite());
            fluidNeighbor.Fluid.UpdateSoon(this, neighborPosition, fluidNeighbor.IsStatic);
        }

        HandleChangedSection(chunk, position);
    }

    private void HandleChangedSection(Chunk chunk, Vector3i position)
    {
        pooledSectionChangedEventArgs.Chunk = chunk;
        pooledSectionChangedEventArgs.Position = position;

        SectionChanged?.Invoke(this, pooledSectionChangedEventArgs);
    }

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

        HandleChangedSection(chunk, position);
    }

    /// <summary>
    ///     Invoked whenever the content of a section has changed.
    /// </summary>
    public event EventHandler<SectionChangedEventArgs>? SectionChanged;

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
    public static Boolean IsInLimits(ChunkPosition position)
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
    ///     Such a position can still be outside the reachable <see cref="Extents" />.
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
    /// <param name="blocks">The blocks of the chunk.</param>
    /// <param name="context">The context of the chunk.</param>
    /// <returns>The new chunk.</returns>
    protected abstract Chunk CreateChunk(NativeSegment<UInt32> blocks, ChunkContext context);

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

    /// <inheritdoc cref="ChunkSet.Request" />
    public Request? RequestChunk(ChunkPosition position, Actor actor)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(!State.IsTerminating);

        if (!IsInLimits(position)) return null;

        Request? request = Chunks.Request(position, actor);

        LogChunkRequested(logger, position);

        return request;
    }

    /// <inheritdoc cref="ChunkSet.Release" />
    public void ReleaseChunk(Request? request)
    {
        Throw.IfDisposed(disposed);

        if (request == null) return;

        Chunks.Release(request);

        LogChunkReleased(logger, request.Position);
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

        return !IsInLimits(position) ? null : Chunks.GetActive(position);
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

        chunk = Chunks.GetAny(position);

        return chunk != null;
    }

    /// <summary>
    ///     Process an update step for this world.
    /// </summary>
    /// <param name="deltaTime">Time since the last update.</param>
    /// <param name="updateTimer">A timer for profiling.</param>
    public void LogicUpdate(Double deltaTime, Timer? updateTimer)
    {
        using Timer? subTimer = logger.BeginTimedSubScoped("World LogicUpdate", updateTimer);

        using (logger.BeginTimedSubScoped("World LogicUpdate Chunks", subTimer))
        {
            UpdateChunks();
        }

        state.LogicUpdate(deltaTime, updateTimer);
    }

    /// <summary>
    ///     Called by the active state during <see cref="LogicUpdate" /> when the world is active.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    /// <param name="updateTimer">A timer for profiling.</param>
    public void OnLogicUpdateInActiveState(Double deltaTime, Timer? updateTimer)
    {
        foreach (WorldComponent component in Components) 
            component.OnLogicUpdateInActiveState(deltaTime, updateTimer);
    }

    /// <summary>
    ///     Event arguments for the <see cref="SectionChanged" /> event.
    ///     Note that this event is pooled and as such should not be kept after the event has been handled.
    /// </summary>
    /// <param name="chunk">The chunk in which the section was changed.</param>
    /// <param name="position">The position of the block that caused the section change.</param>
    public class SectionChangedEventArgs(Chunk chunk, Vector3i position) : EventArgs
    {
        /// <summary>
        ///     The chunk in which the section was changed.
        /// </summary>
        public Chunk Chunk { get; set; } = chunk;

        /// <summary>
        ///     The position of the block that caused the section change.
        /// </summary>
        public Vector3i Position { get; set; } = position;
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    [LoggerMessage(EventId = LogID.World + 0, Level = LogLevel.Information, Message = "Created new world")]
    private static partial void LogCreatedNewWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.World + 1, Level = LogLevel.Information, Message = "Loaded existing world")]
    private static partial void LogLoadedExistingWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.World + 2, Level = LogLevel.Information, Message = "Unloading world")]
    private static partial void LogUnloadingWorld(ILogger logger);

    [LoggerMessage(EventId = LogID.World + 3, Level = LogLevel.Information, Message = "World spawn position has been set to: {Position}")]
    private static partial void LogWorldSpawnPositionSet(ILogger logger, Vector3d position);

    [LoggerMessage(EventId = LogID.World + 4, Level = LogLevel.Information, Message = "World size has been set to: {Size}")]
    private static partial void LogWorldSizeSet(ILogger logger, UInt32 size);

    [LoggerMessage(EventId = LogID.World + 5, Level = LogLevel.Debug, Message = "Chunk {Position} has been requested")]
    private static partial void LogChunkRequested(ILogger logger, ChunkPosition position);

    [LoggerMessage(EventId = LogID.World + 6, Level = LogLevel.Debug, Message = "Released chunk {Position}")]
    private static partial void LogChunkReleased(ILogger logger, ChunkPosition position);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposed) return;

        if (disposing)
        {
            Chunks.Dispose();
            ChunkContext.Dispose();
        }

        disposed = true;
    }

    #endregion DISPOSABLE
}

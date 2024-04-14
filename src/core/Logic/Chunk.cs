// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A chunk, a cubic group of sections.
/// </summary>
public partial class Chunk : IDisposable, IEntity
{
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

    private const Int32 RandomTickBatchSize = SectionCount / 2;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

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
    ///     The sections in this chunk.
    /// </summary>
    private readonly Section[] sections = new Section[SectionCount];

    /// <summary>
    ///     The core resource of a chunk are its sections and their blocks.
    /// </summary>
    private readonly Resource coreResource = new(nameof(Chunk) + "Core");

    /// <summary>
    ///     Extended resources are defined by users of core, like a client or a server.
    ///     An example for extended resources are meshes and renderers.
    /// </summary>
    private readonly Resource extendedResource = new(nameof(Chunk) + "Extended");

    /// <summary>
    ///     Using a local counter allows to use the tick managers after normalization without having to revert that.
    /// </summary>
    private readonly UpdateCounter localUpdateCounter = new();

    private readonly ChunkContext context;

    private readonly ScheduledTickManager<Block.BlockTick> blockTickManager;
    private readonly ScheduledTickManager<Fluid.FluidTick> fluidTickManager;

    private DecorationLevels decoration = DecorationLevels.None;

    /// <summary>
    ///     Whether the chunk is currently requested to be active.
    /// </summary>
    private Boolean isRequested;

    /// <summary>
    ///     The current chunk state.
    /// </summary>
    private ChunkState state = null!;

    private ChunkPosition location;

    /// <summary>
    ///     Create a new chunk. The chunk is not initialized.
    /// </summary>
    /// <param name="context">The chunk context.</param>
    /// <param name="createSection">The section factory.</param>
    protected Chunk(ChunkContext context, SectionFactory createSection)
    {
        this.context = context;

        for (var index = 0; index < SectionCount; index++)
            sections[index] = createSection();

        blockTickManager = new ScheduledTickManager<Block.BlockTick>(
            Block.MaxBlockTicksPerFrameAndChunk,
            localUpdateCounter);

        fluidTickManager = new ScheduledTickManager<Fluid.FluidTick>(
            Fluid.MaxFluidTicksPerFrameAndChunk,
            localUpdateCounter);
    }

    /// <summary>
    ///     Whether the chunk is currently active.
    ///     An active can write to all resources and allows sharing its access for the duration of one update.
    /// </summary>
    public Boolean IsActive
    {
        get
        {
            Debug.Assert(state != null);

            return state.IsActive;
        }
    }

    /// <summary>
    ///     Whether this chunk is intending to get ready according to the current state.
    /// </summary>
    public Boolean IsIntendingToGetReady
    {
        get
        {
            Debug.Assert(state != null);

            return state.IsIntendingToGetReady;
        }
    }

    /// <summary>
    ///     Get whether the chunk is requested.
    /// </summary>
    public Boolean IsRequested => isRequested;

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
    ///     The current chunk state.
    /// </summary>
    protected ChunkState State => state;

    /// <inheritdoc />
    public static Int32 Version => 1;

    /// <inheritdoc />
    public void Serialize(Serializer serializer, IEntity.Header header)
    {
        serializer.SerializeValue(ref location);
        serializer.SerializeEntities(sections);
        serializer.Serialize(ref decoration);
        serializer.SerializeEntity(blockTickManager);
        serializer.SerializeEntity(fluidTickManager);
    }

    /// <summary>
    ///     Initialize the chunk.
    /// </summary>
    /// <param name="world">The world in which the chunk is placed.</param>
    /// <param name="position">The position of the chunk.</param>
    public virtual void Initialize(World world, ChunkPosition position)
    {
        World = world;
        location = position;

        blockTickManager.SetWorld(world);
        fluidTickManager.SetWorld(world);

        ChunkState.Initialize(out state, this, context);
        tracker.Transition(from: null, state);

        for (var index = 0; index < SectionCount; index++)
            sections[index].Initialize(SectionPosition.From(Position, IndexToLocalSection(index)));

        decoration = DecorationLevels.None;
        isRequested = false;
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

        (Guard core, Guard extended)? guards = ChunkState.TryStealAccess(ref state, tracker);

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
    ///     Check if core is held with a specific access by a given guard.
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

        (Guard core, Guard extended)? guards = ChunkState.TryStealAccess(ref state, tracker);

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
    ///     Check if extended is held with a specific access by a given guard.
    /// </summary>
    public Boolean IsExtendedHeldBy(Guard guard, Access access)
    {
        Throw.IfDisposed(disposed);

        return extendedResource.IsHeldBy(guard, access);
    }

    /// <summary>
    ///     Add a request to the chunk to be active.
    /// </summary>
    public void AddRequest()
    {
        Throw.IfDisposed(disposed);

        isRequested = true;
    }

    /// <summary>
    ///     Remove a request to the chunk to be active.
    /// </summary>
    public void RemoveRequest()
    {
        Throw.IfDisposed(disposed);

        isRequested = false;
        BeginSaving();
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

        logger.LogDebug(Events.ChunkOperation, "Started loading chunk for position: {Position}", position);

        Exception? exception = Serialization.Serialize.LoadBinary(path, chunk, FileSignature);

        if (exception is FileFormatException)
        {
            logger.LogError(Events.ChunkOperation, "File for the chunk at {Position} was invalid: format error", position);

            return LoadingResult.FormatError;
        }

        if (exception != null)
        {
            // Because there is no check whether the file exists, IO exceptions are expected.
            // Thus, they are not logged as errors or warnings.

            logger.LogDebug(Events.ChunkOperation, "Could not load chunk for position {Position}, it probably does not exist yet. Exception: {Message}", position, exception.Message);

            return LoadingResult.IOError;
        }

        logger.LogDebug(Events.ChunkOperation, "Finished loading chunk for position: {Position}", position);

        if (chunk.Position != position)
        {
            logger.LogWarning(Events.ChunkOperation, "File for the chunk at {Position} was invalid: position did not match", position);

            return LoadingResult.ValidationError;
        }

        logger.LogDebug(Events.ChunkOperation, "File for the chunk at {Position} was valid", position);

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
        state.RequestNextState<Saving>();
    }

    /// <summary>
    ///     Saves this chunk in the directory specified by the path.
    /// </summary>
    /// <param name="path">The path of the directory where this chunk should be saved.</param>
    private void Save(DirectoryInfo path)
    {
        Throw.IfDisposed(disposed);

        blockTickManager.Normalize();
        fluidTickManager.Normalize();
        localUpdateCounter.Reset();

        FileInfo chunkFile = path.GetFile(GetChunkFileName(Position));

        logger.LogDebug(Events.ChunkOperation, "Started saving chunk {Position} to: {Path}", Position, chunkFile);

        chunkFile.Directory?.Create();

        Exception? exception = Serialization.Serialize.SaveBinary(this, chunkFile, FileSignature);

        if (exception != null)
            throw exception;

        logger.LogDebug(Events.ChunkOperation, "Finished saving chunk {Position} to: {Path}", Position, chunkFile);
    }

    /// <summary>
    ///     Generate the chunk content.
    /// </summary>
    /// <param name="generator">The generator to use.</param>
    public void Generate(IWorldGenerator generator)
    {
        Throw.IfDisposed(disposed);

        logger.LogDebug(
            Events.ChunkOperation,
            "Started generating chunk {Position} using '{Name}' generator",
            Position,
            generator);

        GenerateContent(generator);
        PlaceStructures(generator);
        DecorateCenter(generator);

        logger.LogDebug(
            Events.ChunkOperation,
            "Finished generating chunk {Position} using '{Name}' generator",
            Position,
            generator);
    }

    private void GenerateContent(IWorldGenerator generator)
    {
        (Int32 begin, Int32 end) range = (Position.Y * BlockSize, (Position.Y + 1) * BlockSize);

        for (var x = 0; x < BlockSize; x++)
        for (var z = 0; z < BlockSize; z++)
        {
            Int32 y = range.begin;

            foreach (Content content in generator.GenerateColumn(
                         x + Position.X * BlockSize,
                         z + Position.Z * BlockSize,
                         range))
            {
                Vector3i position = (x, y, z);

                Content modifiedContent = content.Block.Block.GenerateUpdate(content);

                UInt32 encodedContent = Section.Encode(
                    modifiedContent.Block.Block,
                    modifiedContent.Block.Data,
                    modifiedContent.Fluid.Fluid,
                    modifiedContent.Fluid.Level,
                    modifiedContent.Fluid.IsStatic);

                GetSection(position).SetContent(position, encodedContent);

                y++;
            }
        }
    }

    private void PlaceStructures(IWorldGenerator generator)
    {
        for (var index = 0; index < SectionCount; index++)
        {
            Section section = sections[index];
            SectionPosition position = SectionPosition.From(Position, IndexToLocalSection(index));

            generator.GenerateStructures(section, position);
        }
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
    ///     Update the state.
    /// </summary>
    public void Update()
    {
        ChunkState.Update(ref state, tracker);
    }

    /// <summary>
    ///     Tick some random blocks.
    /// </summary>
    public void Tick()
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(IsActive);

        blockTickManager.Process();
        fluidTickManager.Process();

        localUpdateCounter.Increment();

        Int32 anchor = NumberGenerator.Random.Next(minValue: 0, SectionCount);

        for (var i = 0; i < RandomTickBatchSize; i++)
        {
            Int32 index = (anchor + i) % SectionCount;
            sections[index].SendRandomUpdates(World);
        }
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
    protected Section GetLocalSection(Int32 x, Int32 y, Int32 z)
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
    protected static (Int32 x, Int32 y, Int32 z) IndexToLocalSection(Int32 index)
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
    /// </summary>
    /// <returns>The next state, if the chunk needs decoration.</returns>
    public ChunkState? ProcessDecorationOption()
    {
        Throw.IfDisposed(disposed);

        if (!CanAcquireCore(Access.Write)) return null;

        Boolean allAvailableChunksAreFullyDecorated = IsFullyDecorated;
        Neighborhood<Chunk?> available = FindAvailableNeighbors(ref allAvailableChunksAreFullyDecorated);

        if (allAvailableChunksAreFullyDecorated) return null;

        var isAnyDecorationPossible = false;
        Neighborhood<Boolean> needed = FindNeededNeighbors(available, ref isAnyDecorationPossible);

        if (!isAnyDecorationPossible) return null;

        Guard? access = AcquireCore(Access.Write);

        if (access == null) return null;

        var neighbors = new Neighborhood<(Chunk, Guard)?>();

        foreach ((Int32 x, Int32 y, Int32 z) in Neighborhood.Indices)
        {
            if ((x, y, z) == Neighborhood.Center || !needed[x, y, z]) continue;

            Chunk? chunk = available[x, y, z];
            Debug.Assert(chunk != null);

            Guard? guard = chunk.AcquireCore(Access.Write);
            Debug.Assert(guard != null);

            neighbors[x, y, z] = (chunk, guard);
        }

        return new Decorating(access, neighbors);
    }

    private static Neighborhood<Boolean> FindNeededNeighbors(Neighborhood<Chunk?> available, ref Boolean isAnyDecorationPossible)
    {
        Debug.Assert(available.Center != null);

        Neighborhood<Boolean> needed = new();

        foreach (Vector3i corner in VMath.Range3(x: 2, y: 2, z: 2))
        {
            if (IsCornerDecorated(corner, available)) continue;

            var isCornerAvailable = true;

            foreach (Vector3i offset in VMath.Range3(x: 2, y: 2, z: 2))
                isCornerAvailable &= available.GetAt(corner + offset) != null;

            if (!isCornerAvailable) continue;

            isAnyDecorationPossible = true;

            foreach (Vector3i offset in VMath.Range3(x: 2, y: 2, z: 2))
                needed.SetAt(corner + offset, value: true);
        }

        return needed;
    }

    private static Boolean IsCornerDecorated(Vector3i corner, Array3D<Chunk?> chunks)
    {
        var decorated = true;

        foreach ((Vector3i position, DecorationLevels flag) in GetCornerPositions(corner))
            // Use true as default because chunks that do not exist are considered decorated for the purpose of this method.
            // This is because chunks that do not exist could not be used for decoration anyway.
            decorated &= chunks.GetAt(position)?.decoration.HasFlag(flag) ?? true;

        return decorated;
    }

    private Neighborhood<Chunk?> FindAvailableNeighbors(ref Boolean isFullyDecorated)
    {
        var available = new Neighborhood<Chunk?>();

        foreach ((Int32 x, Int32 y, Int32 z) in Neighborhood.Indices)
            if ((x, y, z) == Neighborhood.Center)
            {
                available[x, y, z] = this;
            }
            else
            {
                Boolean neighborExists = World.TryGetChunk(Position.Offset((x, y, z) - Neighborhood.Center), out Chunk? neighbor);

                if (neighborExists)
                {
                    Debug.Assert(neighbor != null);

                    isFullyDecorated &= neighbor.IsFullyDecorated;
                    available[x, y, z] = neighbor.CanAcquireCore(Access.Write) ? neighbor : null;
                }
                else
                {
                    available[x, y, z] = null;
                }
            }

        return available;
    }

    private static void Decorate(IWorldGenerator generator, Neighborhood<Chunk?> neighbors)
    {
        foreach (Vector3i corner in VMath.Range3(x: 2, y: 2, z: 2))
        {
            if (IsCornerDecorated(corner, neighbors)) continue;

            var isCornerAvailable = true;

            foreach ((Vector3i position, _) in GetCornerPositions(corner))
                isCornerAvailable &= neighbors.GetAt(position) != null;

            if (!isCornerAvailable) continue;

            DecorateCorner(generator, neighbors, corner);
        }
    }

    private void DecorateCenter(IWorldGenerator generator)
    {
        Debug.Assert(!decoration.HasFlag(DecorationLevels.Center));

        decoration |= DecorationLevels.Center;

        var neighbors = new Array3D<Section>(length: 3);

        void SetNeighbors(Int32 x, Int32 y, Int32 z)
        {
            Debug.Assert(neighbors != null);
            foreach ((Int32 dx, Int32 dy, Int32 dz) in Neighborhood.Indices) neighbors[dx, dy, dz] = GetLocalSection(x + dx - 1, y + dy - 1, z + dz - 1);
        }

        void DecorateSection(Int32 x, Int32 y, Int32 z)
        {
            SetNeighbors(x, y, z);
            generator.DecorateSection(SectionPosition.From(Position, (x, y, z)), neighbors);
        }

        foreach ((Int32 x, Int32 y, Int32 z) in VMath.Range3(x: 2, y: 2, z: 2)) DecorateSection(1 + x, 1 + y, 1 + z);
    }

    private static DecorationLevels GetFlagForCorner(Vector3i corner)
    {
        return (corner.X, corner.Y, corner.Z) switch
        {
            (0, 0, 0) => DecorationLevels.Corner000,
            (0, 0, 1) => DecorationLevels.Corner001,
            (0, 1, 0) => DecorationLevels.Corner010,
            (0, 1, 1) => DecorationLevels.Corner011,
            (1, 0, 0) => DecorationLevels.Corner100,
            (1, 0, 1) => DecorationLevels.Corner101,
            (1, 1, 0) => DecorationLevels.Corner110,
            (1, 1, 1) => DecorationLevels.Corner111,
            _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, message: null)
        };
    }

    private static void DecorateCorner(IWorldGenerator generator, Neighborhood<Chunk?> chunks, Vector3i corner)
    {
        Neighborhood<Boolean> decorated = new();

        foreach ((Vector3i position, DecorationLevels flag) in GetCornerPositions(corner))
        {
            Chunk? chunk = chunks.GetAt(position);
            Debug.Assert(chunk != null);

            decorated.SetAt(position, chunk.decoration.HasFlag(flag));
            chunk.decoration |= flag;
        }

        // Go trough all sections on the selected corner.
        // We want to decorate 56 of them, which is a cube of 4x4x4 without the corners.
        // The corners of this cube are the centers of the chunks - the cube overlaps with multiple chunks.

        ChunkPosition first = chunks.Center!.Position.Offset(xOffset: -1, yOffset: -1, zOffset: -1);

        Section GetSection(SectionPosition sectionPosition)
        {
            Vector3i offset = first.OffsetTo(sectionPosition.Chunk);

            return chunks.GetAt(offset)!.GetSection(sectionPosition);
        }

        Boolean IsDecorated(SectionPosition sectionPosition)
        {
            Vector3i offset = first.OffsetTo(sectionPosition.Chunk);

            return decorated.GetAt(offset);
        }

        Neighborhood<Section> GetNeighbors(SectionPosition sectionPosition)
        {
            Neighborhood<Section> neighbors = new();

            foreach ((Int32 dx, Int32 dy, Int32 dz) in Neighborhood.Indices) neighbors[dx, dy, dz] = GetSection(sectionPosition.Offset(dx - 1, dy - 1, dz - 1));

            return neighbors;
        }

        void DecorateSection(SectionPosition sectionPosition)
        {
            Neighborhood<Section> neighbors = GetNeighbors(sectionPosition);
            generator.DecorateSection(sectionPosition, neighbors);
        }

        SectionPosition lowCorner = SectionPosition.From(chunks.GetAt(corner)!.Position, (Size - 2, Size - 2, Size - 2));

        foreach ((Int32 dx, Int32 dy, Int32 dz) in VMath.Range3(x: 4, y: 4, z: 4))
        {
            if (IsCorner(dx, dy, dz)) continue;
            if (IsDecorated(lowCorner.Offset(dx, dy, dz))) continue;

            DecorateSection(lowCorner.Offset(dx, dy, dz));
        }

        Debug.Assert(IsCornerDecorated(corner, chunks));
    }

    private static IEnumerable<(Vector3i, DecorationLevels)> GetCornerPositions(Vector3i corner)
    {
        foreach (Vector3i offset in VMath.Range3(x: 2, y: 2, z: 2))
            yield return (corner + offset, GetFlagForCorner(Neighborhood.Center - offset));
    }

    private static Boolean IsCorner(Int32 dx, Int32 dy, Int32 dz)
    {
        return (dx, dy, dz) switch
        {
            (0, 0, 0) => true,
            (0, 0, 3) => true,
            (0, 3, 0) => true,
            (0, 3, 3) => true,
            (3, 0, 0) => true,
            (3, 0, 3) => true,
            (3, 3, 0) => true,
            (3, 3, 3) => true,
            _ => false
        };
    }

    /// <summary>
    ///     Called after the active state was entered.
    /// </summary>
    private void OnActiveState()
    {
        OnActivation();

        foreach (BlockSide side in BlockSide.All.Sides()) World.GetActiveChunk(side.Offset(Position))?.OnNeighborActivation(this);
    }

    /// <summary>
    ///     Called before the active state is left.
    /// </summary>
    private void OnInactiveState()
    {
        OnDeactivation();
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
    protected virtual void OnNeighborActivation(Chunk neighbor) {}

    /// <summary>
    ///     Get a section by index.
    /// </summary>
    protected Section GetSectionByIndex(Int32 index)
    {
        return sections[index];
    }

    /// <summary>
    ///     Creates a section.
    /// </summary>
    protected delegate Section SectionFactory();

    [Flags]
    private enum DecorationLevels
    {
        None = 0,

        Center = 1 << 0,

        Corner000 = 1 << 1,
        Corner001 = 1 << 2,
        Corner010 = 1 << 3,
        Corner011 = 1 << 4,
        Corner100 = 1 << 5,
        Corner101 = 1 << 6,
        Corner110 = 1 << 7,
        Corner111 = 1 << 8,

        AllCorners = Corner000 | Corner001 | Corner010 | Corner011 | Corner100 | Corner101 | Corner110 | Corner111,
        All = Center | AllCorners
    }

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

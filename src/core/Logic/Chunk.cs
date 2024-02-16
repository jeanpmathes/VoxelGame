// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A chunk, a cubic group of sections.
/// </summary>
[Serializable]
public partial class Chunk : IDisposable
{
    /// <summary>
    ///     The number of sections in a chunk along every axis.
    /// </summary>
    public const int Size = 4;

    /// <summary>
    ///     The number of blocks in a chunk along every axis.
    /// </summary>
    public const int BlockSize = Size * Section.Size;

    /// <summary>
    ///     The number of sections per chunk.
    /// </summary>
    public const int SectionCount = Size * Size * Size;

    private const int RandomTickBatchSize = SectionCount / 2;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    /// <summary>
    ///     Result of <c>lb(Size)</c> as int.
    /// </summary>
    public static readonly int SizeExp = (int) Math.Log(Size, newBase: 2);

    /// <summary>
    ///     Result of <c>lb(Size) * 2</c> as int.
    /// </summary>
    public static readonly int SizeExp2 = (int) Math.Log(Size, newBase: 2) * 2;

    /// <summary>
    ///     Result of <c>lb(BlockSize)</c> as int.
    /// </summary>
    public static readonly int BlockSizeExp = (int) Math.Log(BlockSize, newBase: 2);

    /// <summary>
    ///     Result of <c>lb(BlockSize) * 2</c> as int.
    /// </summary>
    public static readonly int BlockSizeExp2 = (int) Math.Log(BlockSize, newBase: 2) * 2;

    /// <summary>
    ///     The sections in this chunk.
    /// </summary>
    private readonly Section[] sections = new Section[SectionCount];

    private DecorationLevels decoration = DecorationLevels.None;

    /// <summary>
    ///     The core resource of a chunk are its sections and their blocks.
    /// </summary>
    [NonSerialized] private Resource coreResource = new(nameof(Chunk) + "Core");

    /// <summary>
    ///     Extended resources are defined by users of core, like a client or a server.
    ///     An example for extended resources are meshes and renderers.
    /// </summary>
    [NonSerialized] private Resource extendedResource = new(nameof(Chunk) + "Extended");

    private ScheduledTickManager<Block.BlockTick> blockTickManager;
    private ScheduledTickManager<Fluid.FluidTick> fluidTickManager;

    /// <summary>
    ///     Whether the chunk is currently requested to be active.
    /// </summary>
    [NonSerialized] private bool isRequested;

    /// <summary>
    ///     Using a local counter allows to use the tick managers after normalization without having to revert that.
    /// </summary>
    [NonSerialized] private UpdateCounter localUpdateCounter = new();

    /// <summary>
    ///     The current chunk state.
    /// </summary>
    [NonSerialized] private ChunkState state;

    /// <summary>
    ///     Create a new chunk.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The chunk position.</param>
    /// <param name="context">The chunk context.</param>
    /// <param name="createSection">The section factory.</param>
    protected Chunk(World world, ChunkPosition position, ChunkContext context, SectionFactory createSection)
    {
        World = world;
        Position = position;

        for (var index = 0; index < SectionCount; index++) sections[index] = createSection(SectionPosition.From(Position, IndexToLocalSection(index)));

        blockTickManager = new ScheduledTickManager<Block.BlockTick>(
            Block.MaxBlockTicksPerFrameAndChunk,
            World,
            localUpdateCounter);

        fluidTickManager = new ScheduledTickManager<Fluid.FluidTick>(
            Fluid.MaxFluidTicksPerFrameAndChunk,
            World,
            localUpdateCounter);

        ChunkState.Initialize(out state, this, context);
    }

    /// <summary>
    ///     Whether the chunk is currently active.
    ///     An active can write to all resources and allows sharing its access for the duration of one update.
    /// </summary>
    public bool IsActive => state.IsActive;

    /// <summary>
    ///     Whether this chunk is intending to get ready according to the current state.
    /// </summary>
    public bool IsIntendingToGetReady => state.IsIntendingToGetReady;

    /// <summary>
    ///     Get whether the chunk is requested.
    /// </summary>
    public bool IsRequested => isRequested;

    /// <summary>
    ///     Get the position of this chunk.
    /// </summary>
    public ChunkPosition Position { get; }

    /// <summary>
    ///     The extents of a chunk.
    /// </summary>
    public static Vector3d Extents => new(BlockSize / 2f, BlockSize / 2f, BlockSize / 2f);

    /// <summary>
    ///     The world this chunk is in.
    /// </summary>
    [field: NonSerialized] public World World { get; private set; }

    /// <summary>
    ///     Get whether this chunk is fully decorated.
    /// </summary>
    public bool IsFullyDecorated => decoration == DecorationLevels.All;

    /// <summary>
    ///     The current chunk state.
    /// </summary>
    protected ChunkState State => state;

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

        if (access == Access.Read)
        {
            // We downgrade our access to read, as stealing always gives us write access.
            core.Dispose();
            core = coreResource.TryAcquire(access);
            Debug.Assert(core != null);
        }

        return core;
    }

    /// <summary>
    ///     Whether it is possible to acquire the core resource.
    /// </summary>
    public bool CanAcquireCore(Access access)
    {
        Throw.IfDisposed(disposed);

        return state.CanStealAccess || coreResource.CanAcquire(access);
    }

    /// <summary>
    ///     Check if core is held with a specific access by a given guard.
    /// </summary>
    public bool IsCoreHeldBy(Guard guard, Access access)
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

        if (access == Access.Read)
        {
            // We downgrade our access to read, as stealing always gives us write access.
            extended.Dispose();
            extended = extendedResource.TryAcquire(access);
            Debug.Assert(extended != null);
        }

        return extended;
    }

    /// <summary>
    ///     Whether it is possible to acquire the extended resource.
    /// </summary>
    public bool CanAcquireExtended(Access access)
    {
        Throw.IfDisposed(disposed);

        return state.CanStealAccess || extendedResource.CanAcquire(access);
    }

    /// <summary>
    ///     Check if extended is held with a specific access by a given guard.
    /// </summary>
    public bool IsExtendedHeldBy(Guard guard, Access access)
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
    ///     Setup the chunk and used sections after loading.
    /// </summary>
    public void Setup(Chunk loaded)
    {
        Throw.IfDisposed(disposed);

        VMath.Move(out blockTickManager, ref loaded.blockTickManager);
        VMath.Move(out fluidTickManager, ref loaded.fluidTickManager);

        decoration = loaded.decoration;

        blockTickManager.Setup(World, localUpdateCounter);
        fluidTickManager.Setup(World, localUpdateCounter);

        for (var s = 0; s < SectionCount; s++) sections[s].Setup(loaded.sections[s]);

        // Loaded chunk is not disposed because this chunk takes ownership of the resources.
    }

    /// <summary>
    ///     Loads a chunk from a file specified by the path.
    ///     If the loaded chunk does not fit the x, y and z parameters, it is considered invalid.
    /// </summary>
    /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The loading result.</returns>
    [SuppressMessage(
        "ReSharper.DPA",
        "DPA0002: Excessive memory allocations in SOH",
        Justification = "Chunks are allocated here.")]
    public static LoadingResult Load(FileInfo path, ChunkPosition position)
    {
        logger.LogDebug(Events.ChunkOperation, "Started loading chunk for position: {Position}", position);

        Chunk chunk;

        try
        {
            using Stream stream = path.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

            IFormatter formatter = new BinaryFormatter();

 #pragma warning disable // Will be replaced with custom serialization
            // Allocation issue flagged here, remove suppression when serialization and deserialization is reworked.
            chunk = (Chunk) formatter.Deserialize(stream);
 #pragma warning restore
        }
        catch (IOException e)
        {
            // Because there is no check whether the file exists, IO exceptions are expected.
            // Thus, they are not logged as errors or warnings.
            logger.LogDebug("Could not load chunk for position {Position}, it probably does not exist yet. Exception: {Message}", position, e.Message);

            return new FileError();
        }

        logger.LogDebug(Events.ChunkOperation, "Finished loading chunk for position: {Position}", position);

        if (chunk.Position == position) return new Success(chunk);

        logger.LogWarning("File for the chunk at {Position} was invalid: position did not match", position);

        return new Invalid();
    }

    /// <summary>
    ///     Runs a task that loads a chunk from a file specified by the path.
    /// </summary>
    /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>A task containing the loading result.</returns>
    public static Task<LoadingResult> LoadAsync(FileInfo path, ChunkPosition position)
    {
        return Task.Run(() => Load(path, position));
    }

    /// <summary>
    ///     Get the file name of a chunk.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The file name of the chunk.</returns>
    public static string GetChunkFileName(ChunkPosition position)
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
    public void Save(DirectoryInfo path)
    {
        Throw.IfDisposed(disposed);

        blockTickManager.Normalize();
        fluidTickManager.Normalize();
        localUpdateCounter.Reset();

        FileInfo chunkFile = path.GetFile(GetChunkFileName(Position));

        logger.LogDebug(Events.ChunkOperation, "Started saving chunk {Position} to: {Path}", Position, chunkFile);

        chunkFile.Directory?.Create();

        using Stream stream = chunkFile.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        IFormatter formatter = new BinaryFormatter();
#pragma warning disable // Will be replaced with custom serialization
        formatter.Serialize(stream, this);
#pragma warning restore

        logger.LogDebug(Events.ChunkOperation, "Finished saving chunk {Position} to: {Path}", Position, chunkFile);
    }

    /// <summary>
    ///     Runs a task which saves this chunk in the directory specified by the path.
    /// </summary>
    /// <param name="path">The path of the directory where this chunk should be saved.</param>
    /// <returns>A task.</returns>
    public Task SaveAsync(DirectoryInfo path)
    {
        Throw.IfDisposed(disposed);

        return Task.Run(() => Save(path));
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
        (int begin, int end) range = (Position.Y * BlockSize, (Position.Y + 1) * BlockSize);

        for (var x = 0; x < BlockSize; x++)
        for (var z = 0; z < BlockSize; z++)
        {
            int y = range.begin;

            foreach (Content content in generator.GenerateColumn(
                         x + Position.X * BlockSize,
                         z + Position.Z * BlockSize,
                         range))
            {
                Vector3i position = (x, y, z);

                Content modifiedContent = content.Block.Block.GenerateUpdate(content);

                uint encodedContent = Section.Encode(modifiedContent.Block.Block, modifiedContent.Block.Data, modifiedContent.Fluid.Fluid, modifiedContent.Fluid.Level, modifiedContent.Fluid.IsStatic);
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

    /// <summary>
    ///     Run a chunk generation task.
    /// </summary>
    /// <param name="generator">The generator to use.</param>
    /// <returns>The task.</returns>
    public Task GenerateAsync(IWorldGenerator generator)
    {
        Throw.IfDisposed(disposed);

        return Task.Run(() => Generate(generator));
    }

    internal void ScheduleBlockTick(Block.BlockTick tick, uint tickOffset)
    {
        blockTickManager.Add(tick, tickOffset);
    }

    internal void ScheduleFluidTick(Fluid.FluidTick tick, uint tickOffset)
    {
        fluidTickManager.Add(tick, tickOffset);
    }

    /// <summary>
    ///     Update the state.
    /// </summary>
    public void Update()
    {
        ChunkState.Update(ref state);
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

        int anchor = NumberGenerator.Random.Next(minValue: 0, SectionCount);

        for (var i = 0; i < RandomTickBatchSize; i++)
        {
            int index = (anchor + i) % SectionCount;
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

        (int x, int y, int z) = position.Local;

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
    protected Section GetLocalSection(int x, int y, int z)
    {
        return sections[LocalSectionToIndex(x, y, z)];
    }

    /// <summary>
    ///     Convert a three-dimensional section position (in this chunk) to a one-dimensional section index.
    /// </summary>
    protected static int LocalSectionToIndex(int x, int y, int z)
    {
        return (x << SizeExp2) + (y << SizeExp) + z;
    }

    /// <summary>
    ///     Convert a one-dimensional section index to a three-dimensional section position (in this chunk).
    /// </summary>
    protected static (int x, int y, int z) IndexToLocalSection(int index)
    {
        int z = index & (Size - 1);
        index = (index - z) >> SizeExp;
        int y = index & (Size - 1);
        index = (index - y) >> SizeExp;
        int x = index;

        return (x, y, z);
    }

    /// <inheritdoc />
    public sealed override string ToString()
    {
        return $"Chunk {Position}";
    }

    /// <inheritdoc />
    public sealed override bool Equals(object? obj)
    {
        if (obj is Chunk other) return Position == other.Position;

        return false;
    }

    /// <inheritdoc />
    public sealed override int GetHashCode()
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

        bool allAvailableChunksAreFullyDecorated = IsFullyDecorated;
        Neighborhood<Chunk?> available = FindAvailableNeighbors(ref allAvailableChunksAreFullyDecorated);

        if (allAvailableChunksAreFullyDecorated) return null;

        var isAnyDecorationPossible = false;
        Neighborhood<bool> needed = FindNeededNeighbors(available, ref isAnyDecorationPossible);

        if (!isAnyDecorationPossible) return null;

        Guard? access = AcquireCore(Access.Write);

        if (access == null) return null;

        var neighbors = new Neighborhood<(Chunk, Guard)?>();

        foreach ((int x, int y, int z) in Neighborhood.Indices)
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

    private static Neighborhood<bool> FindNeededNeighbors(Neighborhood<Chunk?> available, ref bool isAnyDecorationPossible)
    {
        Debug.Assert(available.Center != null);

        Neighborhood<bool> needed = new();

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

    private static bool IsCornerDecorated(Vector3i corner, Array3D<Chunk?> chunks)
    {
        var decorated = true;

        foreach ((Vector3i position, DecorationLevels flag) in GetCornerPositions(corner))
            // Use true as default because chunks that do not exist are considered decorated for the purpose of this method.
            // This is because chunks that do not exist could not be used for decoration anyway.
            decorated &= chunks.GetAt(position)?.decoration.HasFlag(flag) ?? true;

        return decorated;
    }

    private Neighborhood<Chunk?> FindAvailableNeighbors(ref bool isFullyDecorated)
    {
        var available = new Neighborhood<Chunk?>();

        foreach ((int x, int y, int z) in Neighborhood.Indices)
            if ((x, y, z) == Neighborhood.Center)
            {
                available[x, y, z] = this;
            }
            else
            {
                bool neighborExists = World.TryGetChunk(Position.Offset((x, y, z) - Neighborhood.Center), out Chunk? neighbor);

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

    /// <summary>
    ///     Decorate the chunk with the given neighbors. If enough neighbors are available, the chunk will be fully decorated.
    /// </summary>
    /// <param name="generator">The world generator.</param>
    /// <param name="neighbors">The neighbors of this chunk.</param>
    /// <returns>The task that decorates the chunk.</returns>
    public Task DecorateAsync(IWorldGenerator generator, Neighborhood<Chunk?> neighbors)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(ReferenceEquals(neighbors.Center, this));

        return Task.Run(() => Decorate(generator, neighbors));
    }

    private void DecorateCenter(IWorldGenerator generator)
    {
        Debug.Assert(!decoration.HasFlag(DecorationLevels.Center));

        decoration |= DecorationLevels.Center;

        var neighbors = new Array3D<Section>(length: 3);

        void SetNeighbors(int x, int y, int z)
        {
            Debug.Assert(neighbors != null);
            foreach ((int dx, int dy, int dz) in Neighborhood.Indices) neighbors[dx, dy, dz] = GetLocalSection(x + dx - 1, y + dy - 1, z + dz - 1);
        }

        void DecorateSection(int x, int y, int z)
        {
            SetNeighbors(x, y, z);
            generator.DecorateSection(SectionPosition.From(Position, (x, y, z)), neighbors);
        }

        foreach ((int x, int y, int z) in VMath.Range3(x: 2, y: 2, z: 2)) DecorateSection(1 + x, 1 + y, 1 + z);
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
        Neighborhood<bool> decorated = new();

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

        ChunkPosition first = chunks.Center!.Position.Offset(x: -1, y: -1, z: -1);

        Section GetSection(SectionPosition sectionPosition)
        {
            Vector3i offset = first.OffsetTo(sectionPosition.Chunk);

            return chunks.GetAt(offset)!.GetSection(sectionPosition);
        }

        bool IsDecorated(SectionPosition sectionPosition)
        {
            Vector3i offset = first.OffsetTo(sectionPosition.Chunk);

            return decorated.GetAt(offset);
        }

        Neighborhood<Section> GetNeighbors(SectionPosition sectionPosition)
        {
            Neighborhood<Section> neighbors = new();

            foreach ((int dx, int dy, int dz) in Neighborhood.Indices) neighbors[dx, dy, dz] = GetSection(sectionPosition.Offset(dx - 1, dy - 1, dz - 1));

            return neighbors;
        }

        void DecorateSection(SectionPosition sectionPosition)
        {
            Neighborhood<Section> neighbors = GetNeighbors(sectionPosition);
            generator.DecorateSection(sectionPosition, neighbors);
        }

        SectionPosition lowCorner = SectionPosition.From(chunks.GetAt(corner)!.Position, (Size - 2, Size - 2, Size - 2));

        foreach ((int dx, int dy, int dz) in VMath.Range3(x: 4, y: 4, z: 4))
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

    private static bool IsCorner(int dx, int dy, int dz)
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
    protected Section GetSectionByIndex(int index)
    {
        return sections[index];
    }

    /// <summary>
    ///     The result of a chunk loading operation.
    /// </summary>
    public record LoadingResult;

    /// <summary>
    ///     A successful chunk loading operation.
    /// </summary>
    /// <param name="Chunk">The loaded chunk.</param>
    public record Success(Chunk Chunk) : LoadingResult;

    /// <summary>
    ///     A chunk loading operation that failed due to an IO error.
    /// </summary>
    public record FileError : LoadingResult;

    /// <summary>
    ///     A chunk loading operation that failed due to an invalid chunk.
    /// </summary>
    public record Invalid : LoadingResult;

    /// <summary>
    ///     Creates a section.
    /// </summary>
    protected delegate Section SectionFactory(SectionPosition position);

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

    [NonSerialized] private bool disposed;

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    protected virtual void Dispose(bool disposing)
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

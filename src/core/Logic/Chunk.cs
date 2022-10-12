// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A chunk, a cubic group of sections.
/// </summary>
[Serializable]
public abstract partial class Chunk : IDisposable
{
    /// <summary>
    /// The number of sections in a chunk along every axis.
    /// </summary>
    public const int Size = 4;

    /// <summary>
    /// The number of blocks in a chunk along every axis.
    /// </summary>
    public const int BlockSize = Size * Section.Size;

    /// <summary>
    /// The number of sections per chunk.
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

    private ScheduledTickManager<Block.BlockTick> blockTickManager;
    private ScheduledTickManager<Fluid.FluidTick> fluidTickManager;

    /// <summary>
    ///     Create a new chunk.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The chunk position.</param>
    /// <param name="context">The chunk context.</param>
    protected Chunk(World world, ChunkPosition position, ChunkContext context)
    {
        World = world;
        Position = position;

        for (var s = 0; s < SectionCount; s++)
        {
#pragma warning disable S1699 // Constructors should only call non-overridable methods
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            sections[s] = CreateSection();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
#pragma warning restore S1699 // Constructors should only call non-overridable methods
        }

        blockTickManager = new ScheduledTickManager<Block.BlockTick>(
            Block.MaxBlockTicksPerFrameAndChunk,
            World,
            World.UpdateCounter);

        fluidTickManager = new ScheduledTickManager<Fluid.FluidTick>(
            Fluid.MaxFluidTicksPerFrameAndChunk,
            World,
            World.UpdateCounter);

        state = ChunkState.CreateInitialState(this, context);
    }

    /// <summary>
    ///     Whether the chunk is currently active.
    /// </summary>
    public bool IsActive => state.IsActive;

    /// <summary>
    ///     Get whether the chunk is requested.
    /// </summary>
    public bool IsRequested => isRequested;

    /// <summary>
    /// Get the position of this chunk.
    /// </summary>
    public ChunkPosition Position { get; }

    /// <summary>
    ///     Gets the position of the chunk as a point located in the center of the chunk.
    /// </summary>
    public Vector3d ChunkPoint => Position.Center;

    /// <summary>
    ///     The extents of a chunk.
    /// </summary>
    public static Vector3d ChunkExtents => new(BlockSize / 2f, BlockSize / 2f, BlockSize / 2f);

    /// <summary>
    ///     The world this chunk is in.
    /// </summary>
    [field: NonSerialized] protected World World { get; private set; }

    /// <summary>
    ///     Add a request to the chunk to be active.
    /// </summary>
    public void AddRequest()
    {
        isRequested = true;
    }

    /// <summary>
    ///     Remove a request to the chunk to be active.
    /// </summary>
    public void RemoveRequest()
    {
        isRequested = false;
    }

    /// <summary>
    ///     Creates a section.
    /// </summary>
    protected abstract Section CreateSection();

    /// <summary>
    ///     Setup the chunk and used sections after loading.
    /// </summary>
    public void Setup(Chunk loaded)
    {
        blockTickManager = loaded.blockTickManager;
        fluidTickManager = loaded.fluidTickManager;

        blockTickManager.Setup(World, World.UpdateCounter);
        fluidTickManager.Setup(World, World.UpdateCounter);

        for (var s = 0; s < SectionCount; s++)
        {
            sections[s] = loaded.sections[s];
            sections[s].Setup(World);
        }
    }

    /// <summary>
    ///     Loads a chunk from a file specified by the path. If the loaded chunk does not fit the x, y and z parameters, null is
    ///     returned.
    /// </summary>
    /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
    [SuppressMessage(
        "ReSharper.DPA",
        "DPA0002: Excessive memory allocations in SOH",
        Justification = "Chunks are allocated here.")]
    public static Chunk? Load(string path, ChunkPosition position)
    {
        logger.LogDebug(Events.ChunkOperation, "Loading chunk for position: {Position}", position);

        Chunk chunk;

        using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            IFormatter formatter = new BinaryFormatter();

 #pragma warning disable // Will be replaced with custom serialization
            // Allocation issue flagged here, remove suppression when serialization and deserialization is reworked.
            chunk = (Chunk) formatter.Deserialize(stream);
 #pragma warning restore
        }

        // Checking the chunk
        if (chunk.Position == position) return chunk;

        logger.LogWarning("File for the chunk at {Position} was invalid: position did not match", position);

        return null;
    }

    /// <summary>
    ///     Runs a task that loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z
    ///     parameters, null is returned.
    /// </summary>
    /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>A task containing the loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
    public static Task<Chunk?> LoadAsync(string path, ChunkPosition position)
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
    public void Save(string path)
    {
        blockTickManager.Unload();
        fluidTickManager.Unload();

        string chunkFile = Path.Combine(path, GetChunkFileName(Position));

        logger.LogDebug(Events.ChunkOperation, "Saving the chunk {Position} to: {Path}", Position, chunkFile);

        using Stream stream = new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        IFormatter formatter = new BinaryFormatter();
#pragma warning disable // Will be replaced with custom serialization
        formatter.Serialize(stream, this);
#pragma warning restore

        blockTickManager.Load();
        fluidTickManager.Load();
    }

    /// <summary>
    ///     Runs a task which saves this chunk in the directory specified by the path.
    /// </summary>
    /// <param name="path">The path of the directory where this chunk should be saved.</param>
    /// <returns>A task.</returns>
    public Task SaveAsync(string path)
    {
        return Task.Run(() => Save(path));
    }

    /// <summary>
    ///     Generate the chunk content.
    /// </summary>
    /// <param name="generator">The generator to use.</param>
    public void Generate(IWorldGenerator generator)
    {
        logger.LogDebug(
            Events.ChunkOperation,
            "Generating the chunk {Position} using '{Name}' generator",
            Position,
            generator);

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

    /// <summary>
    ///     Run a chunk generation task.
    /// </summary>
    /// <param name="generator">The generator to use.</param>
    /// <returns>The task.</returns>
    public Task GenerateAsync(IWorldGenerator generator)
    {
        return Task.Run(() => Generate(generator));
    }

    internal void ScheduleBlockTick(Block.BlockTick tick, int tickOffset)
    {
        blockTickManager.Add(tick, tickOffset);
    }

    internal void ScheduleFluidTick(Fluid.FluidTick tick, int tickOffset)
    {
        fluidTickManager.Add(tick, tickOffset);
    }

    /// <summary>
    ///     Update the state.
    /// </summary>
    public void Update()
    {
        ChunkState previousState = state;
        state = previousState.Update();

        if (previousState == state) return;

        state.OnEnter();
        logger.LogDebug(Events.ChunkOperation, "Chunk {Position} state changed from {PreviousState} to {State}", Position, previousState, state);
    }

    /// <summary>
    /// Tick some random blocks.
    /// </summary>
    public void Tick()
    {
        blockTickManager.Process();
        fluidTickManager.Process();

        int anchor = NumberGenerator.Random.Next(minValue: 0, SectionCount);

        for (var i = 0; i < RandomTickBatchSize; i++)
        {
            int index = (anchor + i) % SectionCount;
            sections[index].SendRandomUpdates(SectionPosition.From(Position, IndexToLocalSection(index)));
        }
    }

    /// <summary>
    /// Get a section of this chunk.
    /// </summary>
    /// <param name="position">The position of the section. Must be in this chunk.</param>
    /// <returns>The section.</returns>
    public Section GetSection(SectionPosition position)
    {
        (int x, int y, int z) = position.GetLocal();

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

#pragma warning disable CA1051 // Do not declare visible instance fields
    /// <summary>
    ///     The sections in this chunk.
    /// </summary>
    protected readonly Section[] sections = new Section[SectionCount];

    /// <summary>
    ///     Whether the chunk is currently requested to be active.
    /// </summary>
    [NonSerialized] protected bool isRequested;

    /// <summary>
    ///     The current chunk state.
    /// </summary>
    [NonSerialized] protected ChunkState state;
#pragma warning restore CA1051 // Do not declare visible instance fields

    #region IDisposable Support

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    protected abstract void Dispose(bool disposing);

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

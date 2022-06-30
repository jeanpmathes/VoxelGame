// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The world class. Contains everything that is in the world, e.g. chunks, entities, etc.
/// </summary>
public abstract partial class World : IDisposable
{
    /// <summary>
    ///     The limit of the world size.
    ///     The actual size of the world can be smaller, but never larger.
    /// </summary>
    public const uint BlockLimit = 50_000_000;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    private readonly IWorldGenerator generator;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    protected World(string path, string name, int seed) :
        this(
            new WorldInformation
            {
                Name = name,
                Seed = seed,
                Creation = DateTime.Now,
                Version = ApplicationInformation.Instance.Version
            },
            path)
    {
        Information.Save(Path.Combine(WorldDirectory, "meta.json"));

        logger.LogInformation(Events.WorldIO, "Created new world");
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    protected World(string path, WorldInformation information) :
        this(
            information,
            path)
    {
        logger.LogInformation(Events.WorldIO, "Loaded existing world");
    }

    /// <summary>
    ///     Setup of readonly fields and non-optional steps.
    /// </summary>
    private World(WorldInformation information, string worldDirectory)
    {
        positionsToActivate = new HashSet<ChunkPosition>();
        positionsActivating = new HashSet<ChunkPosition>();
        chunksToGenerate = new UniqueQueue<Chunk>();
        chunkGenerateTasks = new List<Task>(MaxGenerationTasks);
        chunksGenerating = new Dictionary<int, Chunk>(MaxGenerationTasks);
        positionsToLoad = new UniqueQueue<ChunkPosition>();
        chunkLoadingTasks = new List<Task<Chunk?>>(MaxLoadingTasks);
        positionsLoading = new Dictionary<int, ChunkPosition>(MaxLoadingTasks);
        activeChunks = new Dictionary<ChunkPosition, Chunk>();
        positionsToReleaseOnActivation = new HashSet<ChunkPosition>();
        chunksToSave = new UniqueQueue<Chunk>();
        chunkSavingTasks = new List<Task>(MaxSavingTasks);
        chunksSaving = new Dictionary<int, Chunk>(MaxSavingTasks);
        positionsSaving = new HashSet<ChunkPosition>(MaxSavingTasks);
        positionsActivatingThroughSaving = new HashSet<ChunkPosition>();

        Information = information;
        ValidateInformation();

        WorldDirectory = worldDirectory;
        ChunkDirectory = Path.Combine(worldDirectory, "Chunks");
        BlobDirectory = Path.Combine(worldDirectory, "Blobs");
        DebugDirectory = Path.Combine(worldDirectory, "Debug");

        UpdateCounter = new UpdateCounter();

        Setup();

        generator = GetGenerator(this);
    }

    private WorldInformation Information { get; }

    /// <summary>
    ///     The update counter counting the world updates.
    /// </summary>
    public UpdateCounter UpdateCounter { get; }

    private int MaxGenerationTasks { get; } = Settings.Default.MaxGenerationTasks;
    private int MaxLoadingTasks { get; } = Settings.Default.MaxLoadingTasks;

    private int MaxSavingTasks { get; } = Settings.Default.MaxSavingTasks;

    /// <summary>
    ///     The directory in which this world is stored.
    /// </summary>
    private string WorldDirectory { get; }

    /// <summary>
    ///     The directory in which all chunks of this world are stored.
    /// </summary>
    private string ChunkDirectory { get; }

    /// <summary>
    ///     The directory in named data blobs are stored.
    /// </summary>
    private string BlobDirectory { get; }

    /// <summary>
    ///     The directory at which debug artifacts can be stored.
    /// </summary>
    public string DebugDirectory { get; }

    /// <summary>
    ///     Gets whether this world is ready for physics ticking and rendering.
    /// </summary>
    protected bool IsReady { get; set; }

    /// <summary>
    ///     Get the world creation seed.
    /// </summary>
    public int Seed => Information.Seed;

    /// <summary>
    ///     Get or set the spawn position in this world.
    /// </summary>
    public Vector3d SpawnPosition
    {
        get => Information.SpawnInformation.Position;
        set
        {
            Information.SpawnInformation = new SpawnInformation(value);
            logger.LogInformation(Events.WorldData, "World spawn position has been set to: {Position}", value);
        }
    }

    /// <summary>
    ///     Get or set the world size in blocks.
    /// </summary>
    public uint BlockSize
    {
        get => Information.Size;
        set
        {
            uint oldSize = Information.Size;
            Information.Size = ClampSize(value);

            if (oldSize != Information.Size) logger.LogInformation(Events.WorldData, "World size has been set to: {Size}", Information.Size);
        }
    }

    /// <summary>
    ///     Get the extents of the world.
    /// </summary>
    public Vector3d Extents => new(BlockSize, BlockSize, BlockSize);

    /// <summary>
    ///     Get a reader for an existing blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <returns>The reader for the blob, or null if the blob does not exist.</returns>
    public BinaryReader? GetBlobReader(string name)
    {
        try
        {
            Stream stream = File.Open(Path.Combine(BlobDirectory, name), FileMode.Open, FileAccess.Read);

            return new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        }
        catch (IOException)
        {
            logger.LogDebug(Events.WorldIO, "Failed to read blob '{Name}'", name);

            return null;
        }
    }

    /// <summary>
    ///     Get a stream to a new blob.
    /// </summary>
    /// <param name="name">The name of the blob.</param>
    /// <returns>The stream to the blob, or null if an error occurred.</returns>
    public BinaryWriter? GetBlobWriter(string name)
    {
        try
        {
            Stream stream = File.Open(Path.Combine(BlobDirectory, name), FileMode.Create, FileAccess.Write);

            return new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        }
        catch (IOException e)
        {
            logger.LogError(Events.WorldIO, e, "Failed to create blob '{Name}'", name);

            return null;
        }
    }

    private void ValidateInformation()
    {
        uint validWorldSize = ClampSize(Information.Size);

        if (validWorldSize == Information.Size) return;

        Information.Size = validWorldSize;
        logger.LogWarning(Events.WorldData, "Loaded world size was invalid, changed to {Value}", validWorldSize);
    }

    private static uint ClampSize(uint size)
    {
        return Math.Clamp(size, min: 1024, BlockLimit);
    }

    private static IWorldGenerator GetGenerator(World world)
    {
        return new Generator(world);
    }

    private void Setup()
    {
        Directory.CreateDirectory(WorldDirectory);
        Directory.CreateDirectory(ChunkDirectory);
        Directory.CreateDirectory(BlobDirectory);
        Directory.CreateDirectory(DebugDirectory);

        positionsToActivate.Add(ChunkPosition.Origin);
    }

    private static bool IsInLimits(Vector3i position)
    {
        if (position.X is int.MinValue) return false;
        if (position.Y is int.MinValue) return false;
        if (position.Z is int.MinValue) return false;

        return Math.Abs(position.X) <= BlockLimit && Math.Abs(position.Y) <= BlockLimit && Math.Abs(position.Z) <= BlockLimit;
    }

    /// <summary>
    ///     Called every update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    public abstract void Update(double deltaTime);

    /// <summary>
    ///     Returns the block instance at a given position in block coordinates. The block is only searched in active chunks.
    /// </summary>
    /// <param name="position">The block position.</param>
    /// <returns>The block instance at the given position or null if the block was not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance? GetBlock(Vector3i position)
    {
        RetrieveContent(position, out Block? block, out uint data, out _, out _, out _);

        return block?.AsInstance(data);
    }

    /// <summary>
    ///     Retrieve the content at a given position. The content can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="block">The block at the given position.</param>
    /// <param name="data">The data of the block.</param>
    /// <param name="fluid">The fluid at the given position.</param>
    /// <param name="level">The level of the fluid.</param>
    /// <param name="isStatic">Whether the fluid is static.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RetrieveContent(Vector3i position,
        out Block? block, out uint data,
        out Fluid? fluid, out FluidLevel level, out bool isStatic)
    {
        Chunk? chunk = GetChunk(position);

        if (chunk != null)
        {
            uint val = chunk.GetSection(position).GetContent(position);
            Section.Decode(val, out block, out data, out fluid, out level, out isStatic);

            return;
        }

        block = null;
        data = 0;
        fluid = null;
        level = 0;
        isStatic = false;
    }

    /// <summary>
    ///     Get the fluid at a given position. The fluid can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The position in the world.</param>
    /// <returns>The fluid instance, if there is any.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluidInstance? GetFluid(Vector3i position)
    {
        RetrieveContent(
            position,
            out Block? _,
            out uint _,
            out Fluid? fluid,
            out FluidLevel level,
            out bool isStatic);

        return fluid?.AsInstance(level, isStatic);
    }

    /// <summary>
    ///     Get both the fluid and block instance at a given position.
    ///     The content can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>The content, if there is any.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (BlockInstance block, FluidInstance fluid)? GetContent(Vector3i position)
    {
        RetrieveContent(
            position,
            out Block? block,
            out uint data,
            out Fluid? fluid,
            out FluidLevel level,
            out bool isStatic);

        if (block == null || fluid == null) return null;

        Debug.Assert(block != null);
        Debug.Assert(fluid != null);

        return (block.AsInstance(data), fluid.AsInstance(level, isStatic));
    }

    /// <summary>
    ///     Sets a block in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of
    ///     the changed block.
    /// </summary>
    /// <param name="block">The block which should be set at the position.</param>
    /// <param name="position">The block position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(BlockInstance block, Vector3i position)
    {
        FluidInstance? potentialFluid = GetFluid(position);

        if (potentialFluid is not {} fluid) return;

        SetContent(block, fluid, position, tickFluid: true);
    }

    /// <summary>
    ///     Sets a fluid in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of the
    ///     changed block.
    /// </summary>
    /// <param name="fluid"></param>
    /// <param name="position"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFluid(FluidInstance fluid, Vector3i position)
    {
        BlockInstance? potentialBlock = GetBlock(position);

        if (potentialBlock is not {} block) return;

        SetContent(block, fluid, position, tickFluid: false);
    }

    /// <summary>
    ///     Set the <c>isStatic</c> flag of a fluid without causing any updates around this fluid.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ModifyFluid(bool isStatic, Vector3i position)
    {
        ModifyWorldData(position, ~Section.StaticMask, isStatic ? Section.StaticMask : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetContent(BlockInstance block, FluidInstance fluid, Vector3i position, bool tickFluid)
    {
        SetContent(block.Block, block.Data, fluid.Fluid, fluid.Level, fluid.IsStatic, position, tickFluid);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetContent(IBlockBase block, uint data, Fluid fluid, FluidLevel level, bool isStatic,
        Vector3i position, bool tickFluid)
    {
        Chunk? chunk = GetChunk(position);

        if (chunk == null) return;

        uint val = Section.Encode(block, data, fluid, level, isStatic);

        chunk.GetSection(position).SetContent(position, val);

        if (tickFluid) fluid.TickNow(this, position, level, isStatic);

        // Block updates - Side is passed out of the perspective of the block receiving the block update.

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i neighborPosition = side.Offset(position);

            (BlockInstance, FluidInstance)? content = GetContent(neighborPosition);

            if (content == null) continue;

            (BlockInstance blockNeighbor, FluidInstance fluidNeighbor) = content.Value;

            blockNeighbor.Block.BlockUpdate(this, neighborPosition, blockNeighbor.Data, side.Opposite());
            fluidNeighbor.Fluid.TickSoon(this, neighborPosition, fluidNeighbor.IsStatic);
        }

        ProcessChangedSection(chunk, position);
    }

    /// <summary>
    ///     Set all data at a world position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPosition(IBlockBase block, uint data, Fluid fluid, FluidLevel level, bool isStatic,
        Vector3i position)
    {
        SetContent(block, data, fluid, level, isStatic, position, tickFluid: true);
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
    private void ModifyWorldData(Vector3i position, uint clearMask, uint addMask)
    {
        Chunk? chunk = GetChunk(position);

        if (chunk == null) return;

        uint val = chunk.GetSection(position).GetContent(position);

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
        SetBlock(BlockInstance.Default, position);
    }

    /// <summary>
    ///     Set a position to the default fluid.
    /// </summary>
    public void SetDefaultFluid(Vector3i position)
    {
        SetFluid(FluidInstance.Default, position);
    }

    /// <summary>
    ///     Force a random update at a position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>True if both the fluid and block at the position received a random update.</returns>
    public bool DoRandomUpdate(Vector3i position)
    {
        (BlockInstance, FluidInstance)? content = GetContent(position);

        if (content == null) return false;

        (BlockInstance block, FluidInstance fluid) = content.Value;

        block.Block.RandomUpdate(this, position, block.Data);
        fluid.Fluid.RandomUpdate(this, position, fluid.Level, fluid.IsStatic);

        return true;
    }

    /// <summary>
    ///     Saves all active chunks that are not currently saved.
    /// </summary>
    /// <returns>A task that represents all tasks saving the chunks.</returns>
    public Task SaveAsync()
    {
        logger.LogInformation(Events.WorldIO, "Saving world");

        List<Task> savingTasks = new(activeChunks.Count);

        foreach (Chunk chunk in activeChunks.Values)
            if (!positionsSaving.Contains(chunk.Position))
                savingTasks.Add(chunk.SaveAsync(ChunkDirectory));

        Information.Version = ApplicationInformation.Instance.Version;

        savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(WorldDirectory, "meta.json"))));

        return Task.WhenAll(savingTasks);
    }

    /// <summary>
    ///     Wait for all world tasks to finish.
    /// </summary>
    /// <returns>A task that is finished when all world tasks are finished.</returns>
    public Task FinishAllAsync()
    {
        // This method is just a quick hack to fix a possible cause of crashes.
        // It would be better to also process the finished tasks.

        List<Task> tasks = new();
        AddAllTasks(tasks);

        return Task.WhenAll(tasks);
    }

    /// <summary>
    ///     Add all tasks to the list. This is used to wait for all tasks to finish when calling <see cref="FinishAllAsync" />.
    /// </summary>
    /// <param name="tasks">The task list.</param>
    protected virtual void AddAllTasks(IList<Task> tasks)
    {
        chunkGenerateTasks.ForEach(tasks.Add);
        chunkLoadingTasks.ForEach(tasks.Add);
        chunkSavingTasks.ForEach(tasks.Add);
    }

    #region IDisposable Support

    private bool disposed;

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                foreach (Chunk activeChunk in activeChunks.Values) activeChunk.Dispose();

                foreach (Chunk generatingChunk in chunksGenerating.Values) generatingChunk.Dispose();

                foreach (Chunk savingChunk in chunksSaving.Values) savingChunk.Dispose();
            }

            disposed = true;
        }
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

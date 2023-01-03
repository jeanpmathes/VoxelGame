// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The world class. Contains everything that is in the world, e.g. chunks, entities, etc.
/// </summary>
public abstract class World : IDisposable, IGrid
{
    /// <summary>
    ///     The limit of the world size.
    ///     The actual size of the world can be smaller, but never larger.
    /// </summary>
    public const uint BlockLimit = 50_000_000;

    private const uint ChunkLimit = BlockLimit / Chunk.BlockSize;

    /// <summary>
    ///     The limit of the world size, in sections.
    /// </summary>
    public const uint SectionLimit = BlockLimit / Section.Size;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    private readonly ChunkSet chunks;

    private readonly IWorldGenerator generator;

    private (Task saving, Action callback)? deactivation;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    protected World(string path, string name, (int upper, int lower) seed) :
        this(
            new WorldInformation
            {
                Name = name,
                UpperSeed = seed.upper,
                LowerSeed = seed.lower,
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
        Information = information;
        ValidateInformation();

        WorldDirectory = worldDirectory;
        ChunkDirectory = Path.Combine(worldDirectory, "Chunks");
        BlobDirectory = Path.Combine(worldDirectory, "Blobs");
        DebugDirectory = Path.Combine(worldDirectory, "Debug");

        Directory.CreateDirectory(WorldDirectory);
        Directory.CreateDirectory(ChunkDirectory);
        Directory.CreateDirectory(BlobDirectory);
        Directory.CreateDirectory(DebugDirectory);

        generator = GetGenerator(this);

        ChunkContext = new ChunkContext(ChunkDirectory, CreateChunk, ProcessNewlyActivatedChunk, ProcessActivatedChunk, UnloadChunk, generator);

        MaxGenerationTasks = ChunkContext.DeclareBudget(Settings.Default.MaxGenerationTasks);
        MaxDecorationTasks = ChunkContext.DeclareBudget(Settings.Default.MaxDecorationTasks);
        MaxLoadingTasks = ChunkContext.DeclareBudget(Settings.Default.MaxLoadingTasks);
        MaxSavingTasks = ChunkContext.DeclareBudget(Settings.Default.MaxSavingTasks);

        chunks = new ChunkSet(ChunkContext);

        RequestChunk(ChunkPosition.Origin);
    }

    /// <summary>
    ///     Setup the chunk context.
    /// </summary>
    protected ChunkContext ChunkContext { get; }

    private WorldInformation Information { get; }

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
    private string DebugDirectory { get; }

    /// <summary>
    ///     Get the world creation seed.
    /// </summary>
    public (int upper, int lower) Seed => (Information.UpperSeed, Information.LowerSeed);

    /// <summary>
    /// Get whether the world is active.
    /// </summary>
    protected bool IsActive => CurrentState == State.Active;

    /// <summary>
    ///     Get the world state.
    /// </summary>
    protected State CurrentState { get; set; } = State.Activating;

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
    public uint SizeInBlocks
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
    ///     Get the extents of the world. This mark the reachable area of the world.
    /// </summary>
    public Vector3i Extents => new((int) SizeInBlocks, (int) SizeInBlocks, (int) SizeInBlocks);

    /// <summary>
    ///     Get the info map of this world.
    /// </summary>
    public IMap Map => generator.Map;

    /// <summary>
    ///     Get the active chunk count.
    /// </summary>
    protected int ActiveChunkCount => chunks.ActiveCount;

    /// <summary>
    ///     All active chunks.
    /// </summary>
    protected IEnumerable<Chunk> ActiveChunks => chunks.AllActive;

    /// <summary>
    ///     The max generation task limit.
    /// </summary>
    public Limit MaxGenerationTasks { get; }

    /// <summary>
    ///     The max decoration task limit.
    /// </summary>
    public Limit MaxDecorationTasks { get; }

    /// <summary>
    ///     The max loading task limit.
    /// </summary>
    public Limit MaxLoadingTasks { get; }

    /// <summary>
    ///     The max saving task limit.
    /// </summary>
    public Limit MaxSavingTasks { get; }

    /// <summary>
    ///     Get both the fluid and block instance at a given position.
    ///     The content can only be retrieved from active chunks.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>The content, if there is any.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Content? GetContent(Vector3i position)
    {
        RetrieveContent(position, out Content? content);

        return content;
    }

    /// <summary>
    ///     Set the content of a world position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetContent(Content content, Vector3i position)
    {
        SetContent(content, position, tickFluid: true);
    }

    /// <summary>
    ///     Begin deactivating the world, saving all chunks and the meta information.
    /// </summary>
    /// <param name="onFinished">The action to be called when the world is deactivated.</param>
    public void BeginDeactivating(Action onFinished)
    {
        Debug.Assert(CurrentState == State.Active);
        CurrentState = State.Deactivating;

        logger.LogInformation(Events.WorldIO, "Unloading world");

        chunks.BeginSaving();

        Information.Version = ApplicationInformation.Instance.Version;
        Task saving = Task.Run(() => Information.Save(Path.Combine(WorldDirectory, "meta.json")));

        deactivation = (saving, onFinished);
    }

    /// <summary>
    ///     Process the deactivation, assuming it has been started.
    /// </summary>
    /// <returns>Whether the deactivation is finished.</returns>
    protected bool ProcessDeactivation()
    {
        Debug.Assert(deactivation != null);

        (Task saving, Action callback) = deactivation.Value;

        bool done = saving.IsCompleted && chunks.IsEmpty;

        if (!done) return false;

        logger.LogInformation(Events.WorldIO, "Unloaded world");
        callback();

        if (saving.IsFaulted) logger.LogError(Events.WorldSavingError, saving.Exception, "Failed to save world meta information");

        return true;
    }

    private void UnloadChunk(Chunk chunk)
    {
        chunks.Unload(chunk);
    }

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
        return Math.Clamp(size, 16 * Chunk.BlockSize, BlockLimit - Chunk.BlockSize);
    }

    private static IWorldGenerator GetGenerator(World world)
    {
        return new Generator(world);
    }

    /// <summary>
    ///     Emit views of global world data for debugging.
    /// </summary>
    public void EmitViews()
    {
        generator.EmitViews(DebugDirectory);
    }

    /// <summary>
    ///     Search for named generated elements, such as structures.
    ///     The search is performed on enumeration.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="name">The name of the element.</param>
    /// <param name="maxDistance">The maximum distance to search.</param>
    /// <returns>The positions of the elements, or null if the name is not valid.</returns>
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, string name, uint maxDistance)
    {
        return generator.SearchNamedGeneratedElements(start, name, maxDistance);
    }

    /// <summary>
    ///     Called every update cycle.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    public abstract void Update(double deltaTime);

    /// <summary>
    ///     Update chunks.
    /// </summary>
    protected void UpdateChunks()
    {
        chunks.Update();
    }

    /// <summary>
    ///     Returns the block instance at a given position in block coordinates. The block is only searched in active chunks.
    /// </summary>
    /// <param name="position">The block position.</param>
    /// <returns>The block instance at the given position or null if the block was not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance? GetBlock(Vector3i position)
    {
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
            uint val = chunk.GetSection(position).GetContent(position);
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
        RetrieveContent(position, out Content? content);

        return content?.Fluid;
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

        SetContent(new Content(block, fluid), position, tickFluid: true);
    }

    /// <summary>
    ///     Sets a fluid in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of the
    ///     changed block.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFluid(FluidInstance fluid, Vector3i position)
    {
        BlockInstance? potentialBlock = GetBlock(position);

        if (potentialBlock is not {} block) return;

        SetContent(new Content(block, fluid), position, tickFluid: false);
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
    private void SetContent(in Content content, Vector3i position, bool tickFluid)
    {
        Chunk? chunk = GetActiveChunk(position);

        if (chunk == null) return;

        uint val = Section.Encode(content);

        chunk.GetSection(position).SetContent(position, val);

        if (tickFluid) content.Fluid.Fluid.TickNow(this, position, content.Fluid.Level, content.Fluid.IsStatic);

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            Vector3i neighborPosition = side.Offset(position);

            Content? neighborContent = GetContent(neighborPosition);

            if (neighborContent == null) continue;

            (BlockInstance blockNeighbor, FluidInstance fluidNeighbor) = neighborContent.Value;

            // Side is passed out of the perspective of the block receiving the block update.
            blockNeighbor.Block.BlockUpdate(this, neighborPosition, blockNeighbor.Data, side.Opposite());
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
    private void ModifyWorldData(Vector3i position, uint clearMask, uint addMask)
    {
        Chunk? chunk = GetActiveChunk(position);

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
        Content? content = GetContent(position);

        if (content == null) return false;

        (BlockInstance block, FluidInstance fluid) = content.Value;

        block.Block.RandomUpdate(this, position, block.Data);
        fluid.Fluid.RandomUpdate(this, position, fluid.Level, fluid.IsStatic);

        return true;
    }

    /// <summary>
    ///     Creates a chunk for a chunk position.
    /// </summary>
    protected abstract Chunk CreateChunk(ChunkPosition position, ChunkContext context);

    /// <summary>
    ///     Get whether a chunk position is in the maximum allowed world limits.
    ///     Such a position can still be outside of the reachable <see cref="Extents" />.
    /// </summary>
    public static bool IsInLimits(ChunkPosition position)
    {
        return Math.Abs(position.X) <= ChunkLimit && Math.Abs(position.Y) <= ChunkLimit && Math.Abs(position.Z) <= ChunkLimit;
    }

    /// <summary>
    ///     Get whether a section position is in the maximum allowed world limits.
    ///     Such a position can still be outside of the reachable <see cref="Extents" />.
    /// </summary>
    public static bool IsInLimits(SectionPosition position)
    {
        return Math.Abs(position.X) <= SectionLimit && Math.Abs(position.Y) <= SectionLimit && Math.Abs(position.Z) <= SectionLimit;
    }

    /// <summary>
    ///     Get whether a block position is in the maximum allowed world limits.
    ///     Such a position can still be outside of the reachable <see cref="Extents" />.
    /// </summary>
    public static bool IsInLimits(Vector3i position)
    {
        if (position.X is int.MinValue) return false;
        if (position.Y is int.MinValue) return false;
        if (position.Z is int.MinValue) return false;

        return Math.Abs(position.X) <= BlockLimit && Math.Abs(position.Y) <= BlockLimit && Math.Abs(position.Z) <= BlockLimit;
    }

    /// <summary>
    ///     Process a chunk that has been just activated.
    /// </summary>
    /// <returns>The next state of the chunk.</returns>
    protected abstract ChunkState ProcessNewlyActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Process a chunk that has just switched to the active state trough a weak activation.
    /// </summary>
    /// <returns>An optional next state of the chunk.</returns>
    protected abstract ChunkState? ProcessActivatedChunk(Chunk activatedChunk);

    /// <summary>
    ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void RequestChunk(ChunkPosition position)
    {
        Debug.Assert(CurrentState != State.Deactivating);

        if (!IsInLimits(position)) return;

        chunks.Request(position);

        logger.LogDebug(Events.ChunkRequest, "Chunk {Position} has been requested", position);
    }

    /// <summary>
    ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    public void ReleaseChunk(ChunkPosition position)
    {
        Debug.Assert(CurrentState != State.Deactivating);

        if (!IsInLimits(position)) return;

        // Check if the chunk can be released
        if (position == ChunkPosition.Origin) return; // The chunk at (0|0|0) cannot be released.

        chunks.Release(position);

        logger.LogDebug(Events.ChunkRelease, "Released chunk {Position}", position);
    }

    /// <summary>
    ///     Gets an active chunk.
    ///     See <see cref="ChunkSet.GetActive" /> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
    public Chunk? GetActiveChunk(ChunkPosition position)
    {
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
        return IsInLimits(position) ? GetActiveChunk(ChunkPosition.From(position)) : null;
    }

    /// <summary>
    ///     Check if a chunk is active.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>True if the chunk is active.</returns>
    protected bool IsChunkActive(ChunkPosition position)
    {
        return GetActiveChunk(position) != null;
    }

    /// <summary>
    ///     Try to get a chunk. The chunk is possibly not active.
    ///     See <see cref="ChunkSet.GetAny" /> for the restrictions.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="chunk">The chunk at the given position or null if no chunk was found.</param>
    /// <returns>True if a chunk was found.</returns>
    public bool TryGetChunk(ChunkPosition position, [NotNullWhen(returnValue: true)] out Chunk? chunk)
    {
        chunk = chunks.GetAny(position);

        return chunk != null;
    }

    /// <summary>
    /// The world state.
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

    #region IDisposable Support

    private bool disposed;

    /// <summary>
    ///     Dispose of the world.
    /// </summary>
    /// <param name="disposing">True when disposing intentionally.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing) chunks.Dispose();

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

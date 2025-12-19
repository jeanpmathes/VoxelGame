// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Memory;
using Chunk = VoxelGame.Client.Logic.Chunks.Chunk;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class World : Core.Logic.World
{
    private static readonly Vector3d sunLightDirection = Vector3d.Normalize(new Vector3d(x: -2, y: -3, z: -1));

    private LocalPlayerHook? localPlayer;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    internal World(Space space, DirectoryInfo path, String name, (Int32 upper, Int32 lower) seed) : base(path, name, seed)
    {
        Space = space;

        SetUp();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    internal World(Space space, WorldData data) : base(data)
    {
        Space = space;

        SetUp();
    }

    /// <summary>
    ///     Get the space in which all objects of this world are placed in.
    /// </summary>
    public Space Space { get; }

    private void SetUp()
    {
        Space.Light.Direction = sunLightDirection;

        AddComponent<SectionMeshing>();
        AddComponent<HideWorldOnTermination>();
    }

    /// <inheritdoc />
    protected override Core.Logic.Chunks.Chunk CreateChunk(NativeSegment<UInt32> blocks, ChunkContext context)
    {
        return new Chunk(blocks, context);
    }

    /// <summary>
    ///     Render this world and everything in it.
    /// </summary>
    public void RenderUpdate()
    {
        if (!State.IsActive) return;

        localPlayer ??= GetComponent<LocalPlayerHook>();

        if (localPlayer == null)
            return;

        Frustum frustum = localPlayer.Player.Camera.View.Definition.Frustum;

        if (Core.App.Application.Instance.IsDebug)
            Chunks.ForEachActive(chunk => chunk.Cast().CullSections(frustum));
        else
            // Rendering chunks even if they are used by an off-thread operation is safe.
            // The rendering resources are only modified on the main thread anyway.
            Chunks.ForEachComplete(chunk => chunk.Cast().CullSections(frustum));
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessNewlyActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        return activatedChunk.ProcessDecorationOption() ?? ProcessActivatedChunk(activatedChunk);
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        ChunkState? state = activatedChunk.Cast().ProcessMeshingOption(out Boolean allowActivation);

        if (state != null) return state;

        return allowActivation
            ? new Core.Logic.Chunks.Chunk.Active()
            : null;
    }
}

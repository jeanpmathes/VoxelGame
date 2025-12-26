// <copyright file="World.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
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
        AddComponent<SectionMeshing>();
        AddComponent<HideWorldOnTermination>();

        AddComponent<TimeBasedLighting>();
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

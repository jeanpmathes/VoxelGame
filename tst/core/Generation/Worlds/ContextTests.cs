// <copyright file="ContextTests.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Toolkit.Memory;

namespace VoxelGame.Core.Tests.Generation.Worlds;

public class ContextTestBase : IDisposable
{
    private readonly NativeAllocator allocator;

    protected ContextTestBase()
    {
        allocator = new NativeAllocator();
    }

    protected Chunk CreateInitializedChunk(ChunkContext context, ChunkPosition position)
    {
        Chunk chunk = new(context, allocator.Allocate<UInt32>(Chunk.BlockCount).Segment, blocks => new Section(blocks));

        chunk.Initialize(null!, position);

        return chunk;
    }

    protected static Chunk CreateChunk(NativeSegment<UInt32> segment, ChunkContext ctx)
    {
        return new Chunk(ctx, segment, blocks => new Section(blocks));
    }

    #region DISPOSABLE

    protected virtual void Dispose(Boolean disposing)
    {
        if (disposing) allocator.Dispose();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion DISPOSABLE
}

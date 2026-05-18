// <copyright file="BrushMap.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Maps GUI brushes to native user-interface brushes.
/// </summary>
internal sealed class BrushMap : IDisposable
{
    public BrushMap(Renderer renderer)
    {
        // todo: Store the native UI renderer and subscribe to scale invalidation if needed.
        // Contract: maps are owned by the presentation renderer and dispose all cached native wrappers.
        _ = renderer;
    }

    public void Dispose()
    {
        // todo: Dispose all cached native brush wrappers.
    }

    public Brush Get(Brush brush)
    {
        // todo: Resolve a GUI brush to a native user-interface brush.
        _ = brush;
        throw new NotImplementedException();
    }
}

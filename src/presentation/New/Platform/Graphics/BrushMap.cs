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
using System.Collections.Generic;
using System.Drawing;
using VoxelGame.GUI.Graphics;
using VoxelGame.Toolkit.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Maps GUI brushes to native brush wrappers.
/// </summary>
internal sealed class BrushMap(Renderer renderer) : IDisposable
{
    private readonly Dictionary<Brush, VoxelGame.Graphics.Objects.UserInterface.Brush> map = [];

    public VoxelGame.Graphics.Objects.UserInterface.Brush? Get(Brush brush, out Color? color)
    {
        color = null;

        switch (brush)
        {
            case TransparentBrush:
                return null;

            case SolidColorBrush solidColorBrush:
                color = solidColorBrush.Color;
                return null;

            default:
                return GetOrCreateBrush(brush);
        }
    }

    private VoxelGame.Graphics.Objects.UserInterface.Brush GetOrCreateBrush(Brush brush)
    {
        if (map.TryGetValue(brush, out VoxelGame.Graphics.Objects.UserInterface.Brush? uiBrush))
            return uiBrush;

        uiBrush = brush switch
        {
            SolidColorBrush solidColorBrush => renderer.CreateSolidColorBrush(solidColorBrush.Color),
            _ => throw Exceptions.UnsupportedValue(brush)
        };

        map.Add(brush, uiBrush);

        return uiBrush;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~BrushMap()
    {
        Dispose(disposing: false);
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            foreach (VoxelGame.Graphics.Objects.UserInterface.Brush brush in map.Values)
            {
                brush.Dispose();
            }

            map.Clear();
        }
        else ExceptionTools.ThrowForMissedDispose<BrushMap>();

        disposed = true;
    }

    #endregion DISPOSABLE
}

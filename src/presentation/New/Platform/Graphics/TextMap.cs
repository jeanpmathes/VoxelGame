// <copyright file="TextMap.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Texts;
using Font = VoxelGame.GUI.Texts.Font;

namespace VoxelGame.Presentation.New.Platform.Graphics;

/// <summary>
///     Maps GUI text requests to native user-interface text layouts.
/// </summary>
internal sealed class TextMap : IDisposable
{
    public TextMap(Renderer renderer, TextFormatMap formats)
    {
        // todo: Store the native UI renderer and text-format map, then subscribe to renderer scale changes.
        // Contract: scale changes invalidate cached text measurements.
        _ = renderer;
        _ = formats;
    }

    public void Dispose()
    {
        // todo: Dispose all cached native text wrappers.
    }

    public Text Get(String text, Font font, TextOptions options)
    {
        // todo: Resolve text, font, and options to a native user-interface text object through TextFormatMap.
        // Contract: there is no mutating API for native text or text formats.
        _ = text;
        _ = font;
        _ = options;
        throw new NotImplementedException();
    }
}

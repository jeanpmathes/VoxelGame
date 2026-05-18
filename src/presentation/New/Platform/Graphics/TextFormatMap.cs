// <copyright file="TextFormatMap.cs" company="VoxelGame">
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
///     Maps GUI text-format descriptions to native user-interface text formats.
/// </summary>
internal sealed class TextFormatMap : IDisposable
{
    public TextFormatMap(Renderer renderer)
    {
        // todo: Store the native UI renderer and subscribe to renderer scale changes.
        // Contract: there is no mutating API for native text formats.
        _ = renderer;
    }

    public void Dispose()
    {
        // todo: Dispose all cached native text-format wrappers.
    }

    public TextFormat Get(Font font, TextOptions options)
    {
        // todo: Resolve font and text options to a native user-interface text format.
        _ = font;
        _ = options;
        throw new NotImplementedException();
    }
}

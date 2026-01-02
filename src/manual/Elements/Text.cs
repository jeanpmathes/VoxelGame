// <copyright file="Text.cs" company="VoxelGame">
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
using System.IO;
using VoxelGame.Manual.Modifiers;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A simple text element.
/// </summary>
internal sealed class Text : IElement
{
    private readonly TextStyle style;

    internal Text(String text, TextStyle style)
    {
        Content = text;
        this.style = style;
    }

    private String Content { get; }

    void IElement.Generate(StreamWriter writer)
    {
        switch (style)
        {
            case TextStyle.Monospace:
                writer.WriteLine($@"\texttt{{{Content}}}");

                break;

            case TextStyle.Normal:
                writer.WriteLine(Content);

                break;

            default:
                throw Exceptions.UnsupportedEnumValue(style);
        }
    }
}

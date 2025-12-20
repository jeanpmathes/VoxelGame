// <copyright file="Boolean.cs" company="VoxelGame">
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

using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A piece of text that represents a boolean value.
/// </summary>
internal sealed class Boolean : IElement
{
    internal Boolean(System.Boolean value)
    {
        Value = value;
    }

    private System.Boolean Value { get; }

    void IElement.Generate(StreamWriter writer)
    {
        writer.Write(Value ? @" \Checkmark " : @" \XSolidBrush ");
    }
}

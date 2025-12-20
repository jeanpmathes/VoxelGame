// <copyright file="SheetLoader.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Loads sheets, splitting them into individual textures and pre-processing them.
/// </summary>
public class SheetLoader
{
    /// <summary>
    ///     The resolution of the single textures in the sheet.
    /// </summary>
    public required Int32 Resolution { get; init; }

    /// <summary>
    ///     Loads a sheet from the given file.
    /// </summary>
    /// <param name="file">The file to load the sheet from.</param>
    /// <param name="error">The error and message, if any.</param>
    /// <returns>The loaded sheet, or <c>null</c> if an error occurred.</returns>
    public Sheet? Load(FileInfo file, out (Exception? exception, String message)? error)
    {
        Image image = Image.LoadFromFile(file);

        if (image.Width % Resolution != 0 || image.Height % Resolution != 0)
        {
            error = (null, $"Image size does not match the required resolution of {Resolution}x{Resolution}");

            return null;
        }

        Int32 xCount = image.Width / Resolution;
        Int32 yCount = image.Height / Resolution;

        if (xCount > Byte.MaxValue || yCount > Byte.MaxValue)
        {
            error = (null, $"Image contains more than {Byte.MaxValue}x{Byte.MaxValue} textures");

            return null;
        }

        Sheet sheet = new((Byte) xCount, (Byte) yCount);

        for (Byte x = 0; x < xCount; x++)
        for (Byte y = 0; y < yCount; y++)
        {
            Vector2i min = (x * Resolution, y * Resolution);
            Vector2i max = (min.X + Resolution, min.Y + Resolution) - Vector2i.One;

            Image texture = image.CreateCopy(new Box2i(min, max));

            texture.RecolorTransparency();

            sheet[x, (Byte) (yCount - y - 1)] = texture;
        }

        error = null;

        return sheet;
    }
}

﻿// <copyright file="SheetLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

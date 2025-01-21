// <copyright file="Sheet.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Represents a sheet of images, arranged in a grid.
/// </summary>
public class Sheet
{
    private readonly Image[,] images;

    /// <summary>
    ///     Create a new sheet with the given dimensions.
    /// </summary>
    /// <param name="width">The width of the sheet.</param>
    /// <param name="height">The height of the sheet.</param>
    public Sheet(Byte width, Byte height)
    {
        Debug.Assert(width > 0);
        Debug.Assert(height > 0);

        images = new Image[width, height];
    }

    /// <summary>
    ///     Get the width of this sheet.
    /// </summary>
    public Byte Width => (Byte) images.GetLength(dimension: 0);

    /// <summary>
    ///     Get the height of this sheet.
    /// </summary>
    public Byte Height => (Byte) images.GetLength(dimension: 1);

    /// <summary>
    ///     Whether this sheet is a single image.
    /// </summary>
    public Boolean IsSingle => Width == 1 && Height == 1;

    /// <summary>
    ///     Get or set the image at the given position.
    /// </summary>
    /// <param name="x">The x position of the image.</param>
    /// <param name="y">The y position of the image.</param>
    public Image this[Byte x, Byte y]
    {
        get => images[x, y];
        set => images[x, y] = value;
    }

    /// <summary>
    ///     Whether a given size is valid for a sheet.
    ///     Sheets can only have a size of up to 255x255.
    /// </summary>
    public static Boolean IsSizeValid(Int32 width, Int32 height)
    {
        return width is > 0 and <= Byte.MaxValue && height is > 0 and <= Byte.MaxValue;
    }

    /// <summary>
    ///     Copy a sheet. This will create a deep copy of the sheet.
    /// </summary>
    public static Sheet Copy(Sheet sheet)
    {
        Sheet copy = new(sheet.Width, sheet.Height);

        for (Byte x = 0; x < sheet.Width; x++)
        for (Byte y = 0; y < sheet.Height; y++)
            copy[x, y] = sheet[x, y].CreateCopy();

        return copy;
    }

    /// <summary>
    ///     Copy an image and wrap it in a sheet.
    /// </summary>
    public static Sheet Copy(Image image)
    {
        return new Sheet(width: 1, height: 1)
        {
            [x: 0, y: 0] = image.CreateCopy()
        };
    }

    /// <summary>
    ///     Place another sheet in this sheet at the given position.
    ///     The placed sheet must fit into this sheet at the given position.
    /// </summary>
    /// <param name="sheet">The sheet to place. The contained images will not be copied.</param>
    /// <param name="x">The x position to place the sheet.</param>
    /// <param name="y">The y position to place the sheet.</param>
    public void Place(Sheet sheet, Byte x, Byte y)
    {
        Debug.Assert(x + sheet.Width <= Width);
        Debug.Assert(y + sheet.Height <= Height);

        for (Byte dx = 0; dx < sheet.Width; dx++)
        for (Byte dy = 0; dy < sheet.Height; dy++)
            this[(Byte) (x + dx), (Byte) (y + dy)] = sheet[dx, dy];
    }
}

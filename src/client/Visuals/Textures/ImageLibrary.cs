// <copyright file="ImageLibrary.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     All available and soon-to-be-available images that can be used in the texture creation process.
/// </summary>
public class ImageLibrary
{
    private const String PartPrefix = "p:";
    private const String CombinedSuffix = ":all";

    private readonly Dictionary<String, Sheet> combinedParts = [];
    private readonly Dictionary<String, Sheet> combinedTextures = [];

    private readonly Dictionary<String, Image> splitParts = [];
    private readonly Dictionary<String, Image> splitTextures = [];

    /// <summary>
    ///     Add a sheet to the library.
    /// </summary>
    /// <param name="file">The source file of the image sheet.</param>
    /// <param name="sheet">The image sheet to add.</param>
    /// <param name="part">
    ///     Whether the images are parts to use or full textures on their own.
    ///     Parts will not be available when the texture loading process is done.
    /// </param>
    /// <returns><c>true</c> if the sheet was added, <c>false</c> if it already exists.</returns>
    public Boolean AddSheet(FileInfo file, Sheet sheet, Boolean part)
    {
        Dictionary<String, Image> splitTarget = part ? splitParts : splitTextures;
        Dictionary<String, Sheet> fullTarget = part ? combinedParts : combinedTextures;

        String name = GetName(file);

        if (!fullTarget.TryAdd(name, sheet))
            return false;

        for (Byte y = 0; y < sheet.Height; y++)
        for (Byte x = 0; x < sheet.Width; x++)
            splitTarget[TID.CreateKey(name, x, y)] = sheet[x, y];

        return true;
    }

    /// <summary>
    ///     Check if a sheet can be found in the library.
    /// </summary>
    /// <param name="key">The key of the sheet to check for. See <see cref="Bundler" /> for more information.</param>
    /// <returns><c>true</c> if the sheet is available, <c>false</c> otherwise.</returns>
    public Boolean HasSheet(String key)
    {
        return Access(key, out _, out _);
    }

    /// <summary>
    ///     Get a sheet from the library. Single images can also be accessed and will be wrapped in a sheet.
    ///     All returned sheets are deep copies and can be modified without affecting the library.
    /// </summary>
    /// <param name="key">The key of the sheet to get. See <see cref="Bundler" /> for more information.</param>
    /// <returns>The requested sheet, or <c>null</c> if it does not exist.</returns>
    public Sheet? GetSheet(String key)
    {
        if (!Access(key, out Sheet? sheet, out Image? image)) return null;

        if (sheet != null)
            return Sheet.Copy(sheet);

        if (image != null)
            return Sheet.Copy(image);

        return null;
    }

    private Boolean Access(String key, out Sheet? sheet, out Image? image)
    {
        ReadOnlySpan<Char> access = key.AsSpan();

        Boolean part = key.StartsWith(PartPrefix, StringComparison.InvariantCulture);
        if (part) access = access[PartPrefix.Length..];

        Boolean combined = key.EndsWith(CombinedSuffix, StringComparison.InvariantCulture);
        if (combined) access = access[..^CombinedSuffix.Length];

        sheet = null;
        image = null;

        if (combined)
        {
            Dictionary<String, Sheet>.AlternateLookup<ReadOnlySpan<Char>> lookup = part
                ? combinedParts.GetAlternateLookup<ReadOnlySpan<Char>>()
                : combinedTextures.GetAlternateLookup<ReadOnlySpan<Char>>();

            return lookup.TryGetValue(access, out sheet);
        }
        else
        {
            Dictionary<String, Image>.AlternateLookup<ReadOnlySpan<Char>> lookup = part
                ? splitParts.GetAlternateLookup<ReadOnlySpan<Char>>()
                : splitTextures.GetAlternateLookup<ReadOnlySpan<Char>>();

            return lookup.TryGetValue(access, out image);
        }
    }

    /// <summary>
    ///     Bundle all images in the library that should be available in the game as textures.
    ///     Will ensure that <see cref="ITextureIndexProvider.MissingTextureIndex" /> is available in the bundle.
    /// </summary>
    /// <param name="resolution">The resolution of the textures in this library.</param>
    /// <returns>The intermediate bundle containing all require images.</returns>
    public IntermediateBundle Bundle(Int32 resolution)
    {
        List<Image> textures = new(1 + splitTextures.Count)
        {
            Image.CreateFallback(resolution)
        };

        Dictionary<String, Int32> indices = [];

        foreach ((String key, Image image) in splitTextures)
        {
            if (image.IsEmpty())
                continue;

            indices[key] = textures.Count;
            textures.Add(image);
        }

        return new IntermediateBundle(textures, indices);
    }

    /// <summary>
    ///     Get the name of a texture from its file.
    /// </summary>
    /// <param name="file">The file defining the texture.</param>
    /// <returns>The name of the texture.</returns>
    public static String GetName(FileInfo file)
    {
        StringBuilder key = new();

        foreach (Char c in file.GetFileNameWithoutExtension())
            if (Char.IsLetterOrDigit(c) || c == '_')
                key.Append(c);

        return key.ToString();
    }
}

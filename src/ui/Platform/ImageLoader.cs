// <copyright file="ImageLoader.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Loads images from files.
/// </summary>
public static class ImageLoader
{
    private static readonly Dictionary<string, Loader> loaders = new()
    {
        {"JPEG", StandardLoader},
        {"JPE", StandardLoader},
        {"JFIF", StandardLoader},
        {"JPG", StandardLoader},
        {"BMP", StandardLoader},
        {"DIB", StandardLoader},
        {"RLE", StandardLoader},
        {"PNG", StandardLoader},
        {"GIF", StandardLoader},
        {"TIF", StandardLoader},
        {"EXIF", StandardLoader},
        {"WMF", StandardLoader},
        {"EMF", StandardLoader}
    };

    private static Bitmap StandardLoader(string s)
    {
        return new Bitmap(s);
    }

    /// <summary>
    ///     Loads an image from a file.
    /// </summary>
    /// <param name="filename">The file to load.</param>
    /// <returns>The loaded image.</returns>
    public static Bitmap Load(string filename)
    {
        string resourceType = filename.ToUpperInvariant().Split(separator: '.').Last();

        if (loaders.TryGetValue(resourceType, out Loader? loader)) return loader.Invoke(filename);

        throw new InvalidOperationException($"Unknown image type: {resourceType}");
    }

    private delegate Bitmap Loader(string filename);
}


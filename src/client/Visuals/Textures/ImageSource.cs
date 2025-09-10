// <copyright file="ImageSource.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Represents a source of images during the texture creation process.
///     See <see cref="Bundler" /> for more information.
/// </summary>
public class ImageSource : IIssueSource
{
    private readonly List<FileInfo> sheets = [];
    private readonly List<FileInfo> decks = [];
    private readonly List<FileInfo> parts = [];

    private ImageSource(DirectoryInfo directory)
    {
        InstanceName = directory.Name;
    }
    
    /// <inheritdoc />
    public String? InstanceName { get; }

    /// <summary>
    ///     Get all sheets in this source.
    /// </summary>
    public IEnumerable<FileInfo> Sheets => sheets;

    /// <summary>
    ///     Get all decks in this source.
    /// </summary>
    public IEnumerable<FileInfo> Decks => decks;

    /// <summary>
    ///     Get all parts in this source.
    /// </summary>
    public IEnumerable<FileInfo> Parts => parts;

    /// <summary>
    ///     Scan a texture directory, detecting all files in it that can be used for texture creation.
    /// </summary>
    /// <param name="directory">The directory to scan.</param>
    /// <param name="context">The resource context to use.</param>
    /// <returns>The image source, will be empty if an error occurred.</returns>
    public static ImageSource Scan(DirectoryInfo directory, IResourceContext context)
    {
        ImageSource source = new(directory);

        try
        {
            source.sheets.AddRange(directory.GetFiles("*.png"));
            source.decks.AddRange(directory.GetFiles("*.xml"));

            DirectoryInfo parts = directory.GetDirectory("Parts");

            try
            {
                source.parts.AddRange(parts.GetFiles("*.png"));
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore
            }
        }
        catch (DirectoryNotFoundException exception)
        {
            context.ReportWarning(source, "Texture directory not found", exception, directory);
        }

        return source;
    }
}

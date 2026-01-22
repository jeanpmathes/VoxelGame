// <copyright file="ImageSource.cs" company="VoxelGame">
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
    private readonly List<FileInfo> decks = [];
    private readonly List<FileInfo> parts = [];
    private readonly List<FileInfo> sheets = [];

    private ImageSource(DirectoryInfo directory)
    {
        InstanceName = directory.Name;
    }

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

    /// <inheritdoc />
    public String? InstanceName { get; }

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

// <copyright file="Bundler.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Bundles textures together, creating a texture bundle.
///     Textures are created from three source types, which all provide images:
///     <list type="bullet">
///         <item>Sheets, which are image files containing one or multiple textures in a 2D grid.</item>
///         <item>Decks, which combine multiple images and modifiers to create textures.</item>
///         <item>Parts, which are sheets which are only available during the texture creation process.</item>
///     </list>
///     In decks, other images can be referenced, using this syntax:
///     <list type="bullet">
///         <item>Sheets and Decks: <c>'file_name':'x','y'</c></item>
///         <item>Part: <c>p:'file_name':'x','y'</c></item>
///         <item>Complete Sheets and Decks: <c>'file_name':all</c></item>
///     </list>
/// </summary>
public static class Bundler
{
    /// <summary>
    ///     Bundle all textures in the given directories.
    /// </summary>
    /// <param name="directories">The directories to bundle.</param>
    /// <param name="resolution">
    ///     The resolution of each texture in the bundle. Must be a power of 2.
    ///     Other resolutions will be ignored.
    /// </param>
    /// <param name="context">The resource context to use.</param>
    /// <returns>The (intermediate) result of the bundling process.</returns>
    public static IntermediateBundle Bundle(IEnumerable<DirectoryInfo> directories, Int32 resolution, IResourceContext context)
    {
        List<ImageSource> sources = directories.Select(directory => ImageSource.Scan(directory, context)).ToList();

        ImageLibrary library = new();

        SheetLoader sheetLoader = new() {Resolution = resolution};
        DeckLoader deckLoader = new() {Library = library, Context = context};

        deckLoader.Initialize();

        foreach (ImageSource source in sources) LoadSource(source, sheetLoader, deckLoader, library, context);

        return library.Bundle(resolution);
    }

    private static void LoadSource(ImageSource source, SheetLoader sheetLoader, DeckLoader deckLoader, ImageLibrary library, IResourceContext context)
    {
        // Both sheets and parts can be loaded without any dependencies.

        IEnumerable<(FileInfo file, Boolean part)> sheets = source.Sheets
            .Select(sheet => (sheet, false)).Concat(source.Parts.Select(part => (part, true)));

        foreach ((FileInfo file, Boolean part) in sheets)
        {
            Sheet? sheet = sheetLoader.Load(file, out (Exception? exception, String message)? error);

            if (sheet != null)
            {
                Boolean added = library.AddSheet(file, sheet, part);

                if (!added)
                    error = (null, $"Name '{ImageLibrary.GetName(file)}' is already in use");
            }

            context.ReportDiscovery(ResourceTypes.TextureBundlePNG, RID.Path(file), error?.exception, error?.message);
        }

        // Decks can depend on parts and sheets, as well as other decks.

        deckLoader.LoadDecks(source.Decks);
    }
}

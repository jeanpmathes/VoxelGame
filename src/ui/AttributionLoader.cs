// <copyright file="AttributionLoader.cs" company="VoxelGame">
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
using System.Security;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.UI;

/// <summary>
///     Loads all attributions.
/// </summary>
public sealed class AttributionLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        DirectoryInfo directory = FileSystem.GetResourceDirectory<Attribution>();

        List<IResource> attributions = [];

        try
        {
            foreach (FileInfo file in directory.EnumerateFiles(FileSystem.GetResourceSearchPattern<Attribution>(), SearchOption.TopDirectoryOnly))
            {
                RID identifier = RID.Path(file);

                String name = file.GetFileNameWithoutExtension().Replace(oldChar: '-', newChar: ' ');
                String text;

                try
                {
                    text = file.ReadAllText();
                }
                catch (IOException exception)
                {
                    attributions.Add(new MissingResource(Attribution.ResourceType, identifier, ResourceIssue.FromException(Level.Warning, exception)));

                    continue;
                }

                attributions.Add(new Attribution(identifier, name, text));
            }
        }
        catch (Exception exception) when (exception is IOException or SecurityException)
        {
            return [new MissingResource(ResourceTypes.Directory, RID.Path(directory), ResourceIssue.FromException(Level.Warning, exception))];
        }

        return attributions;
    }
}

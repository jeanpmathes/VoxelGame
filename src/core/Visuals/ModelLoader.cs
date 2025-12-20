// <copyright file="ModelLoader.cs" company="VoxelGame">
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
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Loads all models.
/// </summary>
public class ModelLoader : IResourceLoader
{
    /// <inheritdoc />
    public String? Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        DirectoryInfo directory = FileSystem.GetResourceDirectory<Model>();

        FileInfo[] files;

        try
        {
            files = directory.GetFiles(FileSystem.GetResourceSearchPattern<Model>());
        }
        catch (DirectoryNotFoundException exception)
        {
            return [new MissingResource(ResourceTypes.Directory, RID.Path(directory), ResourceIssue.FromException(Level.Warning, exception))];
        }

        List<IResource> loaded = [];

        Operations.Launch(async token =>
        {
            foreach (FileInfo file in files)
            {
                Result<Model> result = await Model.LoadAsync(file, token).InAnyContext();

                result.Switch(
                    model => loaded.Add(model),
                    exception => loaded.Add(new MissingResource(ResourceTypes.Model, RID.Path(file), ResourceIssue.FromException(Level.Warning, exception)))
                );
            }
        }).Wait().ThrowIfError();

        return loaded;
    }
}

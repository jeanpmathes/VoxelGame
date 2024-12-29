// <copyright file="Models.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
/// Loads all models.
/// </summary>
public class BlockModelLoader : IResourceLoader
{
    /// <inheritdoc />
    public String? Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        DirectoryInfo directory = FileSystem.GetResourceDirectory<BlockModel>();

        FileInfo[] files;

        try
        {
            files = directory.GetFiles(FileSystem.GetResourceSearchPattern<BlockModel>());
        }
        catch (DirectoryNotFoundException exception)
        {
            return [new MissingResource(ResourceTypes.Directory, RID.Path(directory), ResourceIssue.FromException(Level.Warning, exception))];
        }

        List<IResource> loaded = [];

        foreach (FileInfo file in files)
        {
            Exception? exception = BlockModel.Load(file, out BlockModel model);

            if (exception != null)
            {
                loaded.Add(new MissingResource(ResourceTypes.Model, RID.Path(file), ResourceIssue.FromException(Level.Warning, exception)));
            }
            else
            {
                loaded.Add(model);
            }
        }

        return loaded;
    }
}

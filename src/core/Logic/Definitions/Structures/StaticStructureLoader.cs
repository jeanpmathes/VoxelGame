// <copyright file="StaticStructureLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     Loads all static structures.
/// </summary>
public sealed class StaticStructureLoader : IResourceLoader
{
    private static readonly DirectoryInfo directory = FileSystem.GetResourceDirectory("Structures");

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        FileInfo[] files;

        try
        {
            files = directory.GetFiles(FileSystem.GetResourceSearchPattern<StaticStructure>());
        }
        catch (DirectoryNotFoundException exception)
        {
            return [new MissingResource(ResourceTypes.Directory, RID.Path(directory), ResourceIssue.FromException(Level.Warning, exception))];
        }

        List<IResource> loaded = [];

        foreach (FileInfo file in files)
        {
            Exception? exception = StaticStructure.Load(file, context, out StaticStructure structure);

            if (exception != null) loaded.Add(new MissingResource(ResourceTypes.Structure, RID.Path(file), ResourceIssue.FromException(Level.Warning, exception)));
            else loaded.Add(structure);
        }

        return loaded;
    }
}

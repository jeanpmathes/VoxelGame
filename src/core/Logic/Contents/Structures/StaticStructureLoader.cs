// <copyright file="StaticStructureLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     Loads all static structures.
/// </summary>
public sealed class StaticStructureLoader : IResourceLoader
{
    private static readonly DirectoryInfo directory = FileSystem.GetResourceDirectory("Structures");

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context) => context.Require<Block>(_ =>
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

            Operations.Launch(async token =>
            {
                foreach (FileInfo file in files)
                {
                    Result<StaticStructure> result = await StaticStructure.LoadAsync(file, context, token).InAnyContext();

                    result.Switch(
                        structure => loaded.Add(structure),
                        exception => loaded.Add(new MissingResource(ResourceTypes.Structure, RID.Path(file), ResourceIssue.FromException(Level.Warning, exception))));
                }
            }).Wait().ThrowIfError();

            return loaded;
        });
}

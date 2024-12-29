// <copyright file="AttributionLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
/// Loads all attributions.
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

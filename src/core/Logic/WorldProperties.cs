// <copyright file="WorldProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The properties of a world.
///     These contain some general information about the world and the file-system representation.
/// </summary>
public class WorldProperties : Group
{
    /// <summary>
    ///     Create a new instance of the <see cref="WorldProperties" /> class.
    /// </summary>
    /// <param name="information">Information about the world.</param>
    /// <param name="path">A path to the world.</param>
    public WorldProperties(WorldInformation information, FileSystemInfo path) : base(Language.Properties,
        new Property[]
        {
            new Message(Language.Name, information.Name),
            new FileSystemPath(Language.Path, path),
            path.GetSize() is {} size
                ? new Measure(Language.FileSize, size)
                : new Error(Language.FileSize, Language.Error, isCritical: false),
            new Group(Language.Seed,
                new[]
                {
                    new Integer("L", information.LowerSeed),
                    new Integer("U", information.UpperSeed)
                })
        }) {}
}

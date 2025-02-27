﻿// <copyright file="WorldProperties.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The properties of a world.
///     These contain some general information about the world and the file-system representation.
/// </summary>
public class WorldProperties : Group
{
    private WorldProperties(WorldInformation information, FileSystemInfo path, Memory? memory) : base(Language.Properties,
    [
        new Message(Language.Name, information.Name),
        new FileSystemPath(Language.Path, path),
        memory is {} size
            ? new Measure(Language.FileSize, size)
            : new Error(Language.FileSize, Language.Error, isCritical: false),
        new Group(Language.Seed,
        [
            new Integer("L", information.LowerSeed),
            new Integer("U", information.UpperSeed)
        ])
    ]) {}

    /// <summary>
    ///     Create a new instance of the <see cref="WorldProperties" /> class.
    ///     As this performs file operations, it is an async method.
    /// </summary>
    /// <param name="information">Information about the world.</param>
    /// <param name="path">A path to the world.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>The world properties.</returns>
    public static async Task<WorldProperties> CreateAsync(WorldInformation information, FileSystemInfo path, CancellationToken token = default)
    {
        Memory? size = await path.GetSizeAsync(token).InAnyContext();

        return new WorldProperties(information, path, size);
    }
}

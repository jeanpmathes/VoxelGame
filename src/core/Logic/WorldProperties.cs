// <copyright file="WorldProperties.cs" company="VoxelGame">
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

// <copyright file="WorldInformation.cs" company="VoxelGame">
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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Basic information about a world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public partial class WorldInformation
{
    /// <summary>
    ///     The name of the world.
    /// </summary>
    public String Name { get; set; } = "No Name";

    /// <summary>
    ///     The first seed used to generate the world.
    /// </summary>
    public Int32 UpperSeed { get; set; } = 2133;

    /// <summary>
    ///     The second seed used to generate the world.
    /// </summary>
    public Int32 LowerSeed { get; set; } = 3213;

    /// <summary>
    ///     The size of the world, as extents.
    ///     This means the number of blocks on each side is twice the size.
    /// </summary>
    public UInt32 Size { get; set; } = World.BlockLimit - Chunk.BlockSize * 5;

    /// <summary>
    ///     The creation date of the world.
    /// </summary>
    public DateTime Creation { get; set; } = DateTime.MinValue;

    /// <summary>
    ///     The game version in which the world was created.
    /// </summary>
    public String Version { get; set; } = "missing";

    /// <summary>
    ///     The spawn information of the world.
    /// </summary>
    public SpawnInformation SpawnInformation { get; set; } = new(Vector3d.Zero);

    /// <summary>
    ///     The time of day as a value in the range [0, 1).
    /// </summary>
    public Double TimeOfDay { get; set; }

    /// <summary>
    ///     Save this world information to a file.
    /// </summary>
    /// <param name="path">The save path.</param>
    /// <param name="token">The cancellation token.</param>
    public async Task SaveAsync(FileInfo path, CancellationToken token = default)
    {
        Result result = await Serialize.SaveJsonAsync(this, path, token).InAnyContext();

        result.Switch(
            () => LogInfoSaved(logger, Name, path.FullName),
            exception => LogInfoSavingError(logger, exception, path.FullName));
    }

    /// <summary>
    ///     Load the world information from a file. If loading fails, default world information is returned.
    /// </summary>
    /// <param name="path">The path to load from.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The loaded world information.</returns>
    public static async Task<WorldInformation> LoadAsync(FileInfo path, CancellationToken token = default)
    {
        Result<WorldInformation> result = await Serialize.LoadJsonAsync<WorldInformation>(path, token).InAnyContext();

        return result.Switch(
            information =>
            {
                LogInfoLoaded(logger, information.Name, path.FullName);

                return information;
            },
            exception =>
            {
                LogInfoLoadingError(logger, exception, path.FullName);

                return new WorldInformation();
            });
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldInformation>();

    [LoggerMessage(EventId = LogID.WorldInformation + 0, Level = LogLevel.Error, Message = "The info file could not be saved: {Path}")]
    private static partial void LogInfoSavingError(ILogger logger, Exception exception, String path);

    [LoggerMessage(EventId = LogID.WorldInformation + 1, Level = LogLevel.Debug, Message = "Information for World '{Name}' was saved to: {Path}")]
    private static partial void LogInfoSaved(ILogger logger, String name, String path);

    [LoggerMessage(EventId = LogID.WorldInformation + 2, Level = LogLevel.Error, Message = "The info file could not be loaded: {Path}")]
    private static partial void LogInfoLoadingError(ILogger logger, Exception exception, String path);

    [LoggerMessage(EventId = LogID.WorldInformation + 3, Level = LogLevel.Debug, Message = "Information for World '{Name}' was loaded from: {Path}")]
    private static partial void LogInfoLoaded(ILogger logger, String name, String path);

    #endregion LOGGING
}

/// <summary>
///     World spawn information.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public struct SpawnInformation : IEquatable<SpawnInformation>
{
    /// <summary>
    ///     The x position of the spawn.
    /// </summary>
    public Double X { get; set; }

    /// <summary>
    ///     The y position of the spawn.
    /// </summary>
    public Double Y { get; set; }

    /// <summary>
    ///     The z position of the spawn.
    /// </summary>
    public Double Z { get; set; }

    /// <summary>
    ///     Create spawn information from a vector.
    /// </summary>
    /// <param name="position">The position.</param>
    public SpawnInformation(Vector3d position)
    {
        X = position.X;
        Y = position.Y;
        Z = position.Z;
    }

    /// <summary>
    ///     Get the position of the spawn information.
    /// </summary>
    public Vector3d Position => new(X, Y, Z);

    /// <inheritdoc />
    public Boolean Equals(SpawnInformation other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is SpawnInformation other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     Determine equality between two spawn information.
    /// </summary>
    public static Boolean operator ==(SpawnInformation left, SpawnInformation right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Determine inequality between two spawn information.
    /// </summary>
    public static Boolean operator !=(SpawnInformation left, SpawnInformation right)
    {
        return !left.Equals(right);
    }
}

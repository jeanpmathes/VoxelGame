﻿// <copyright file="WorldInformation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Serialization;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Basic information about a world.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class WorldInformation
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<WorldInformation>();

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
    ///     Save this world information to a file.
    /// </summary>
    /// <param name="path">The save path.</param>
    public void Save(FileInfo path)
    {
        Exception? exception = Serialize.SaveJSON(this, path);

        if (exception != null) logger.LogError(Events.WorldSavingError, exception, "The meta file could not be saved: {Path}", path);
        else
            logger.LogDebug(
                Events.WorldIO,
                "WorldInformation for World '{Name}' was saved to: {Path}",
                Name,
                path);
    }

    /// <summary>
    ///     Load a world information from a file. If loading fails, a default world information is returned.
    /// </summary>
    /// <param name="path">The path to load from.</param>
    /// <returns>The loaded world information.</returns>
    public static WorldInformation Load(FileInfo path)
    {
        Exception? exception = Serialize.LoadJSON(path, out WorldInformation information);

        if (exception != null) logger.LogError(Events.WorldLoadingError, exception, "The meta file could not be loaded: {Path}", path);
        else
            logger.LogDebug(
                Events.WorldIO,
                "WorldInformation for World '{Name}' was loaded from: {Path}",
                information.Name,
                path);

        return information;
    }
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

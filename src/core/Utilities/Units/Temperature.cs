// <copyright file="Temperature.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

public readonly partial struct Temperature
{
    /// <summary>
    ///     Whether the temperature is below the freezing point of water.
    /// </summary>
    public Boolean IsFreezing => DegreesCelsius <= 0;
}

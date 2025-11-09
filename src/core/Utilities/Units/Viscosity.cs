// <copyright file="Viscosity.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

public readonly partial struct Viscosity
{
    private const Double UpdateTicksPerMilliPascalSecond = 15.0;

    /// <summary>
    ///     Gets the viscosity expressed as update ticks.
    /// </summary>
    public Double UpdateTicks
    {
        get => MilliPascalSeconds * UpdateTicksPerMilliPascalSecond;
        init => MilliPascalSeconds = value / UpdateTicksPerMilliPascalSecond;
    }

    /// <summary>
    ///     Gets the viscosity in milli Pascal seconds.
    /// </summary>
    public Double MilliPascalSeconds
    {
        get => PascalSeconds * 1000.0;
        init => PascalSeconds = value / 1000.0;
    }

    /// <summary>
    ///     Converts this viscosity to the update delay used for scheduling.
    /// </summary>
    public UInt32 ToUpdateDelay()
    {
        return (UInt32) Math.Max(val1: 1, Math.Round(UpdateTicks));
    }
}

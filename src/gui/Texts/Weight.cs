// <copyright file="Weight.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.GUI.Texts;

/// <summary>
///     The weight of a font.
/// </summary>
public readonly record struct Weight
{
    /// <summary>
    ///     Thin, with a value of <c>100</c>.
    /// </summary>
    public static readonly Weight Thin = new(100);

    /// <summary>
    ///     ExtraLight, with a value of <c>200</c>.
    /// </summary>
    public static readonly Weight ExtraLight = new(200);

    /// <summary>
    ///     Light, with a value of <c>300</c>.
    /// </summary>
    public static readonly Weight Light = new(300);

    /// <summary>
    ///     SemiLight, with a value of <c>350</c>.
    /// </summary>
    public static readonly Weight SemiLight = new(350);

    /// <summary>
    ///     Normal, with a value of <c>400</c>.
    /// </summary>
    public static readonly Weight Normal = new(400);

    /// <summary>
    ///     Medium, with a value of <c>500</c>.
    /// </summary>
    public static readonly Weight Medium = new(500);

    /// <summary>
    ///     SemiBold, with a value of <c>600</c>.
    /// </summary>
    public static readonly Weight SemiBold = new(600);

    /// <summary>
    ///     Bold, with a value of <c>700</c>.
    /// </summary>
    public static readonly Weight Bold = new(700);

    /// <summary>
    ///     ExtraBold, with a value of <c>800</c>.
    /// </summary>
    public static readonly Weight ExtraBold = new(800);

    /// <summary>
    ///     Black, with a value of <c>900</c>.
    /// </summary>
    public static readonly Weight Black = new(900);

    /// <summary>
    ///     ExtraBlack, with a value of <c>950</c>.
    /// </summary>
    public static readonly Weight ExtraBlack = new(950);

    /// <summary>
    ///     Create a new font weight.
    /// </summary>
    /// <param name="value">The weight value, an integer between 1 and 999.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is outside the valid range.</exception>
    public Weight(Int16 value = 400)
    {
        if (value is < 1 or > 999)
            throw Exceptions.ArgumentOutOfRange(nameof(value), value);

        Value = value;
    }

    /// <summary>
    ///     The weight value, an integer between 1 and 999.
    /// </summary>
    public Int16 Value { get; }

    /// <summary>
    ///     Greater than operator, compares the weight values of two <see cref="Weight" /> instances.
    /// </summary>
    public static Boolean operator >(Weight left, Weight right)
    {
        return left.Value > right.Value;
    }

    /// <summary>
    ///     Less than operator, compares the weight values of two <see cref="Weight" /> instances.
    /// </summary>
    public static Boolean operator <(Weight left, Weight right)
    {
        return left.Value < right.Value;
    }

    /// <summary>
    ///     Greater than or equal operator, compares the weight values of two <see cref="Weight" /> instances.
    /// </summary>
    public static Boolean operator >=(Weight left, Weight right)
    {
        return left.Value >= right.Value;
    }

    /// <summary>
    ///     Less than or equal operator, compares the weight values of two <see cref="Weight" /> instances.
    /// </summary>
    public static Boolean operator <=(Weight left, Weight right)
    {
        return left.Value <= right.Value;
    }
}

// <copyright file="CircularTimeBuffer.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Linq;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A simple class for storing a set amount of time values. They cannot be read, but operations to calculate the
///     average and similar are available.
/// </summary>
public class CircularTimeBuffer
{
    private readonly Double[] buffer;
    private readonly Int32 capacity;

    private Int32 filledSlots;

    private Double total;
    private Int32 writeIndex;

    /// <summary>
    ///     Create a new <see cref="CircularTimeBuffer" /> with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity, must be larger than zero.</param>
    public CircularTimeBuffer(Int32 capacity)
    {
        Debug.Assert(capacity > 0);

        this.capacity = capacity;
        buffer = new Double[capacity];
    }

    /// <summary>
    ///     Get the average of all values in the buffer.
    /// </summary>
    public Double Average => total / filledSlots;

    /// <summary>
    ///     Get the maximum value in the buffer.
    /// </summary>
    public Double Max { get; private set; } = Double.MinValue;

    /// <summary>
    ///     Get the minimum value in the buffer.
    /// </summary>
    public Double Min { get; private set; } = Double.MaxValue;

    /// <summary>
    ///     Write a new value to the buffer.
    /// </summary>
    /// <param name="time">The value to write.</param>
    public void Write(Double time)
    {
        var old = 0.0;

        if (filledSlots == capacity)
            old = buffer[writeIndex];

        total -= old;
        total += time;

        buffer[writeIndex] = time;
        writeIndex++;
        writeIndex %= capacity;

        filledSlots = Math.Min(filledSlots + 1, capacity);

        Max = Math.Max(Max, time);
        Min = Math.Min(Min, time);

        if (MathTools.NearlyEqual(old, Max))
            Max = buffer.Take(filledSlots).Max();

        if (MathTools.NearlyEqual(old, Min))
            Min = buffer.Take(filledSlots).Min();
    }
}

﻿// <copyright file="CircularTimeBuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

    private Double total;

    private Int32 filledSlots;
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

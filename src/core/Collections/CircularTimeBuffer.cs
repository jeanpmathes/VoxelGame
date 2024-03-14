// <copyright file="CircularTimeBuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A simple class for storing a set amount of time values. They cannot be read, but operations to calculate the
///     average and similar are available.
/// </summary>
public class CircularTimeBuffer
{
    private readonly double[] buffer;
    private readonly int capacity;

    private double total;

    private int filledSlots;
    private int writeIndex;

    /// <summary>
    ///     Create a new <see cref="CircularTimeBuffer" /> with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity, must be larger than zero.</param>
    public CircularTimeBuffer(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), @"Capacity has to be larger than zero.");

        this.capacity = capacity;
        buffer = new double[capacity];
    }

    /// <summary>
    ///     Get the average of all values in the buffer.
    /// </summary>
    public double Average => total / filledSlots;

    /// <summary>
    ///     Get the maximum value in the buffer.
    /// </summary>
    public double Max { get; private set; } = double.MinValue;

    /// <summary>
    ///     Get the minimum value in the buffer.
    /// </summary>
    public double Min { get; private set; } = double.MaxValue;

    /// <summary>
    ///     Write a new value to the buffer.
    /// </summary>
    /// <param name="time">The value to write.</param>
    public void Write(double time)
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

        if (VMath.NearlyEqual(old, Max))
            Max = buffer.Take(filledSlots).Max();

        if (VMath.NearlyEqual(old, Min))
            Min = buffer.Take(filledSlots).Min();
    }
}

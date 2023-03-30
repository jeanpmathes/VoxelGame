// <copyright file="CircularTimeBuffer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A simple class for storing a set amount of time values. They cannot be read, but operations to calculate the
///     average and similar are available.
/// </summary>
public class CircularTimeBuffer
{
    private readonly double[] buffer;
    private readonly int capacity;
    private int writeIndex;

    /// <summary>
    ///     Create a new <see cref="CircularTimeBuffer" /> with the specified capacity.
    ///     Initially the buffer is filled with default values, and are already used for all operations.
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
    public double Average => buffer.Sum() / capacity;

    /// <summary>
    ///     Write a new value to the buffer.
    /// </summary>
    /// <param name="time">The value to write.</param>
    public void Write(double time)
    {
        buffer[writeIndex] = time;
        writeIndex++;
        writeIndex %= capacity;
    }
}


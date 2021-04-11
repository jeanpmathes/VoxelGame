// <copyright file="CircularTimeBuffer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Linq;

namespace VoxelGame.Client.Collections
{
    /// <summary>
    /// A simple class for storing a set amount of time values. They cannot be read, but operations to calculate the average and similar are available.
    /// </summary>
    public class CircularTimeBuffer
    {
        private readonly int capacity;
        private readonly double[] buffer;
        private int writeIndex;

        public CircularTimeBuffer(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity has to be larger than zero.");
            this.capacity = capacity;
            buffer = new double[capacity];
        }

        public void Write(double time)
        {
            buffer[writeIndex] = time;
            writeIndex++;
            writeIndex %= capacity;
        }

        public double Average => buffer.Sum() / capacity;
    }
}
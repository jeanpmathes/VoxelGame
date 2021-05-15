// <copyright file="NumberGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Utilities
{
    public static class NumberGenerator
    {
        public static Random Random { get; } = new Random();
    }
}
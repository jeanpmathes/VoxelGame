// <copyright file="SineGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation
{
    /// <summary>
    ///     Creates worlds using sine waves.
    /// </summary>
    public class SineGenerator : IWorldGenerator
    {
        private readonly float a;
        private readonly int amplitude;
        private readonly float b;
        private readonly int mid;

        /// <summary>
        ///     Creates a new sine generator.
        /// </summary>
        /// <param name="amplitude">The amplitude of the sine wave.</param>
        /// <param name="mid">The mid point around which the sine wave is constructed.</param>
        /// <param name="a">Stretch factor a.</param>
        /// <param name="b">Stretch factor b.</param>
        public SineGenerator(int amplitude, int mid, float a = 1f, float b = 1f)
        {
            this.amplitude = amplitude;
            this.mid = mid;
            this.a = a;
            this.b = b;
        }

        /// <inheritdoc />
        public IEnumerable<Block> GenerateColumn(int x, int z)
        {
            int height = (int) (amplitude * (Math.Sin(a * x) - Math.Sin(b * z))) + mid;

            for (var y = 0; y < Section.SectionSize * Chunk.VerticalSectionCount; y++)
                if (y > height) yield return Block.Air;
                else if (y == height) yield return Block.Grass;
                else if (y > height - 5) yield return Block.Dirt;
                else yield return Block.Stone;
        }
    }
}
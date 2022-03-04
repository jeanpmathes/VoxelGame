// <copyright file="BitHelper.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Linq;

namespace VoxelGame.Core.Utilities
{
    /// <summary>
    ///     A utility class for bit manipulation.
    /// </summary>
    public static class BitHelper
    {
        /// <summary>
        ///     Counts how many bits are set in an integer.
        /// </summary>
        /// <param name="n">The integer in which to count the set bits.</param>
        /// <returns>The number of set bits.</returns>
        public static int CountSetBits(int n)
        {
            var count = 0;

            while (n != 0)
            {
                count++;
                n &= n - 1;
            }

            return count;
        }

        /// <summary>
        ///     Counts how many bits are set in an unsigned integer.
        /// </summary>
        /// <param name="n">The unsigned integer in which to count the set bits.</param>
        /// <returns>The number of set bits.</returns>
        public static int CountSetBits(uint n)
        {
            var count = 0;

            while (n != 0)
            {
                count++;
                n &= n - 1;
            }

            return count;
        }

        /// <summary>
        ///     Count the number of set boolean values in an array.
        /// </summary>
        /// <param name="bools">The array or parameter to count the true values in.</param>
        /// <returns>Return the count.</returns>
        public static int CountSetBooleans(params bool[] bools)
        {
            return bools.Sum(b => b.ToInt());
        }
    }
}

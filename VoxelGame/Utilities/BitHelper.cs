// <copyright file="BitHelper.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Utilities
{
    public static class BitHelper
    {
        /// <summary>
        /// Counts how many bits are set in an integer.
        /// </summary>
        /// <param name="n">The integer in which to count the set bits.</param>
        /// <returns>The number of set bits.</returns>
        public static int CountSetBits(int n)
        {
            int count = 0;

            while (n != 0)
            {
                count++;
                n &= n - 1;
            }

            return count;
        }
    }
}
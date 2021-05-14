// <copyright file="UpdateCounter.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Updates
{
    public class UpdateCounter
    {
        /// <summary>
        /// The number of the current update cycle. It is incremented every time a new cycle begins.
        /// </summary>
        public long CurrentUpdate { get; private set; }

        public void IncrementUpdate()
        {
            CurrentUpdate++;
        }

        public void ResetUpdate()
        {
            CurrentUpdate = 0;
        }
    }
}
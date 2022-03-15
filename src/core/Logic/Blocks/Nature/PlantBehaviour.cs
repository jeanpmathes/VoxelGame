// <copyright file="PlantBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     Contains common logic for plant blocks.
    /// </summary>
    public static class PlantBehaviour
    {
        /// <summary>
        ///     Check whether a plant can be placed at a given position.
        /// </summary>
        public static bool CanPlace(World world, Vector3i position)
        {
            BlockInstance? ground = world.GetBlock(position.Below());

            return ground?.Block is IPlantable;
        }

        /// <summary>
        ///     Place a plant block at a given position, assuming the checks pass.
        ///     This operation places the block and sets a lowered-bit.
        /// </summary>
        public static void DoPlace(Block self, World world, Vector3i position)
        {
            bool isLowered = world.IsLowered(position);
            world.SetBlock(self.AsInstance(isLowered ? 1u : 0u), position);
        }
    }
}

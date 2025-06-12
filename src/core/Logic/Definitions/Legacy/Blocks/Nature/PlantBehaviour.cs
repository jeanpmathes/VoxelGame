// <copyright file="PlantBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     Contains common logic for plant blocks.
/// </summary>
public static class PlantBehaviour
{
    /// <summary>
    ///     Check whether a plant can be placed at a given position.
    /// </summary>
    public static Boolean CanPlace(World world, Vector3i position)
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
        Boolean isLowered = world.IsLowered(position);
        world.SetBlock(self.AsInstance(isLowered ? 1u : 0u), position);
    }

    /// <summary>
    ///     Check if the block below the plant is still valid, if not, destroy the plant.
    /// </summary>
    public static void NeighborUpdate(World world, IBlockBase block, Vector3i position, Side side)
    {
        if (side == Side.Bottom && (world.GetBlock(position.Below())?.Block ?? Elements.Legacy.Blocks.Instance.Air) is not IPlantable)
            block.Destroy(world, position);
    }
}

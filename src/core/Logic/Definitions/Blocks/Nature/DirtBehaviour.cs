// <copyright file="DirtBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Contains some utility methods that implement behaviour for dirt-like blocks.
/// </summary>
public static class DirtBehaviour
{
    /// <summary>
    ///     Return true if a covered block can be placed at the given position.
    /// </summary>
    public static bool CanPlaceCovered(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.HasOpaqueTop(position) == false || Logic.Blocks.Instance.Dirt.CanPlace(world, position, entity);
    }

    /// <summary>
    ///     Place a covered block at the given position. This is only allowed if the checks pass.
    /// </summary>
    public static void DoPlaceCovered(Block self, World world, Vector3i position, PhysicsEntity? entity)
    {
        if (world.HasOpaqueTop(position) == false) world.SetBlock(self.AsInstance(), position);
        else Logic.Blocks.Instance.Dirt.Place(world, position, entity);
    }

    /// <summary>
    ///     Perform an update for a covered block. This will check if the placement conditions still hold.
    ///     If not, the block will be replaced with dirt.
    /// </summary>
    public static void BlockUpdateCovered(World world, Vector3i position, BlockSide side)
    {
        if (side == BlockSide.Top && world.HasOpaqueTop(position) == true)
            world.SetBlock(Logic.Blocks.Instance.Dirt.AsInstance(), position);
    }
}


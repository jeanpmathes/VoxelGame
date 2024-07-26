// <copyright file="DirtBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
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
    public static Boolean CanPlaceCovered(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.HasOpaqueTop(position) == false || Elements.Blocks.Instance.Dirt.CanPlace(world, position, actor);
    }

    /// <summary>
    ///     Place a covered block at the given position. This is only allowed if the checks pass.
    /// </summary>
    public static void DoPlaceCovered(Block self, World world, Vector3i position, PhysicsActor? actor)
    {
        if (world.HasOpaqueTop(position) == false) world.SetBlock(self.AsInstance(), position);
        else Elements.Blocks.Instance.Dirt.Place(world, position, actor);
    }

    /// <summary>
    ///     Perform an update for a covered block. This will check if the placement conditions still hold.
    ///     If not, the block will be replaced with dirt.
    /// </summary>
    public static void BlockUpdateCovered(World world, Vector3i position, BlockSide side)
    {
        if (side == BlockSide.Top && world.HasOpaqueTop(position) == true)
            world.SetBlock(Elements.Blocks.Instance.Dirt.AsInstance(), position);
    }
}

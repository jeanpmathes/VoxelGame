// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A custom model block that uses tint.
///     Data bit usage: <c>-ccccc</c>
/// </summary>
// c: color
public class TintedCustomModelBlock : CustomModelBlock, ICombustible
{
    internal TintedCustomModelBlock(String name, String namedID, BlockFlags flags, RID modelName,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            flags with {IsInteractable = true},
            modelName,
            boundingVolume) {}

    /// <inheritdoc />
    protected override IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with {Tint = ((BlockColor) (0b01_1111 & info.Data)).ToTintColor()};
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
    }
}

// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
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
    internal TintedCustomModelBlock(string name, string namedID, BlockFlags flags, string modelName,
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
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        entity.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
    }
}

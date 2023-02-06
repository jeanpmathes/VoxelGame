// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that has differently colored versions. Animation can be activated.
///     Data bit usage: <c>-ccccc</c>
/// </summary>
// c: color
public class TintedBlock : BasicBlock, IWideConnectable
{
    private readonly bool isAnimated;

    internal TintedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout,
        bool isAnimated = false) :
        base(
            name,
            namedId,
            flags with {IsInteractable = true},
            layout)
    {
        this.isAnimated = isAnimated;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with
        {
            Tint = GetTintColor(info.Data),
            IsAnimated = isAnimated
        };
    }

    private static TintColor GetTintColor(uint data)
    {
        return ((BlockColor) (0b01_1111 & data)).ToTintColor();
    }

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        entity.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
    }
}


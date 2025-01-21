// <copyright file="TintedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
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
    private readonly Boolean isAnimated;

    internal TintedBlock(String name, String namedID, BlockFlags flags, TextureLayout layout,
        Boolean isAnimated = false) :
        base(
            name,
            namedID,
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

    private static ColorS GetTintColor(UInt32 data)
    {
        return ((NamedColor) (0b01_1111 & data)).ToColorS();
    }

    /// <inheritdoc />
    protected override void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data)
    {
        actor.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
    }
}

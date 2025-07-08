// <copyright file="ModifiableHeightBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A block that allows to change its height by interacting.
///     Data bit usage: <c>--hhhh</c>
/// </summary>
public class ModifiableHeightBlock : VaryingHeightBlock
{
    internal ModifiableHeightBlock(String name, String namedID, TextureLayout layout, Boolean isSolid = true, Boolean isTrigger = false) :
        base(
            name,
            namedID,
            BlockFlags.Functional with {IsOpaque = true, IsSolid = isSolid, ReceiveCollisions = isTrigger, IsTrigger = isTrigger},
            layout) {}

    /// <inheritdoc />
    protected override void ActorInteract(Actor actor, Vector3i position, UInt32 data)
    {
        UInt32 height = data & 0b00_1111;
        height++;

        if (height <= IHeightVariable.MaximumHeight) actor.World.SetBlock(this.AsInstance(height), position);
    }
}

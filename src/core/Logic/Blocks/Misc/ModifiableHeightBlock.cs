// <copyright file="ModifiableHeightBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A block that allows to change its height by interacting.
///     Data bit usage: <c>--hhhh</c>
/// </summary>
public class ModifiableHeightBlock : VaryingHeightBlock
{
    internal ModifiableHeightBlock(string name, string namedId, TextureLayout layout) :
        base(
            name,
            namedId,
            BlockFlags.Functional,
            layout) {}

    /// <inheritdoc />
    protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
    {
        uint height = data & 0b00_1111;
        height++;

        if (height <= IHeightVariable.MaximumHeight) entity.World.SetBlock(this.AsInstance(height), position);
    }
}

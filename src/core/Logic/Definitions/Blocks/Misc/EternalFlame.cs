// <copyright file="EternalFlame.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that does not stop burning.
///     Data bit usage: <c>------</c>
/// </summary>
public class EternalFlame : BasicBlock, ICombustible
{
    internal EternalFlame(String name, String namedID, TextureLayout layout) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout) {}

    /// <inheritdoc />
    public Boolean Burn(World world, Vector3i position, Block fire)
    {
        return false;
    }
}

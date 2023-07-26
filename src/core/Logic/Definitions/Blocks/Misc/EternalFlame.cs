// <copyright file="EternalFlame.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    internal EternalFlame(string name, string namedId, TextureLayout layout) :
        base(
            name,
            namedId,
            BlockFlags.Basic,
            layout) {}

    /// <inheritdoc />
    public bool Burn(World world, Vector3i position, Block fire)
    {
        return false;
    }
}

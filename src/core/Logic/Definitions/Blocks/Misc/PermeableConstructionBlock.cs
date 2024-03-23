// <copyright file="PermeableConstructionBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A construction block that allows fluids to pass through.
///     Data bit usage: <c>------</c>
/// </summary>
public class PermeableConstructionBlock : ConstructionBlock, IFillable
{
    internal PermeableConstructionBlock(string name, string namedID, TextureLayout layout) : base(name, namedID, layout) {}
}

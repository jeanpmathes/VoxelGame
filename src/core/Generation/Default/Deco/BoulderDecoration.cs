// <copyright file="BoulderDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Places boulders in the world.
/// </summary>
public class BoulderDecoration : PlacementDecoration
{
    /// <summary>
    ///     Creates a new instance of the <see cref="BoulderDecoration" /> class.
    /// </summary>
    public BoulderDecoration(string name, float rarity, Decorator decorator) : base(name, rarity, new Sphere {Radius = 2.5f}, decorator) {}

    /// <inheritdoc />
    protected override Content GetContent(State state)
    {
        return state.Palette.GetStone(state.StoneType);
    }
}

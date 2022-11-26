// <copyright file="BoulderDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Places boulders in the world.
/// </summary>
public class BoulderDecoration : Decoration
{
    private readonly Shape3D shape;

    /// <summary>
    ///     Creates a new instance of the <see cref="BoulderDecoration" /> class.
    /// </summary>
    public BoulderDecoration(string name, float rarity, Decorator decorator) : base(name, rarity, decorator)
    {
        const int diameter = 5;

        shape = new Sphere {Radius = diameter / 2.0f};
        Size = diameter;
    }

    /// <inheritdoc />
    public override int Size { get; }

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, State state, IGrid grid)
    {
        Vector3i extents = new(Size / 2);
        Vector3i center = position - extents;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            Vector3i offset = new(x, y, z);
            Vector3i current = center + offset;

            if (shape.Contains(offset - extents))
                grid.SetContent(state.Palette.GetStone(state.StoneType), current);
        }
    }
}

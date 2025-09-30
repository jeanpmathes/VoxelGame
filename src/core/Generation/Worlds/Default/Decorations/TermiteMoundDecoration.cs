// <copyright file="TermiteMoundDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
///     A termite mound.
/// </summary>
public class TermiteMoundDecoration : Decoration
{
    private readonly Shape3D shape;

    /// <summary>
    ///     Creates a new instance of the <see cref="RootDecoration" /> class.
    /// </summary>
    public TermiteMoundDecoration(String name, Decorator decorator) : base(name, decorator)
    {
        const Int32 diameter = 5;

        shape = new Spheroid {Radius = (1.5, diameter / 2.0, 1.5)};
        Size = diameter;
    }

    /// <inheritdoc />
    public override Int32 Size { get; }

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, in PlacementContext placementContext, IGrid grid)
    {
        Vector3i extents = new(Size / 2);
        Vector3i center = position - extents;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            Vector3i offset = new(x, y, z);
            Vector3i current = center + offset;

            if (!shape.Contains(offset - extents)) continue;

            Content? content = grid.GetContent(current);

            if (content is not {Block: var block})
                continue;

            if (block.IsReplaceable || block.Block.Is<Soil>())
                grid.SetContent(new Content(Blocks.Instance.Organic.TermiteMound), current);
        }
    }
}

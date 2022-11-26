// <copyright file="PlacementDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     A decoration that places blocks in a shape.
/// </summary>
public abstract class PlacementDecoration : Decoration
{
    private readonly Shape3D shape;

    /// <summary>
    ///     Creates a new placement decoration.
    /// </summary>
    /// <param name="name">The name of the decoration.</param>
    /// <param name="rarity">The rarity of the decoration.</param>
    /// <param name="shape">The shape to place blocks in.</param>
    /// <param name="decorator">The decorator to use.</param>
    protected PlacementDecoration(string name, float rarity, Shape3D shape, Decorator decorator) : base(name, rarity, decorator)
    {
        this.shape = shape;

        var size = (int) Math.Floor(shape.Size);
        Debug.Assert(size <= 16, "Size must be less than 16.");
        Debug.Assert(size % 2 != 0, "Size must be odd.");
        Size = size;
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
                grid.SetContent(GetContent(state), current);
        }
    }

    /// <summary>
    ///     Get the content that should be placed by this decorator.
    /// </summary>
    /// <param name="state">The state of the decoration.</param>
    /// <returns>The content to place.</returns>
    protected abstract Content GetContent(State state);
}

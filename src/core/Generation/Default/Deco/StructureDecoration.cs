﻿// <copyright file="StructureDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Structures;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     A decoration that places structures.
/// </summary>
public class StructureDecoration : Decoration
{
    private readonly Structure structure;

    /// <summary>
    ///     Create a new structure decoration.
    /// </summary>
    /// <param name="name">The name of the decoration.</param>
    /// <param name="rarity">The rarity of the decoration.</param>
    /// <param name="structure">The structure to place.</param>
    /// <param name="decorator">The decorator to use.</param>
    public StructureDecoration(string name, float rarity, Structure structure, Decorator decorator) : base(name, rarity, decorator)
    {
        this.structure = structure;

        decorator.SetSizeHint(structure.Extents);

        Debug.Assert(Size <= Section.Size);
    }

    /// <inheritdoc />
    public sealed override int Size => structure.Extents.MaxComponent();

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, State state, IGrid grid)
    {
        int xOffset = structure.Extents.X / 2;
        int zOffset = structure.Extents.Z / 2;

        structure.SetStructureSeed(state.Random.GetHashCode());
        Debug.Assert(structure.IsPlaceable);

        structure.Place(grid, position - new Vector3i(xOffset, y: 0, zOffset));
    }
}

// <copyright file="StructureDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Deco;

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
    public StructureDecoration(String name, Single rarity, Structure structure, Decorator decorator) : base(name, rarity, decorator)
    {
        this.structure = structure;

        decorator.SetSizeHint(structure.Extents);

        Debug.Assert(Size <= Section.Size);
    }

    /// <inheritdoc />
    public sealed override Int32 Size => structure.Extents.MaxComponent();

    /// <inheritdoc />
    protected override void DoPlace(Vector3i position, in PlacementContext placementContext, IGrid grid)
    {
        Int32 xOffset = structure.Extents.X / 2;
        Int32 zOffset = structure.Extents.Z / 2;

        structure.SetStructureSeed(placementContext.Random.GetHashCode());
        Debug.Assert(structure.IsPlaceable);

        structure.Place(grid, position - new Vector3i(xOffset, y: 0, zOffset));
    }
}

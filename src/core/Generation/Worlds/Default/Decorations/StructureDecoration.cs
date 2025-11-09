// <copyright file="StructureDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

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
    /// <param name="structure">The structure to place.</param>
    /// <param name="decorator">The decorator to use.</param>
    public StructureDecoration(String name, Structure structure, Decorator decorator) : base(name, decorator)
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

        structure.Place(placementContext.Random.GetHashCode(), grid, position - new Vector3i(xOffset, y: 0, zOffset));
    }
}

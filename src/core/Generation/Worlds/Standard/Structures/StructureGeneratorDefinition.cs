// <copyright file="StructureGeneratorDefinition.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Structures;

/// <summary>
///     Defines a structure generator which places structures in the world.
/// </summary>
public sealed class StructureGeneratorDefinition : IResource
{
    /// <summary>
    ///     The kind of the structure. Determines placement and generation.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        ///     A surface structure is placed on the surface of the world.
        /// </summary>
        Surface,

        /// <summary>
        ///     An underground structure is placed below the surface of the world.
        /// </summary>
        Underground
    }

    /// <summary>
    ///     Creates a new generated structure.
    /// </summary>
    /// <param name="name">The name of the structure.</param>
    /// <param name="kind">The kind of the structure.</param>
    /// <param name="structure">The structure to generate.</param>
    /// <param name="rarity">The rarity of the structure. A higher value means less common. Must be greater or equal 0.</param>
    /// <param name="offset">An offset to apply to the structure. Must be less than the size of a section.</param>
    public StructureGeneratorDefinition(String name, Kind kind, Structure structure, Single rarity, Vector3i offset)
    {
        Debug.Assert(rarity >= 0);
        rarity += 1;

        Name = name;

        Placement = kind;
        Structure = structure;

        Debug.Assert(Math.Abs(offset.X) < Section.Size);
        Debug.Assert(Math.Abs(offset.Y) < Section.Size);
        Debug.Assert(Math.Abs(offset.Z) < Section.Size);

        Offset = offset;

        EffectiveSectionExtents = (structure.Extents + new Vector3i(offset.Absolute().Xz.MaxComponent())) / Section.Size + Vector3i.One;
        Frequency = 1.0f / (EffectiveSectionExtents.MaxComponent() * 2 * 2 * rarity);
    }

    /// <summary>
    ///     Get the name of the structure.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     The frequency with which the noise generator should be initialized.
    /// </summary>
    public Single Frequency { get; }

    /// <summary>
    ///     The effective extents of the structure in sections.
    /// </summary>
    public Vector3i EffectiveSectionExtents { get; }

    /// <summary>
    ///     The placement kind of the structure.
    /// </summary>
    public Kind Placement { get; }

    /// <summary>
    ///     The offset of the structure from the placement position.
    /// </summary>
    public Vector3i Offset { get; }

    /// <summary>
    ///     The placed structure.
    /// </summary>
    public Structure Structure { get; }

    /// <inheritdoc />
    public RID Identifier => RID.Named<StructureGeneratorDefinition>(Name);

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.WorldStructure;

    #region DISPOING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE
}

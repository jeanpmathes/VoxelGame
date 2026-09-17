// <copyright file="StaticStructureDefinition.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Collections.Generic;
using System.Text.Json.Nodes;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     A JSON-compatible definition of a 3D vector.
/// </summary>
public class Vector
{
    /// <summary>
    ///     The three components of the vector, [X, Y, Z].
    /// </summary>
    public Int32[] Values { get; init; } = [0, 0, 0];
}

/// <summary>
///     A placement of a block/fluid in a structure.
/// </summary>
public class Placement
{
    /// <summary>
    ///     The position of the placement in the structure.
    /// </summary>
    public Vector Position { get; init; } = new();

    /// <summary>
    ///     The block of the placement.
    /// </summary>
    public String Block { get; init; } = nameof(Blocks.Instance.Core.Air);

    /// <summary>
    ///     The state of the block.
    /// </summary>
    public JsonNode State { get; init; } = new JsonObject();

    /// <summary>
    ///     The fluid of the placement.
    /// </summary>
    public String Fluid { get; init; } = nameof(Voxels.Fluids.Instance.None);

    /// <summary>
    ///     The level of the fluid.
    /// </summary>
    public Int32 Level { get; init; } = FluidLevel.Eight.ToInt32();

    /// <summary>
    ///     Whether the fluid is static.
    /// </summary>
    public Boolean IsStatic { get; init; } = true;
}

/// <summary>
///     Defines a static structure for serialization.
/// </summary>
public class StaticStructureDefinition
{
    /// <summary>
    ///     The extents of the structure.
    /// </summary>
    public Vector Extents { get; init; } = new();

    /// <summary>
    ///     All placements of the structure.
    /// </summary>
    public Placement[] Placements { get; init; } = [];
}

#pragma warning restore CS1591

/// <summary>
///     Helper to build a <see cref="StaticStructureDefinition" />.
/// </summary>
public sealed class StaticStructureBuilder
{
    private readonly List<Placement> placements = [];

    /// <summary>
    ///     Add a placement to the structure definition.
    /// </summary>
    public void AddPlacement(Vector3i position, Content content, Boolean isStatic)
    {
        placements.Add(new Placement
        {
            Position = new Vector {Values = [position.X, position.Y, position.Z]},
            Block = content.Block.Block.ContentID.ToString(),
            State = content.Block.Owner.GetJson(content.Block),
            Fluid = content.Fluid.Fluid.NamedID,
            Level = content.Fluid.Level.ToInt32(),
            IsStatic = content.Fluid.IsStatic
        });
    }

    /// <summary>
    ///     Build the structure definition.
    /// </summary>
    public StaticStructureDefinition Build(Vector3i extents)
    {
        return new StaticStructureDefinition
        {
            Extents = new Vector {Values = [extents.X, extents.Y, extents.Z]},
            Placements = placements.ToArray()
        };
    }
}

/// <summary>
///     Helper to read a <see cref="StaticStructureDefinition" />.
/// </summary>
public sealed class StaticStructureDefinitionReader
{
    private readonly StaticStructureDefinition definition;
    private readonly String name;

    private readonly HashSet<Vector3i> covered = [];

    private Int32 placement = -1;

    /// <summary>
    ///     Helper to read a <see cref="StaticStructureDefinition" />.
    /// </summary>
    public StaticStructureDefinitionReader(StaticStructureDefinition definition, String name)
    {
        this.definition = definition;
        this.name = name;

        Extents = GetVector(definition.Extents, name);

        if (!IsInExtents(Extents, new Vector3i(StaticStructure.MaxSize)))
            throw new FileFormatException(name, $"Extents must be positive and not exceed {StaticStructure.MaxSize} in any dimension.");
    }

    /// <summary>
    ///     The extents of the structure.
    /// </summary>
    public Vector3i Extents { get; }

    private Placement CurrentPlacement => definition.Placements[placement];

    /// <summary>
    ///     The position of the current placement.
    /// </summary>
    public Vector3i Position => GetVector(CurrentPlacement.Position, name);

    private static Vector3i GetVector(Vector vector, String name)
    {
        if (vector.Values.Length != 3)
            throw new FileFormatException(name, "Vector must have 3 values.");

        return new Vector3i(vector.Values[0], vector.Values[1], vector.Values[2]);
    }

    private static Boolean IsInExtents(Vector3i vector, Vector3i extents)
    {
        if (vector.X < 0 || vector.X >= extents.X) return false;
        if (vector.Y < 0 || vector.Y >= extents.Y) return false;
        if (vector.Z < 0 || vector.Z >= extents.Z) return false;

        return true;
    }

    /// <summary>
    ///     Advance to the next placement.
    /// </summary>
    public Boolean AdvanceToNextPlacement()
    {
        if (placement + 1 >= definition.Placements.Length) return false;

        placement++;

        if (!IsInExtents(Position, Extents))
            throw new FileFormatException(name, $"Position {Position} is out of bounds.");

        if (!covered.Add(Position))
            throw new FileFormatException(name, $"Position {Position} is already occupied.");

        return true;
    }

    /// <summary>
    ///     Get the block state of the current placement.
    /// </summary>
    public State? GetBlock(out String namedID)
    {
        namedID = CurrentPlacement.Block;

        return Blocks.Instance.TranslateContentID(new CID(namedID))?.States.SetJson(CurrentPlacement.State);
    }

    /// <summary>
    ///     Get the fluid instance of the current placement.
    /// </summary>
    public FluidInstance? GetFluid(out String namedID)
    {
        namedID = CurrentPlacement.Fluid;

        Fluid? fluid = Voxels.Fluids.Instance.TranslateNamedID(namedID);

        if (fluid == null) return null;

        return new FluidInstance(fluid, FluidLevel.FromInt32(CurrentPlacement.Level), CurrentPlacement.IsStatic);
    }
}

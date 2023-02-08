// <copyright file="StaticStructureDefinition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Definitions.Structures;

public partial class StaticStructure
{
 #pragma warning disable CS1591
    public class Vector
    {
        public int[] Values { get; set; } = {0, 0, 0};
    }

    public class Placement
    {
        public Vector Position { get; set; } = new();
        public string Block { get; set; } = nameof(Logic.Blocks.Instance.Air);
        public int Data { get; set; }
        public string Fluid { get; set; } = nameof(Logic.Fluids.Instance.None);
        public int Level { get; set; } = (int) FluidLevel.Eight;
        public bool IsStatic { get; set; } = true;
    }

    /// <summary>
    ///     Defines a static structure for serialization.
    /// </summary>
    public class Definition
    {
        public Vector Extents { get; set; } = new();
        public Placement[] Placements { get; set; } = Array.Empty<Placement>();
    }
     #pragma warning restore CS1591
}


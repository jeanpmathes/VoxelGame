// <copyright file="StaticStructureDefinition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Logic.Definitions.Structures;

public partial class StaticStructure
{
#pragma warning disable CS1591 // Public for JSON serialization.
    public class Vector
    {
        public Int32[] Values { get; set; } = [0, 0, 0];
    }

    public class Placement
    {
        public Vector Position { get; set; } = new();
        public String Block { get; set; } = nameof(Elements.Blocks.Instance.Air);
        public Int32 Data { get; set; }
        public String Fluid { get; set; } = nameof(Elements.Fluids.Instance.None);
        public Int32 Level { get; set; } = (Int32) FluidLevel.Eight;
        public Boolean IsStatic { get; set; } = true;
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

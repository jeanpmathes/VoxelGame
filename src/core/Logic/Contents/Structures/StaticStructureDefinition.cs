// <copyright file="StaticStructureDefinition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text.Json.Nodes;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Logic.Contents.Structures;

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
        public String Block { get; set; } = nameof(Blocks.Instance.Core.Air);
        public JsonNode State { get; set; } = new JsonObject();
        public String Fluid { get; set; } = nameof(Voxels.Fluids.Instance.None);
        public Int32 Level { get; set; } = FluidLevel.Eight.ToInt32();
        public Boolean IsStatic { get; set; } = true;
    }

    /// <summary>
    ///     Defines a static structure for serialization.
    /// </summary>
    public class Definition
    {
        public Vector Extents { get; set; } = new();
        public Placement[] Placements { get; set; } = [];
    }
     #pragma warning restore CS1591
}

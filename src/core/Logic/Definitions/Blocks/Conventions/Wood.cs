// <copyright file="Wood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks.Conventions;

/// <summary>
///     All blocks grouped by the <see cref="WoodConvention" />.
/// </summary>
public class Wood
{
    /// <summary>
    ///     The leaves of the tree.
    /// </summary>
    public required Block Leaves { get; init; }

    /// <summary>
    ///     The log of the tree.
    /// </summary>
    public required Block Log { get; init; }

    /// <summary>
    ///     Planks made out of the tree.
    /// </summary>
    public required Block Planks { get; init; }

    /// <summary>
    ///     The wooden fence can be used as way of marking areas. It does not prevent jumping over it.
    ///     As this fence is made out of wood, it is flammable. Fences can connect to other blocks.
    /// </summary>
    public required Block Fence { get; init; }

    /// <summary>
    ///     Fence gates are meant as a passage trough fences and walls.
    /// </summary>
    public required Block FenceGate { get; init; }

    /// <summary>
    ///     The door allows closing of a room. It can be opened and closed.
    ///     As this door is made out of wood, it is flammable.
    /// </summary>
    public required Block Door { get; init; }

    /// <summary>
    ///     The wooden pipe offers a primitive way of controlling fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public required Block Pipe { get; init; }

    /// <summary>
    /// </summary>
    public required Block Bed { get; init; }
}

/// <summary>
///     A convention for wood types - creating all blocks for the respective tree as well as everything made out of the
///     wood.
/// </summary>
public static class WoodConvention
{
    /// <summary>
    ///     Register a wood type following the convention.
    /// </summary>
    /// <param name="registry">The registry to register the blocks to.</param>
    /// <param name="language">The language for the blocks.</param>
    /// <param name="namedID">The base ID, which is used as a prefix for all created block IDs.</param>
    /// <param name="needles">Whether the tree has needles instead of leaves. Needles will not have neutral tint.</param>
    /// <returns>The wood type.</returns>
    public static Wood RegisterWood(this Registry<Block> registry, Language language, String namedID, Boolean needles = false)
    {
        String texture = namedID.PascalCaseToSnakeCase();

        return new Wood
        {
            Leaves = registry.Register(new NaturalBlock(
                language.Leaves,
                $"{namedID}{nameof(Wood.Leaves)}",
                !needles,
                BlockFlags.Leaves,
                TextureLayout.Uniform(TID.Block($"{texture}_leaves")))),
            Log = registry.Register(new RotatedBlock(
                language.Log,
                $"{namedID}{nameof(Wood.Log)}",
                BlockFlags.Basic,
                TextureLayout.Column(TID.Block($"{texture}_log", x: 0), TID.Block($"{texture}_log", x: 1)))),
            Planks = registry.Register(new OrganicConstructionBlock(
                language.Planks,
                $"{namedID}{nameof(Wood.Planks)}",
                TextureLayout.Uniform(TID.Block($"{texture}_planks")))),
            Fence = registry.Register(new FenceBlock(
                language.Fence,
                $"{namedID}{nameof(Wood.Fence)}",
                TID.Block(texture),
                RID.File<BlockModel>("fence_post"),
                RID.File<BlockModel>("fence_extension"))),
            FenceGate = registry.Register(new GateBlock(
                language.Gate,
                $"{namedID}{nameof(Wood.FenceGate)}",
                TID.Block(texture),
                RID.File<BlockModel>("gate_closed"),
                RID.File<BlockModel>("gate_open"))),
            Door = registry.Register(new OrganicDoorBlock(
                language.Door,
                $"{namedID}{nameof(Wood.Door)}",
                TID.Block($"{texture}_door"),
                RID.File<BlockModel>("door_wood_closed"),
                RID.File<BlockModel>("door_wood_open"))),
            Pipe = registry.Register(new PipeBlock<IPrimitivePipeConnectable>(
                language.Pipe,
                $"{namedID}{nameof(Wood.Pipe)}",
                diameter: 0.3125f,
                TID.Block(texture),
                RID.File<BlockModel>("wood_pipe_center"),
                RID.File<BlockModel>("wood_pipe_connector"),
                RID.File<BlockModel>("wood_pipe_surface"))),
            Bed = registry.Register(new BedBlock(
                language.Bed,
                $"{namedID}{nameof(Wood.Bed)}",
                TID.Block($"{texture}_bed"),
                RID.File<BlockModel>("bed")))
        };
    }

    /// <summary>
    ///     Text for all blocks of the group.
    /// </summary>
    /// <param name="Leaves">The text of the leaves.</param>
    /// <param name="Log">The text of the log.</param>
    /// <param name="Material">The text of the material, used for all blocks made out of the wood.</param>
    public record Language(String Leaves, String Log, String Material)
    {
        /// <summary>
        ///     The text of the planks block.
        /// </summary>
        public String Planks => Materialize(Resources.Language.Language.Planks);

        /// <summary>
        ///     The text of the fence block.
        /// </summary>
        public String Fence => Materialize(Resources.Language.Language.Fence);

        /// <summary>
        ///     The text of the gate block.
        /// </summary>
        public String Gate => Materialize(Resources.Language.Language.Gate);

        /// <summary>
        ///     The text of the door block.
        /// </summary>
        public String Door => Materialize(Resources.Language.Language.Door);

        /// <summary>
        ///     The text of the pipe block.
        /// </summary>
        public String Pipe => Materialize(Resources.Language.Language.Pipe);

        /// <summary>
        ///     The text of the bed block.
        /// </summary>
        public String Bed => Materialize(Resources.Language.Language.Bed);

        private String Materialize(String entity)
        {
            return $"{entity} ({Material})";
        }
    }
}

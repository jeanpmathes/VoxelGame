// <copyright file="Wood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks.Conventions;

/// <summary>
///     All blocks grouped by the <see cref="WoodConvention" />.
/// </summary>
public sealed class Wood(String namedID, String texture, Wood.Tree tree, Wood.Language language, ContentRegistry registry) : IConvention
{
    /// <summary>
    /// </summary>
    public Tree Trees => tree;

    /// <summary>
    ///     The leaves of the tree.
    /// </summary>
    public Block Leaves { get; } = registry.Register(new NaturalBlock(
        language.Leaves,
        $"{namedID}{nameof(Leaves)}",
        !tree.Needles,
        BlockFlags.Leaves,
        TextureLayout.Uniform(TID.Block($"{texture}_leaves"))));

    /// <summary>
    ///     The log of the tree.
    /// </summary>
    public Block Log { get; } = registry.Register(new RotatedBlock(
        language.Log,
        $"{namedID}{nameof(Log)}",
        BlockFlags.Basic,
        TextureLayout.Column(TID.Block($"{texture}_log", x: 0), TID.Block($"{texture}_log", x: 1))));

    /// <summary>
    ///     Planks made out of the tree.
    /// </summary>
    public Block Planks { get; } = registry.Register(new OrganicConstructionBlock(
        language.Planks,
        $"{namedID}{nameof(Planks)}",
        TextureLayout.Uniform(TID.Block($"{texture}_planks"))));

    /// <summary>
    ///     The wooden fence can be used as way of marking areas. It does not prevent jumping over it.
    ///     As this fence is made out of wood, it is flammable. Fences can connect to other blocks.
    /// </summary>
    public Block Fence { get; } = registry.Register(new FenceBlock(
        language.Fence,
        $"{namedID}{nameof(Fence)}",
        TID.Block(texture),
        RID.File<BlockModel>("fence_post"),
        RID.File<BlockModel>("fence_extension")));

    /// <summary>
    ///     Fence gates are meant as a passage trough fences and walls.
    /// </summary>
    public Block FenceGate { get; } = registry.Register(new GateBlock(
        language.Gate,
        $"{namedID}{nameof(FenceGate)}",
        TID.Block(texture),
        RID.File<BlockModel>("gate_closed"),
        RID.File<BlockModel>("gate_open")));

    /// <summary>
    ///     The door allows closing of a room. It can be opened and closed.
    ///     As this door is made out of wood, it is flammable.
    /// </summary>
    public Block Door { get; } = registry.Register(new OrganicDoorBlock(
        language.Door,
        $"{namedID}{nameof(Door)}",
        TID.Block($"{texture}_door"),
        RID.File<BlockModel>("door_wood_closed"),
        RID.File<BlockModel>("door_wood_open")));

    /// <summary>
    ///     The wooden pipe offers a primitive way of controlling fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block Pipe { get; } = registry.Register(new PipeBlock<IPrimitivePipeConnectable>(
        language.Pipe,
        $"{namedID}{nameof(Pipe)}",
        diameter: 0.3125f,
        TID.Block(texture),
        RID.File<BlockModel>("wood_pipe_center"),
        RID.File<BlockModel>("wood_pipe_connector"),
        RID.File<BlockModel>("wood_pipe_surface")));

    /// <summary>
    ///     The bed is a block that allows the player to sleep and set a spawn point.
    /// </summary>
    public Block Bed { get; } = registry.Register(new BedBlock(
        language.Bed,
        $"{namedID}{nameof(Bed)}",
        TID.Block($"{texture}_bed"),
        RID.File<BlockModel>("bed")));

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<Wood>(namedID);

    /// <inheritdoc />
    public String NamedID => namedID;

    /// <inheritdoc />
    public IEnumerable<IContent> Content => registry.RetrieveContent();

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    #region TREE

    /// <summary>
    ///     Specifies the tree that grows this wood.
    /// </summary>
    /// <param name="Height">The height of the tree.</param>
    /// <param name="Shape">The shape of the crown.</param>
    /// <param name="Density">The density of the crown.</param>
    /// <param name="Needles">Whether the tree has needles instead of leaves. Needles will not have neutral tint.</param>
    /// <param name="Soil">The soil the tree grows on.</param>
    public record Tree(Tree.Growth Height, Tree.CrownShape Shape, Tree.CrownDensity Density, Boolean Needles = false, Tree.SoilType Soil = Tree.SoilType.Dirt)
    {
        /// <summary>
        ///     The density of the crown.
        /// </summary>
        public enum CrownDensity
        {
            /// <summary>
            ///     A dense crown.
            /// </summary>
            Dense,

            /// <summary>
            ///     A crown which is neither dense nor sparse.
            /// </summary>
            Normal,

            /// <summary>
            ///     A sparse crown.
            /// </summary>
            Sparse
        }

        /// <summary>
        ///     Possible shapes of the crown.
        /// </summary>
        public enum CrownShape
        {
            /// <summary>
            ///     A spherical crown.
            /// </summary>
            Sphere,

            /// <summary>
            ///     A spheroid-shaped crown which is elongated along the y-axis.
            /// </summary>
            LongSpheroid,

            /// <summary>
            ///     A spheroid-shaped crown which is flattened along the y-axis.
            /// </summary>
            FlatSpheroid,

            /// <summary>
            ///     A cone-shaped crown.
            /// </summary>
            Cone,

            /// <summary>
            ///     A palm-shaped crown.
            /// </summary>
            Palm
        }

        /// <summary>
        ///     The height of the tree.
        /// </summary>
        public enum Growth
        {
            /// <summary>
            ///     A short tree.
            /// </summary>
            Short,

            /// <summary>
            ///     A tree of medium height.
            /// </summary>
            Medium,

            /// <summary>
            ///     A tall tree.
            /// </summary>
            Tall,

            /// <summary>
            ///     A shrub, which is shorter than a short tree.
            /// </summary>
            Shrub
        }

        /// <summary>
        ///     The soil the tree grows on.
        /// </summary>
        public enum SoilType
        {
            /// <summary>
            ///     Dirt-based soil.
            /// </summary>
            Dirt,

            /// <summary>
            ///     Sand-based soil.
            /// </summary>
            Sand
        }
    }

    #endregion TREE

    #region LANGUAGE

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

    #endregion LANGUAGE
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
    /// <param name="registry">The registry to register the content to.</param>
    /// <param name="language">The language for the blocks.</param>
    /// <param name="namedID">The base ID, which is used as a prefix for all created block IDs.</param>
    /// <param name="tree">The tree the wood is made out of.</param>
    /// <returns>The wood type.</returns>
    public static Wood RegisterWood(this ContentRegistry registry, Wood.Language language, String namedID, Wood.Tree tree)
    {
        return registry.RegisterContent(new Wood(namedID, namedID.PascalCaseToSnakeCase(), tree, language, registry.CreateScoped()));
    }
}

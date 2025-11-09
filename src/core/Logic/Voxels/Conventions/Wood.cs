// <copyright file="Wood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A wood, as defined by the <see cref="WoodConvention" />.
/// </summary>
public sealed class Wood(CID contentID, BlockBuilder builder) : Convention<Wood>(contentID, builder)
{
    /// <summary>
    ///     The tree that grows this wood.
    /// </summary>
    public required Tree Trees { get; init; }

    /// <summary>
    ///     The leaves of the tree.
    /// </summary>
    public required Block Leaves { get; init; }

    /// <summary>
    ///     The log of the tree.
    /// </summary>
    public required Block Log { get; init; }

    /// <summary>
    ///     Planks made out of the wood.
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
    ///     The bed is a block that allows the player to sleep and set a spawn point.
    /// </summary>
    public required Block Bed { get; init; }

    #region TREE

    /// <summary>
    ///     Specifies the tree that grows this wood.
    /// </summary>
    /// <param name="Height">The height of the tree.</param>
    /// <param name="Shape">The shape of the crown.</param>
    /// <param name="Density">The density of the crown.</param>
    /// <param name="Needles">Whether the tree has needles instead of leaves. Needles will not have neutral tint.</param>
    /// <param name="Terrain">The terrain the tree grows on.</param>
    public record Tree(Tree.Growth Height, Tree.CrownShape Shape, Tree.CrownDensity Density, Boolean Needles = false, Tree.TerrainType Terrain = Tree.TerrainType.Earth)
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
        ///     The terrain the tree grows on.
        /// </summary>
        public enum TerrainType
        {
            /// <summary>
            ///     Earth-based terrain.
            /// </summary>
            Earth,

            /// <summary>
            ///     Sand-based terrain.
            /// </summary>
            Sand
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
    }

    #endregion TREE
}

/// <summary>
///     A convention on wood types.
/// </summary>
public static class WoodConvention
{
    /// <summary>
    ///     Build a new wood type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the wood type, used to create the block CIDs.</param>
    /// <param name="name">The names of the associated blocks, used for display purposes.</param>
    /// <param name="tree">The tree that grows this wood.</param>
    /// <returns>The created wood type.</returns>
    public static Wood BuildWood(this BlockBuilder b, CID contentID, (String leaves, String log, String wood) name, Wood.Tree tree)
    {
        return b.BuildConvention<Wood>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Wood(contentID, builder)
            {
                Trees = tree,

                Leaves = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Wood.Leaves)}"), name.leaves)
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_leaves")))
                    .WithBehavior<Combustible>()
                    .WithBehavior<TreePart>()
                    .WithConditionalDefinition(!tree.Needles, definition => definition.WithBehavior<NeutralTint>())
                    .WithProperties(properties =>
                    {
                        properties.IsOpaque.ContributeConstant(value: false);
                        properties.MeshFaceAtNonOpaques.ContributeConstant(value: true);
                    })
                    .Complete(),

                Log = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Wood.Log)}"), name.log)
                    .WithTextureLayout(TextureLayout.Column(TID.Block($"{texture}_log", x: 0), TID.Block($"{texture}_log", x: 1)))
                    .WithBehavior<AxisRotatable>()
                    .WithBehavior<Combustible>(combustible =>
                    {
                        combustible.BurnedState.ContributeFunction((_, context) =>
                        {
                            State state = context.state;

                            Block burnedLog = Blocks.Instance.Environment.BurnedLog;
                            State burnedState = new(burnedLog);

                            if (state.Block.Get<AxisRotatable>() is {} source && burnedLog.Get<AxisRotatable>() is {} target)
                            {
                                Axis axis = source.GetAxis(state);
                                burnedState = target.SetAxis(burnedState, axis);
                            }

                            if (burnedLog.Get<Smoldering>() is {} smoldering)
                                burnedState = smoldering.WithEmbers(burnedState);

                            return burnedState;
                        });
                        
                        combustible.CompleteDestructionChance.Initializer.ContributeConstant(Chance.CoinToss);
                    })
                    .WithBehavior<TreePart>()
                    .Complete(),

                Planks = builder
                    .BuildSimpleBlock(new CID($"{contentID}{nameof(Wood.Planks)}"), $"{Language.Planks} ({name.wood})")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_planks")))
                    .WithBehavior<Combustible>(combustible =>
                    {
                        combustible.BurnedState.ContributeFunction((_, _) =>
                        {
                            Block burnedPlanks = Blocks.Instance.Environment.BurnedPlanks;
                            State burnedState = new(burnedPlanks);

                            if (burnedPlanks.Get<Smoldering>() is {} smoldering)
                                burnedState = smoldering.WithEmbers(burnedState);

                            return burnedState;
                        });
                        
                        combustible.CompleteDestructionChance.Initializer.ContributeConstant(Chance.CoinToss);
                    })
                    .WithBehavior<ConstructionMaterial>()
                    .Complete(),

                Fence = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Wood.Fence)}"), $"{Language.Fence} ({name.wood})")
                    .WithBehavior<WideConnecting>(
                        connecting => connecting.Models.Initializer.ContributeConstant(
                            (RID.File<Model>("fence_post"), RID.File<Model>("fence_extension"), null)))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_planks")))
                    .WithBehavior<Fence>()
                    .WithBehavior<Combustible>()
                    .Complete(),

                FenceGate = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Wood.FenceGate)}"), $"{Language.Gate} ({name.wood})")
                    .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("gate_closed"), RID.File<Model>("gate_open")]))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_planks")))
                    .WithBehavior<Combustible>()
                    .WithBehavior<LateralRotatable>()
                    .WithBehavior<Gate>()
                    .WithBehavior<Fillable>()
                    .Complete(),

                Door = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Wood.Door)}"), $"{Language.Door} ({name.wood})")
                    .WithBehavior<Modelled>(
                        modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("door_wood_closed"), RID.File<Model>("door_wood_open")]))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_door")))
                    .WithBehavior<Door>()
                    .Complete(),

                Pipe = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Wood.Pipe)}"), $"{Language.Pipe} ({name.wood})")
                    .WithBehavior<Piped>(piped => piped.Tier.Initializer.ContributeConstant(Piped.PipeTier.Primitive))
                    .WithBehavior<ConnectingPipe>(
                        pipe => pipe.Models.Initializer.ContributeConstant(
                            (RID.File<Model>("wood_pipe_center"), RID.File<Model>("wood_pipe_connector"), RID.File<Model>("wood_pipe_surface"))))
                    .WithTextureOverride(TextureOverride.Single(index: 0, TID.Block(texture)))
                    .WithBehavior<Combustible>()
                    .Complete(),

                Bed = builder
                    .BuildComplexBlock(new CID($"{contentID}{nameof(Wood.Bed)}"), $"{Language.Bed} ({name.wood})")
                    .WithBehavior<LateralRotatable>()
                    .WithBehavior<Modelled>(modelled => modelled.Layers.Initializer.ContributeConstant([RID.File<Model>("bed")]))
                    .WithTextureOverride(new Dictionary<Int32, TID>
                    {
                        [0] = TID.Block($"{texture}_bed", x: 0, y: 1),
                        [1] = TID.Block($"{texture}_bed", x: 0, y: 0),
                        [2] = TID.Block($"{texture}_bed", x: 1, y: 0),
                        [3] = TID.Block($"{texture}_bed", x: 1, y: 1)
                    })
                    .WithBehavior<Bed>()
                    .WithBehavior<DirectionalSidePlacement>()
                    .WithBehavior<Composite>(composite => composite.MaximumSize.Initializer.ContributeConstant((1, 1, 2)))
                    .WithBehavior<Grounded>()
                    .WithBehavior<Paintable>()
                    .WithBehavior<Combustible>()
                    .Complete()
            };
        });
    }
}

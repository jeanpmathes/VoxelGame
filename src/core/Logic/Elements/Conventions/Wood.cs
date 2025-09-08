// <copyright file="Wood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Connection;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Materials;
using VoxelGame.Core.Logic.Elements.Behaviors.Miscellaneous;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// A wood, as defined by the <see cref="WoodConvention"/>.
/// </summary>
public sealed class Wood(String namedID, BlockBuilder builder) : Convention<Wood>(namedID, builder)
{
    /// <summary>
    /// The tree that grows this wood.
    /// </summary>
    public required Tree Trees { get; init; }
    
    /// <summary>
    /// The leaves of the tree.
    /// </summary>
    public required Block Leaves { get; init; }
    
    /// <summary>
    /// The log of the tree.
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
}

/// <summary>
/// A convention on wood types.
/// </summary>
public static class WoodConvention
{
    /// <summary>
    /// Build a new wood type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="name">The names of the associated blocks, used for display purposes.</param>
    /// <param name="namedID">The named ID of the wood type, used to create the block IDs.</param>
    /// <param name="tree">The tree that grows this wood.</param>
    /// <returns>The created wood type.</returns>
    public static Wood BuildWood(this BlockBuilder b, (String leaves, String log, String wood) name, String namedID, Wood.Tree tree)
    {
        return b.BuildConvention<Wood>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Wood(namedID, builder)
            {
                Trees = tree,

                Leaves = builder
                    .BuildSimpleBlock(name.leaves, $"{namedID}{nameof(Wood.Leaves)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_leaves")))
                    .WithBehavior<Combustible>()
                    .WithConditionalDefinition(!tree.Needles, definition => definition.WithBehavior<NeutralTint>())
                    .WithProperties(properties =>
                    {
                        properties.IsOpaque.ContributeConstant(value: false);
                        properties.MeshFaceAtNonOpaques.ContributeConstant(value: true);
                    })
                    .Complete(),

                Log = builder
                    .BuildSimpleBlock(name.log, $"{namedID}{nameof(Wood.Log)}")
                    .WithTextureLayout(TextureLayout.Column(TID.Block($"{texture}_log", x: 0), TID.Block($"{texture}_log", x: 1)))
                    .WithBehavior<AxisRotatable>()
                    .WithBehavior<Combustible>()
                    .Complete(),

                Planks = builder
                    .BuildSimpleBlock($"{Language.Planks} ({name.wood})", $"{namedID}{nameof(Wood.Planks)}")
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"{texture}_planks")))
                    .WithBehavior<Combustible>()
                    .WithBehavior<ConstructionMaterial>()
                    .Complete(),

                Fence = builder
                    .BuildComplexBlock($"{Language.Fence} ({name.wood})", $"{namedID}{nameof(Wood.Fence)}")
                    .WithBehavior<WideConnecting>(connecting => connecting.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("fence_post"), RID.File<BlockModel>("fence_extension"), null)))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_planks")))
                    .WithBehavior<Fence>()
                    .WithBehavior<Combustible>()
                    .Complete(),

                FenceGate = builder
                    .BuildComplexBlock($"{Language.Gate} ({name.wood})", $"{namedID}{nameof(Wood.FenceGate)}")
                    .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("gate_closed"), RID.File<BlockModel>("gate_open")]))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_planks")))
                    .WithBehavior<Combustible>()
                    .WithBehavior<LateralRotatable>()
                    .WithBehavior<Gate>()
                    .Complete(),

                Door = builder
                    .BuildComplexBlock($"{Language.Door} ({name.wood})", $"{namedID}{nameof(Wood.Door)}")
                    .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("door_wood_closed"), RID.File<BlockModel>("door_wood_open")]))
                    .WithTextureOverride(TextureOverride.All(TID.Block($"{texture}_door")))
                    .WithBehavior<Door>()
                    .Complete(),

                Pipe = builder
                    .BuildComplexBlock($"{Language.Pipe} ({name.wood})", $"{namedID}{nameof(Wood.Pipe)}")
                    .WithBehavior<Piped>(piped => piped.TierInitializer.ContributeConstant(Piped.PipeTier.Primitive))
                    .WithBehavior<ConnectingPipe>(pipe => pipe.ModelsInitializer.ContributeConstant((RID.File<BlockModel>("wood_pipe_center"), RID.File<BlockModel>("wood_pipe_connector"), RID.File<BlockModel>("wood_pipe_surface"))))
                    .WithTextureOverride(TextureOverride.All(TID.Block(texture)))
                    .WithBehavior<Combustible>()
                    .Complete(),

                Bed = builder
                    .BuildComplexBlock($"{Language.Bed} ({name.wood})", $"{namedID}{nameof(Wood.Bed)}")
                    .WithBehavior<LateralRotatable>()
                    .WithBehavior<Modelled>(modelled => modelled.LayersInitializer.ContributeConstant([RID.File<BlockModel>("bed")]))
                    .WithTextureOverride(new Dictionary<Int32, TID>
                    {
                        [0] = TID.Block($"{texture}_bed", x: 0, y: 1),
                        [1] = TID.Block($"{texture}_bed", x: 0, y: 0),
                        [2] = TID.Block($"{texture}_bed", x: 1, y: 0),
                        [3] = TID.Block($"{texture}_bed", x: 1, y: 1),
                    })
                    .WithBehavior<Bed>()
                    .WithBehavior<DirectionalSidePlacement>()
                    .WithBehavior<Composite>(composite => composite.MaximumSizeInitializer.ContributeConstant((1, 1, 2)))
                    .WithBehavior<Grounded>()
                    .WithBehavior<Paintable>()
                    .WithBehavior<Combustible>()
                    .Complete()
                
                // todo: es braucht vielleicht ein opposite in placement oder so, oder irgendwo weil derzeit passt es einfach nicht
            };
        });
    }
}

// <copyright file="Organic.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Height;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;
using VoxelGame.Core.Logic.Elements.Behaviors.Orienting;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// Organic blocks are different plants and such which do not fit into other categories.
/// </summary>
/// <param name="builder"></param>
public class Organic(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     A cactus slowly grows upwards. It can only be placed on sand.
    /// </summary>
    public Block Cactus { get; } = builder
        .BuildSimpleBlock(Language.Cactus, nameof(Cactus))
        .WithTextureLayout(TextureLayout.Column(TID.Block("cactus", x: 0), TID.Block("cactus", x: 1)))
        .WithBehavior<Growing>(growing => growing.RequiredGroundInitializer.ContributeConstant(nameof(Blocks.Instance.Environment.Sand)))
        .Complete();

    /// <summary>
    ///     Spiderwebs slow the movement of entities and can be used to trap enemies.
    /// </summary>
    public Block Spiderweb { get; } = builder
        .BuildComplexBlock(Language.SpiderWeb, nameof(Spiderweb))
        .WithBehavior<CrossModel>()
        .WithBehavior<SingleTextured>(texture => texture.DefaultTextureInitializer.ContributeConstant(TID.Block("spider_web")))
        .WithBehavior<Slowing>(slowing => slowing.MaxVelocityInitializer.ContributeConstant(0.01))
        .WithBehavior<DestroyOnLiquid>() // todo: currently is only destroyed with level > 1, but should be destroyed on any liquid
        .WithBehavior<Combustible>()
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();
    
    /// <summary>
    ///     Vines grow downwards, and can hang freely. It is possible to climb them.
    /// </summary>
    public Block Vines { get; } = builder
        .BuildComplexBlock(Language.Vines, nameof(Vines))
        .WithBehavior<FlatModel>()
        .WithBehavior<SingleTextured>(texture => texture.DefaultTextureInitializer.ContributeConstant(TID.Block("vines")))
        .WithBehavior<NeutralTint>()
        .WithBehavior<DestroyOnLiquid>(destroy => destroy.ThresholdInitializer.ContributeConstant(FluidLevel.Two))
        .WithBehavior<Climbable>(climbable => climbable.ClimbingVelocityInitializer.ContributeConstant(value: 2.0))
        .WithBehavior<FourWayRotatable>()
        .WithBehavior<Attached, SingleSided>((attached, siding) =>
        { 
            attached.AttachmentSidesInitializer.ContributeConstant(Sides.Lateral);
            
            attached.AttachedSides.ContributeFunction((_, state) => siding.GetSide(state).Opposite().ToFlag());
            attached.AttachedState.ContributeFunction((_, context) => siding.SetSide(context.state, context.sides.Single().Opposite())); // todo: handling if not single as this allows null, maybe a new extension for sides
        }) 
        .WithBehavior<Vine>()
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();

    /// <summary>
    ///     Lichen is a plant that grows on rocks and trees.
    /// </summary>
    public Block Lichen { get; } = builder // todo: find a way to unify that at with the flat model which currently expects it to be FourWayRotatable - use glue behavior FourWay and SixWay to side flags, using the Sided behavior
        .BuildComplexBlock(Language.Lichen, nameof(Lichen))
        .WithBehavior<FlatModel>()
        .WithBehavior<SingleTextured>(textured => textured.DefaultTextureInitializer.ContributeConstant(TID.Block("lichen")))
        // todo: use attached behavior or similar to update the sides when neighbors are updated
        // todo: also do the placement that selects all sides, could be in attached to with boolean flag to enable
        // todo: do the generator update to select the bottom side if no side is set
        .Complete();
    
    /// <summary>
    ///     Moss is a covering that grows flatly on the ground.
    /// </summary>
    public Block Moss { get; } = builder
        .BuildPartialHeightBlock(Language.Moss, nameof(Moss))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("moss")))
        .WithBehavior<Modifiable>()
        .WithBehavior<DestroyOnLiquid>(destroy => destroy.ThresholdInitializer.ContributeConstant(FluidLevel.Three))
        .WithBehavior<Combustible>()
        .Complete();
    
    /// <summary>
    ///     A fern, a plant that grows in shady areas.
    /// </summary>
    public Block Fern { get; } = builder
        .BuildFoliageBlock(Language.Fern, nameof(Fern))
        .WithTexture(TID.Block("fern"))
        .WithBehavior<NeutralTint>()
        .WithBehavior<CrossPlant>()
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Four))
        .WithProperties(flags => flags.IsReplaceable.ContributeConstant(value: true))
        .Complete();
    
    /// <summary>
    ///     A chanterelle, a type of mushroom.
    /// </summary>
    public Block Chanterelle { get; } = builder
        .BuildFoliageBlock(Language.Chanterelle, nameof(Chanterelle))
        .WithTexture(TID.Block("chanterelle"))
        .WithBehavior<CrossPlant>()
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Four))
        .Complete();
    
    /// <summary>
    ///     An aloe vera plant - a succulent.
    /// </summary>
    public Block AloeVera { get; } = builder
        .BuildFoliageBlock(Language.AloeVera, nameof(AloeVera))
        .WithTexture(TID.Block("aloe_vera"))
        .WithBehavior<CrossPlant>()
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Four))
        .Complete();
    
    /// <summary>
    ///     This block is part of a termite mound.
    /// </summary>
    public Block TermiteMound { get; } = builder
        .BuildSimpleBlock(Language.TermiteMound, nameof(TermiteMound))
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("termite_mound")))
        .WithBehavior<Animated>()
        .WithBehavior<Combustible>()
        .Complete();
}

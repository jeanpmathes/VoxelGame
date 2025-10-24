// <copyright file="Organic.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Organic blocks are different plants and such which do not fit into other categories.
/// </summary>
/// <param name="builder"></param>
public class Organic(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     A cactus slowly grows upwards. It can only be placed on sand.
    /// </summary>
    public Block Cactus { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Cactus)), Language.Cactus)
        .WithTextureLayout(TextureLayout.Column(TID.Block("cactus", x: 0), TID.Block("cactus", x: 1)))
        .WithBehavior<Growing>(growing => growing.RequiredGround.Initializer.ContributeConstant(new CID(nameof(Blocks.Instance.Environment.Sand))))
        .Complete();

    /// <summary>
    ///     Spiderwebs slow the movement of entities and can be used to trap enemies.
    /// </summary>
    public Block Spiderweb { get; } = builder
        .BuildComplexBlock(new CID(nameof(Spiderweb)), Language.SpiderWeb)
        .WithBehavior<CrossModel>()
        .WithBehavior<SingleTextured>(texture => texture.DefaultTexture.Initializer.ContributeConstant(TID.Block("spider_web")))
        .WithBehavior<Slowing>(slowing => slowing.MaxVelocity.Initializer.ContributeConstant(0.01))
        .WithBehavior<DestroyOnLiquid>()
        .WithBehavior<Combustible>()
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();

    /// <summary>
    ///     Vines grow downwards, and can hang freely. It is possible to climb them.
    /// </summary>
    public Block Vines { get; } = builder
        .BuildComplexBlock(new CID(nameof(Vines)), Language.Vines)
        .WithBehavior<FlatModel>(model => model.Width.Initializer.ContributeConstant(value: 0.9))
        .WithBehavior<SingleTextured>(texture => texture.DefaultTexture.Initializer.ContributeConstant(TID.Block("vines")))
        .WithBehavior<NeutralTint>()
        .WithBehavior<DestroyOnLiquid>(destroy => destroy.Threshold.Initializer.ContributeConstant(FluidLevel.Two))
        .WithBehavior<Climbable>(climbable => climbable.ClimbingVelocity.Initializer.ContributeConstant(value: 2.0))
        .WithBehavior<LateralRotatable>()
        .WithBehavior<Attached, SingleSided>((attached, siding) =>
        {
            attached.AttachmentSides.Initializer.ContributeConstant(Sides.Lateral);

            attached.AttachedSides.ContributeFunction((_, state) => siding.GetSide(state).ToFlag());
            attached.AttachedState.ContributeFunction((_, context) => siding.SetSide(context.state, context.sides.SingleOrDefault()));
        })
        .WithBehavior<Vine>()
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();

    /// <summary>
    ///     Lichen is a plant that grows on rocks and trees.
    /// </summary>
    public Block Lichen { get; } = builder
        .BuildComplexBlock(new CID(nameof(Lichen)), Language.Lichen)
        .WithBehavior<FlatModel>()
        .WithBehavior<SingleTextured>(textured => textured.DefaultTexture.Initializer.ContributeConstant(TID.Block("lichen")))
        .WithBehavior<Attached, StoredMultiSided>((attached, siding) =>
        {
            attached.AttachmentSides.Initializer.ContributeConstant(Sides.All);
            attached.Mode.Initializer.ContributeConstant(Attached.AttachmentMode.Multi);

            attached.AttachedSides.ContributeFunction((_, state) => siding.GetSides(state));
            attached.AttachedState.ContributeFunction((_, context) => siding.SetSides(context.state, context.sides));
        })
        .WithBehavior<StoredMultiSided, Constraint>((siding, constraint) => constraint.IsValid.ContributeFunction((_, state) => siding.GetSides(state) != Sides.None))
        .WithProperties(properties => properties.IsOpaque.ContributeConstant(value: false))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .Complete();

    /// <summary>
    ///     Moss is a covering that grows flatly on the ground.
    /// </summary>
    public Block Moss { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Moss)), Language.Moss)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("moss")))
        .WithBehavior<ConstantHeight>(height => height.Height.Initializer.ContributeConstant(BlockHeight.Minimum))
        .WithBehavior<DestroyOnLiquid>(destroy => destroy.Threshold.Initializer.ContributeConstant(FluidLevel.Three))
        .WithBehavior<Combustible>()
        .WithBehavior<CoverPreserving>()
        .WithBehavior<Grounded>()
        .Complete();

    /// <summary>
    ///     A fern, a plant that grows in shady areas.
    /// </summary>
    public Block Fern { get; } = builder
        .BuildFoliageBlock(new CID(nameof(Fern)), Language.Fern)
        .WithTexture(TID.Block("fern"))
        .WithBehavior<NeutralTint>()
        .WithBehavior<CrossPlant>(plant => plant.Height.Initializer.ContributeConstant(value: 0.6))
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Four))
        .WithProperties(flags => flags.Substance.ContributeConstant(Substance.Replaceable))
        .Complete();

    /// <summary>
    ///     A chanterelle, a type of mushroom.
    /// </summary>
    public Block Chanterelle { get; } = builder
        .BuildFoliageBlock(new CID(nameof(Chanterelle)), Language.Chanterelle)
        .WithTexture(TID.Block("chanterelle"))
        .WithBehavior<CrossPlant>(plant => plant.Height.Initializer.ContributeConstant(value: 0.25))
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Four))
        .Complete();

    /// <summary>
    ///     An aloe vera plant - a succulent.
    /// </summary>
    public Block AloeVera { get; } = builder
        .BuildFoliageBlock(new CID(nameof(AloeVera)), Language.AloeVera)
        .WithTexture(TID.Block("aloe_vera"))
        .WithBehavior<CrossPlant>(plant => plant.Height.Initializer.ContributeConstant(value: 0.5))
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Four))
        .Complete();

    /// <summary>
    ///     This block is part of a termite mound.
    /// </summary>
    public Block TermiteMound { get; } = builder
        .BuildSimpleBlock(new CID(nameof(TermiteMound)), Language.TermiteMound)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("termite_mound")))
        .WithBehavior<Animated>()
        .WithBehavior<Combustible>()
        .Complete();
}

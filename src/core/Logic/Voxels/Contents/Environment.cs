// <copyright file="Environment.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Behaviors.Miscellaneous;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     These blocks make up most of the environment and thus are essential for world generation.
/// </summary>
public class Environment(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Soil with some grass on top. Plants can be placed on top of this.
    ///     The grass can burn, creating ash.
    /// </summary>
    public Block Grass { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Grass)), Language.Grass)
        .WithTextureLayout(TextureLayout.UniqueColumn(TID.Block("grass_side"), TID.Block("soil"), TID.Block("grass")))
        .WithWetTextureLayout(TextureLayout.UniqueColumn(TID.Block("grass_side_wet"), TID.Block("soil_wet"), TID.Block("grass_wet")))
        .WithBehavior<NeutralTint>()
        .WithBehavior<Grass>()
        .Complete();

    /// <summary>
    ///     Soil covered with ash. Water can wash the ash away.
    /// </summary>
    public Block AshCoveredSoil { get; } = builder
        .BuildSimpleBlock(new CID(nameof(AshCoveredSoil)), Language.AshCoveredSoil)
        .WithTextureLayout(TextureLayout.UniqueColumn(TID.Block("ash_side"), TID.Block("soil"), TID.Block("ash")))
        .WithBehavior<WashableCoveredSoil>()
        .WithBehavior<GrassSpreadable>()
        .Complete();

    /// <summary>
    ///     Simple soil. Grass next to it can spread over it.
    /// </summary>
    public Block Soil { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Soil)), Language.Soil)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("soil")))
        .WithWetTextureLayout(TextureLayout.Uniform(TID.Block("soil_wet")))
        .WithBehavior<NeutralTint>()
        .WithBehavior<Soil>()
        .WithBehavior<GrassSpreadable>()
        .WithBehavior<AshCoverable>()
        .Complete();

    /// <summary>
    ///     Mud is created when water and soil mix.
    /// </summary>
    public Block Mud { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Mud)), Language.Mud)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("mud")))
        .WithBehavior<Mud>()
        .WithBehavior<Slowing>(slowing => slowing.MaxVelocity.Initializer.ContributeConstant(value: 0.1))
        .Complete();

    /// <summary>
    ///     Mud, but dried out and cracked.
    /// </summary>
    public Block CrackedDriedMud { get; } = builder
        .BuildSimpleBlock(new CID(nameof(CrackedDriedMud)), Language.CrackedDriedMud)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("mud_cracked")))
        .WithBehavior<CrackedMud>()
        .Complete();

    /// <summary>
    ///     Peat is naturally created from organic matter and can be found in bogs.
    /// </summary>
    public Block Peat { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Peat)), Language.Peat)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("peat")))
        .WithBehavior<Mud>()
        .WithBehavior<Slowing>(slowing => slowing.MaxVelocity.Initializer.ContributeConstant(value: 0.1))
        .Complete();

    /// <summary>
    ///     Tilled soil that allows many plants to grow.
    ///     While plants can also grow on normal soil, this block allows full growth.
    /// </summary>
    public Block Farmland { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Farmland)), Language.Farmland)
        .WithTextureLayout(TextureLayout.UniqueTop(TID.Block("soil"), TID.Block("farmland")))
        .WithWetTextureLayout(TextureLayout.UniqueTop(TID.Block("soil_wet"), TID.Block("farmland_wet")))
        .WithBehavior<ConstantHeight>(height => height.Height.Initializer.ContributeConstant(BlockHeight.Maximum - 1))
        .WithBehavior<CompletableGround>(ground => ground.Replacement.Initializer.ContributeConstant(new CID(nameof(Soil))))
        .WithBehavior<CoveredSoil>()
        .WithBehavior<Plantable>(plantable => plantable.SupportsFullGrowth.Initializer.ContributeConstant(value: true))
        .Complete();

    /// <summary>
    ///     Clay is found beneath the ground and blocks groundwater flow.
    /// </summary>
    public Block Clay { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Clay)), Language.Clay)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("clay")))
        .Complete();

    /// <summary>
    ///     Permafrost is a type of soil that is frozen solid.
    /// </summary>
    public Block Permafrost { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Permafrost)), Language.Permafrost)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("permafrost")))
        .Complete();

    /// <summary>
    ///     The path is a soil block with its top layer trampled.
    /// </summary>
    public Block Path { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Path)), Language.Path)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("soil")))
        .WithWetTextureLayout(TextureLayout.Uniform(TID.Block("soil_wet")))
        .WithBehavior<ConstantHeight>(height => height.Height.Initializer.ContributeConstant(BlockHeight.Maximum - 1))
        .WithBehavior<CompletableGround>(ground => ground.Replacement.Initializer.ContributeConstant(new CID(nameof(Soil))))
        .WithBehavior<CoveredSoil>()
        .WithBehavior<Plantable>()
        .Complete();

    /// <summary>
    ///     Sand naturally forms and allows water to flow through it.
    /// </summary>
    public Block Sand { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Sand)), Language.Sand)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("sand")))
        .WithWetTint()
        .WithBehavior<Sandy>()
        .WithBehavior<Regolith>()
        .Complete();

    /// <summary>
    ///     Gravel, which is made out of small pebbles, allows water to flow through it.
    /// </summary>
    public Block Gravel { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Gravel)), Language.Gravel)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("gravel")))
        .WithWetTint()
        .WithBehavior<Loose>()
        .WithBehavior<Regolith>()
        .Complete();

    /// <summary>
    ///     Tall grass that can grow through three stages.
    /// </summary>
    public Block TallGrass { get; } = builder
        .BuildFoliageBlock(new CID(nameof(TallGrass)), Language.TallGrass)
        .WithTexture(TID.Block("grass_tall"))
        .WithBehavior<NeutralTint>()
        .WithBehavior<Composite>()
        .WithBehavior<CompositePlant>()
        .WithBehavior<TallGrass>(grass => grass.Textures.Initializer.ContributeConstant((
            TID.Block("grass_tall"),
            TID.Block("grass_taller"),
            TID.Block("grass_tallest"))))
        .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Five))
        .Complete();

    /// <summary>
    ///     Snow covers the ground and can have different heights.
    /// </summary>
    public Block Snow { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Snow)), Language.Snow)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("snow")))
        .WithBehavior<StoredHeight16>()
        .WithBehavior<Modifiable>()
        .WithBehavior<Grounded>()
        .WithBehavior<Densifying>()
        .WithBehavior<CoverPreserving, PartialHeight>((preserving, height) => preserving.Preservation.ContributeFunction((_, state) => height.GetHeight(state) < BlockHeight.Maximum - 2))
        .WithBehavior<DestroyOnFluid>()
        .Complete();

    /// <summary>
    ///     Pulverized snow allows entities to sink into it.
    /// </summary>
    public Block PulverizedSnow { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(PulverizedSnow)), Language.PulverizedSnow)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("snow_pulverized")))
        .WithBehavior<StoredHeight16>()
        .WithBehavior<Modifiable>()
        .WithBehavior<Grounded>()
        .WithBehavior<Densifying>()
        .WithBehavior<DestroyOnFluid>()
        .WithBehavior<Slowing>(slowing => slowing.MaxVelocity.Initializer.ContributeConstant(value: 0.01))
        .WithProperties(properties => properties.IsSolid.ContributeConstant(value: false))
        .WithBehavior<CoverPreserving, PartialHeight>((preserving, height) => preserving.Preservation.ContributeFunction((_, state) => !height.GetHeight(state).IsFull))
        .Complete();

    /// <summary>
    ///     A block made out of frozen water.
    /// </summary>
    public Block Ice { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Ice)), Language.Ice)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("ice")))
        .WithBehavior<StoredHeight16>()
        .WithBehavior<Modifiable>()
        .Complete();

    /// <summary>
        ///     Ash is the remainder of burning processes.
    /// </summary>
    public Block Ash { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Ash)), Language.Ash)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("ash")))
        .WithBehavior<Smoldering>(smoldering => smoldering.EmberTexture.Initializer.ContributeConstant(TextureLayout.Uniform(TID.Block("ash_embers"))))
        .WithBehavior<DestroyOnFluid>()
        .WithBehavior<ConstantHeight>()
        .WithBehavior<Grounded>()
        .Complete();

    /// <summary>
    ///     The charred remainder of a log.
    /// </summary>
    public Block BurnedLog { get; } = builder
        .BuildSimpleBlock(new CID(nameof(BurnedLog)), Language.BurnedLog)
        .WithTextureLayout(TextureLayout.Column(TID.Block("burned_log", x: 0), TID.Block("burned_log", x: 1)))
        .WithBehavior<AxisRotatable>()
        .WithBehavior<Smoldering>(smoldering => smoldering.EmberTexture.Initializer.ContributeConstant(TextureLayout.Uniform(TID.Block("burned_log_embers"))))
        .Complete();

    /// <summary>
    ///     The charred remainder of wooden planks.
    /// </summary>
    public Block BurnedPlanks { get; } = builder
        .BuildSimpleBlock(new CID(nameof(BurnedPlanks)), Language.BurnedPlanks)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("burned_planks")))
        .WithBehavior<Smoldering>(smoldering => smoldering.EmberTexture.Initializer.ContributeConstant(TextureLayout.Uniform(TID.Block("burned_planks_embers"))))
        .Complete();

    /// <summary>
    ///     Fire is a dangerous block that spreads onto nearby flammable blocks.
    ///     When spreading, fire burns blocks which can destroy them.
    /// </summary>
    public Block Fire { get; } = builder
        .BuildComplexBlock(new CID(nameof(Fire)), Language.Fire)
        .WithBehavior<Fire>(fire => fire.Models.Initializer.ContributeConstant((RID.File<Model>("fire_complete"), RID.File<Model>("fire_side"), RID.File<Model>("fire_top"))))
        .WithBehavior<DestroyOnLiquid>()
        .Complete();

    /// <summary>
    ///     Roots grow at the bottom of trees.
    /// </summary>
    public Block Roots { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Roots)), Language.Roots)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("roots")))
        .WithBehavior<Fillable>()
        .WithBehavior<Combustible>()
        .Complete();

    /// <summary>
    ///     Salt is contained in seawater, it becomes usable after the water evaporates.
    /// </summary>
    public Block Salt { get; } = builder
        .BuildPartialHeightBlock(new CID(nameof(Salt)), Language.Salt)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("salt")))
        .WithBehavior<StoredHeight16>()
        .WithBehavior<Modifiable>()
        .WithBehavior<Salt>()
        .WithBehavior<Grounded>()
        .WithBehavior<CoverPreserving>()
        .Complete();
}

// <copyright file="Blocks.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Blocks;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Elements;

#pragma warning disable S1192 // Definition class is not clean, but allows for better readability everywhere else.
#pragma warning disable S104 // Definition class is not clean, but allows for better readability everywhere else.
#pragma warning disable S1200 // Definition class is not clean, but allows for better readability everywhere else.

/// <summary>
///     Contains all block definitions of the core game.
/// </summary>
public sealed partial class Blocks(Registry<Block> registry)
{
    private SpecialBlocks? special;

    /// <summary>
    /// Get all blocks in this class.
    /// </summary>
    public IEnumerable<Block> Content => registry.Values;

    /// <summary>
    ///     Get the blocks instance.
    /// </summary>
    public static Blocks Instance { get; } = new(new Registry<Block>(block => block.NamedID));

    /// <summary>
    ///     Gets the count of registered blocks.
    /// </summary>
    public Int32 Count => registry.Count;

    /// <summary>
    ///     Get special blocks as their actual block type.
    /// </summary>
    internal SpecialBlocks Specials => special ??= new SpecialBlocks(this);

    /// <summary>
    ///     Translates a block ID to a reference to the block that has that ID. If the ID is not valid, air is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block TranslateID(UInt32 id)
    {
        if (registry.Count > id) return registry[(Int32) id];

        LogUnknownID(logger, id, Air.NamedID);

        return Air;
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID.
    /// </summary>
    /// <param name="namedID">The named ID to translate.</param>
    /// <returns>The block, or null if no block with the ID exists.</returns>
    public Block? TranslateNamedID(String namedID)
    {
        return registry[namedID];
    }

    /// <summary>
    /// Translate a named ID to the block that has that ID. If the ID is not valid, air is returned.
    /// </summary>
    /// <param name="namedID">The named ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    public Block SafelyTranslateNamedID(String namedID)
    {
        Block? block = registry[namedID];

        if (block != null)
            return block;

        LogUnknownNamedID(logger, namedID, Air.NamedID);

        return Air;
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    internal class SpecialBlocks(Blocks blocks)
    {
        public ConcreteBlock Concrete { get; } = (ConcreteBlock) blocks.Concrete;
        public SnowBlock Snow { get; } = (SnowBlock) blocks.Snow;
        public ModifiableHeightBlock Ice { get; } = (ModifiableHeightBlock) blocks.Ice;
        public RotatedBlock Log { get; } = (RotatedBlock) blocks.Log;
        public FlatBlock Vines { get; } = (FlatBlock) blocks.Vines;
        public SaltBlock Salt { get; } = (SaltBlock) blocks.Salt;
    }

    #region NATURAL BLOCKS

    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public Block Air { get; } = registry.Register(new AirBlock(Language.Air, nameof(Air)));

    /// <summary>
    ///     Dirt with some grass on top. Plants can be placed on top of this.
    ///     The grass can burn, creating ash.
    /// </summary>
    public Block Grass { get; } = registry.Register(new GrassBlock(
        Language.Grass,
        nameof(Grass),
        TextureLayout.UniqueColumn("grass_side", "dirt", "grass"),
        TextureLayout.UniqueColumn("grass_side_wet", "dirt_wet", "grass_wet")));

    /// <summary>
    ///     Grass that was burned. Water can burn the ash away.
    /// </summary>
    public Block GrassBurned { get; } = registry.Register(new CoveredGrassSpreadableBlock(
        Language.AshCoveredDirt,
        nameof(GrassBurned),
        TextureLayout.UniqueColumn("ash_side", "dirt", "ash"),
        hasNeutralTint: false));

    /// <summary>
    ///     Simple dirt. Grass next to it can spread over it.
    /// </summary>
    public Block Dirt { get; } = registry.Register(new DirtBlock(
        Language.Dirt,
        nameof(Dirt),
        TextureLayout.Uniform("dirt"),
        TextureLayout.Uniform("dirt_wet")));

    /// <summary>
    ///     Tilled dirt that allows many plants to grow.
    ///     While plants can also grow on normal grass, this block allows full growth.
    /// </summary>
    public Block Farmland { get; } = registry.Register(new InsetDirtBlock(
        Language.Farmland,
        nameof(Farmland),
        TextureLayout.UniqueTop("dirt", "farmland"),
        TextureLayout.UniqueTop("dirt_wet", "farmland_wet"),
        supportsFullGrowth: true));

    /// <summary>
    ///     A tall grassy plant. Fluids will destroy it, if the level is too high.
    /// </summary>
    public Block TallGrass { get; } = registry.Register(new CrossPlantBlock(
        Language.TallGrass,
        nameof(TallGrass),
        "tall_grass",
        BlockFlags.Replaceable,
        BoundingVolume.CrossBlock));

    /// <summary>
    ///     A much larger version of the normal tall grass.
    /// </summary>
    public Block VeryTallGrass { get; } = registry.Register(new DoubleCrossPlantBlock(
        Language.VeryTallGrass,
        nameof(VeryTallGrass),
        "very_tall_grass",
        topTexOffset: 1,
        BoundingVolume.CrossBlock));

    /// <summary>
    ///     A simple flower.
    /// </summary>
    public Block Flower { get; } = registry.Register(new CrossPlantBlock(
        Language.Flower,
        nameof(Flower),
        "flower",
        BlockFlags.Replaceable,
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f))));

    /// <summary>
    ///     A very tall flower.
    /// </summary>
    public Block TallFlower { get; } = registry.Register(new DoubleCrossPlantBlock(
        Language.TallFlower,
        nameof(TallFlower),
        "tall_flower",
        topTexOffset: 1,
        BoundingVolume.CrossBlock));

    /// <summary>
    ///     Mud is created when water and dirt mix.
    /// </summary>
    public Block Mud { get; } = registry.Register(new MudBlock(
        Language.Mud,
        nameof(Mud),
        TextureLayout.Uniform("mud"),
        maxVelocity: 0.1f));

    /// <summary>
    ///     Pumice is created when lava rapidly cools down, while being in contact with a lot of water.
    /// </summary>
    public Block Pumice { get; } = registry.Register(new BasicBlock(
        Language.Pumice,
        nameof(Pumice),
        BlockFlags.Basic,
        TextureLayout.Uniform("pumice")));

    /// <summary>
    ///     Obsidian is a dark type of stone, that forms from lava.
    /// </summary>
    public Block Obsidian { get; } = registry.Register(new BasicBlock(
        Language.Obsidian,
        nameof(Obsidian),
        BlockFlags.Basic,
        TextureLayout.Uniform("obsidian")));

    /// <summary>
    ///     Snow covers the ground, and can have different heights.
    /// </summary>
    public Block Snow { get; } = registry.Register(new SnowBlock(
        Language.Snow,
        nameof(Snow),
        TextureLayout.Uniform("snow")));

    /// <summary>
    ///     Leaves are transparent parts of the tree. They are flammable.
    /// </summary>
    public Block Leaves { get; } = registry.Register(new NaturalBlock(
        Language.Leaves,
        nameof(Leaves),
        hasNeutralTint: true,
        new BlockFlags
        {
            IsSolid = true,
            RenderFaceAtNonOpaques = true
        },
        TextureLayout.Uniform("leaves")));

    /// <summary>
    ///     Log is the unprocessed, wooden part of a tree. As it is made of wood, it is flammable.
    /// </summary>
    public Block Log { get; } = registry.Register(new RotatedBlock(
        Language.Log,
        nameof(Log),
        BlockFlags.Basic,
        TextureLayout.Column("log:0", "log:1")));

    /// <summary>
    ///     Processed wood that can be used as construction material. It is flammable.
    /// </summary>
    public Block Wood { get; } = registry.Register(new OrganicConstructionBlock(
        Language.Wood,
        nameof(Wood),
        TextureLayout.Uniform("wood")));

    /// <summary>
    ///     Sand naturally forms and allows water to flow through it.
    /// </summary>
    public Block Sand { get; } = registry.Register(new PermeableBlock(
        Language.Sand,
        nameof(Sand),
        TextureLayout.Uniform("sand")));

    /// <summary>
    ///     Gravel, which is made out of small pebbles, allows water to flow through it.
    /// </summary>
    public Block Gravel { get; } = registry.Register(new PermeableBlock(
        Language.Gravel,
        nameof(Gravel),
        TextureLayout.Uniform("gravel")));

    /// <summary>
    ///     Ahs is the remainder of burning processes.
    /// </summary>
    public Block Ash { get; } = registry.Register(new BasicBlock(
        Language.Ash,
        nameof(Ash),
        BlockFlags.Basic,
        TextureLayout.Uniform("ash")));

    #endregion NATURAL BLOCKS

    #region PLANT BLOCKS

    /// <summary>
    ///     A cactus slowly grows upwards. It can only be placed on sand.
    /// </summary>
    public Block Cactus { get; } = registry.Register(new GrowingBlock(
        Language.Cactus,
        nameof(Cactus),
        TextureLayout.Column("cactus:0", "cactus:1"),
        nameof(Sand),
        maxHeight: 4));

    /// <summary>
    ///     Pumpkins are the fruit of the pumpkin plant. They have to be placed on solid ground.
    /// </summary>
    public Block Pumpkin { get; } = registry.Register(new GroundedBlock(
        Language.Pumpkin,
        nameof(Pumpkin),
        BlockFlags.Basic,
        TextureLayout.Column("pumpkin:0", "pumpkin:1")));

    /// <summary>
    ///     Melons are the fruit of the melon plant. They have to be placed on solid ground.
    /// </summary>
    public Block Melon { get; } = registry.Register(new GroundedBlock(
        Language.Melon,
        nameof(Melon),
        BlockFlags.Basic,
        TextureLayout.Column("melon:0", "melon:1")));

    /// <summary>
    ///     Spiderwebs slow the movement of entities and can be used to trap enemies.
    /// </summary>
    public Block Spiderweb { get; } = registry.Register(new SpiderWebBlock(
        Language.SpiderWeb,
        nameof(Spiderweb),
        "spider_web",
        maxVelocity: 0.01f));

    /// <summary>
    ///     Vines grow downwards, and can hang freely. It is possible to climb them.
    /// </summary>
    public Block Vines { get; } = registry.Register(new GrowingFlatBlock(
        Language.Vines,
        nameof(Vines),
        "vines",
        climbingVelocity: 2f,
        slidingVelocity: 1f));

    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Block Flax { get; } = registry.Register(new CropBlock(
        Language.Flax,
        nameof(Flax),
        "flax",
        second: 0,
        third: 1,
        fourth: 2,
        fifth: 3,
        sixth: 3,
        final: 4,
        dead: 5));

    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public Block Potatoes { get; } = registry.Register(new CropBlock(
        Language.Potatoes,
        nameof(Potatoes),
        "potato",
        second: 1,
        third: 1,
        fourth: 2,
        fifth: 2,
        sixth: 3,
        final: 4,
        dead: 5));

    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public Block Onions { get; } = registry.Register(new CropBlock(
        Language.Onions,
        nameof(Onions),
        "onion",
        second: 0,
        third: 1,
        fourth: 1,
        fifth: 2,
        sixth: 2,
        final: 3,
        dead: 4));

    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Block Wheat { get; } = registry.Register(new CropBlock(
        Language.Wheat,
        nameof(Wheat),
        "wheat",
        second: 0,
        third: 1,
        fourth: 1,
        fifth: 2,
        sixth: 2,
        final: 3,
        dead: 4));

    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public Block Maize { get; } = registry.Register(new DoubleCropBlock(
        Language.Maize,
        nameof(Maize),
        "maize",
        dead: 0,
        first: 1,
        second: 2,
        third: 2,
        (3, 6),
        (3, 6),
        (4, 7),
        (5, 8)));

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public Block PumpkinPlant { get; } = registry.Register(new FruitCropBlock(
        Language.PumpkinPlant,
        nameof(PumpkinPlant),
        "pumpkin_plant",
        nameof(Pumpkin)));

    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public Block MelonPlant { get; } = registry.Register(new FruitCropBlock(
        Language.MelonPlant,
        nameof(MelonPlant),
        "melon_plant",
        nameof(Melon)));

    #endregion PLANT BLOCKS

    #region BUILDING BLOCKS

    /// <summary>
    ///     Glass is transparent block.
    /// </summary>
    public Block Glass { get; } = registry.Register(new GlassBlock(
        Language.Glass,
        nameof(Glass),
        TextureLayout.Uniform("glass")));

    /// <summary>
    ///     Tiled glass is like glass, but made out of four tiles.
    /// </summary>
    public Block GlassTiled { get; } = registry.Register(new GlassBlock(
        Language.TiledGlass,
        nameof(GlassTiled),
        TextureLayout.Uniform("glass_tiled")));

    /// <summary>
    ///     The steel block is a metal construction block.
    /// </summary>
    public Block Steel { get; } = registry.Register(new ConstructionBlock(
        Language.Steel,
        nameof(Steel),
        TextureLayout.Uniform("steel")));

    /// <summary>
    ///     A ladder allows climbing up and down.
    /// </summary>
    public Block Ladder { get; } = registry.Register(new FlatBlock(
        Language.Ladder,
        nameof(Ladder),
        "ladder",
        climbingVelocity: 3f,
        slidingVelocity: 1f));

    /// <summary>
    ///     Small tiles for construction of floors and walls.
    /// </summary>
    public Block TilesSmall { get; } = registry.Register(new ConstructionBlock(
        Language.SmallTiles,
        nameof(TilesSmall),
        TextureLayout.Uniform("small_tiles")));

    /// <summary>
    ///     Large tiles for construction of floors and walls.
    /// </summary>
    public Block TilesLarge { get; } = registry.Register(new ConstructionBlock(
        Language.LargeTiles,
        nameof(TilesLarge),
        TextureLayout.Uniform("large_tiles")));

    /// <summary>
    ///     Black checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardBlack { get; } = registry.Register(new TintedBlock(
        Language.CheckerboardTilesBlack,
        nameof(TilesCheckerboardBlack),
        BlockFlags.Basic,
        TextureLayout.Uniform("checkerboard_tiles_black")));

    /// <summary>
    ///     White checkerboard tiles come in different colors.
    /// </summary>
    public Block TilesCheckerboardWhite { get; } = registry.Register(new TintedBlock(
        Language.CheckerboardTilesWhite,
        nameof(TilesCheckerboardWhite),
        BlockFlags.Basic,
        TextureLayout.Uniform("checkerboard_tiles_white")));

    /// <summary>
    ///     Clay bricks, placed as a block and connected with mortar.
    ///     This block is a construction material.
    /// </summary>
    public Block ClayBricks { get; } = registry.Register(new ConstructionBlock(
        Language.ClayBricks,
        nameof(ClayBricks),
        TextureLayout.Uniform("bricks")));

    /// <summary>
    ///     Red plastic is a construction material.
    /// </summary>
    public Block RedPlastic { get; } = registry.Register(new ConstructionBlock(
        Language.RedPlastic,
        nameof(RedPlastic),
        TextureLayout.Uniform("red_plastic")));

    /// <summary>
    ///     Concrete is a flexible construction material that can have different heights and colors.
    ///     It can be build using fluid concrete.
    /// </summary>
    public Block Concrete { get; } = registry.Register(new ConcreteBlock(
        Language.Concrete,
        nameof(Concrete),
        TextureLayout.Uniform("concrete")));

    #endregion BUILDING BLOCKS

    #region DECORATION BLOCKS

    /// <summary>
    ///     The vase is a decorative block that must be placed on solid ground.
    /// </summary>
    public Block Vase { get; } = registry.Register(new CustomModelBlock(
        Language.Vase,
        nameof(Vase),
        BlockFlags.Basic,
        RID.File<BlockModel>("vase"),
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.375f, z: 0.25f))));

    /// <summary>
    ///     The bed can be placed to set a different spawn point.
    ///     It is possible to change to color of a bed.
    /// </summary>
    public Block Bed { get; } = registry.Register(new BedBlock(Language.Bed, nameof(Bed), RID.File<BlockModel>("bed")));

    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public Block Wool { get; } = registry.Register(new OrganicTintedBlock(
        Language.Wool,
        nameof(Wool),
        TextureLayout.Uniform("wool")));

    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public Block WoolDecorated { get; } = registry.Register(new OrganicTintedBlock(
        Language.WoolDecorated,
        nameof(WoolDecorated),
        TextureLayout.Uniform("wool_decorated")));

    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public Block Carpet { get; } = registry.Register(new TintedCustomModelBlock(
        Language.Carpet,
        nameof(Carpet),
        BlockFlags.Basic,
        RID.File<BlockModel>("carpet"),
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f))));

    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public Block CarpetDecorated { get; } = registry.Register(new TintedCustomModelBlock(
        Language.CarpetDecorated,
        nameof(CarpetDecorated),
        BlockFlags.Basic,
        RID.File<BlockModel>("carpet_decorated"),
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f))));

    /// <summary>
    ///     Glass panes are a thin alternative to glass blocks.
    ///     They connect to some neighboring blocks.
    /// </summary>
    public Block GlassPane { get; } = registry.Register(new ThinConnectingBlock(
        Language.GlassPane,
        nameof(GlassPane),
        isOpaque: false,
        RID.File<BlockModel>("pane_glass_post"),
        RID.File<BlockModel>("pane_glass_side"),
        RID.File<BlockModel>("pane_glass_extension")));

    /// <summary>
    ///     Steel bars are a thin, but strong barrier.
    /// </summary>
    public Block Bars { get; } = registry.Register(new ThinConnectingBlock(
        Language.Bars,
        nameof(Bars),
        isOpaque: true,
        RID.File<BlockModel>("bars_post"),
        RID.File<BlockModel>("bars_side"),
        RID.File<BlockModel>("bars_extension")));

    #endregion DECORATION BLOCKS

    #region ACCESS BLOCKS

    /// <summary>
    ///     The wooden fence can be used as way of marking areas. It does not prevent jumping over it.
    ///     As this fence is made out of wood, it is flammable. Fences can connect to other blocks.
    /// </summary>
    public Block FenceWood { get; } = registry.Register(new FenceBlock(
        Language.WoodenFence,
        nameof(FenceWood),
        "wood",
        RID.File<BlockModel>("fence_post"),
        RID.File<BlockModel>("fence_extension")));

    /// <summary>
    ///     A wall constructed using clay bricks.
    ///     The wall does not prevent jumping over it, and can connect to other blocks.
    /// </summary>
    public Block ClayBrickWall { get; } = registry.Register(new WallBlock(
        Language.ClayBrickWall,
        nameof(ClayBrickWall),
        "bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     The steel door allows closing of a room. It can be opened and closed.
    /// </summary>
    public Block DoorSteel { get; } = registry.Register(new DoorBlock(
        Language.SteelDoor,
        nameof(DoorSteel),
        RID.File<BlockModel>("door_steel_closed"),
        RID.File<BlockModel>("door_steel_open")));

    /// <summary>
    ///     The wooden door allows closing of a room. It can be opened and closed.
    ///     As this door is made out of wood, it is flammable.
    /// </summary>
    public Block DoorWood { get; } = registry.Register(new OrganicDoorBlock(
        Language.WoodenDoor,
        nameof(DoorWood),
        RID.File<BlockModel>("door_wood_closed"),
        RID.File<BlockModel>("door_wood_open")));

    /// <summary>
    ///     Fence gates are meant as a passage trough fences and walls.
    /// </summary>
    public Block GateWood { get; } = registry.Register(new GateBlock(
        Language.WoodenGate,
        nameof(GateWood),
        RID.File<BlockModel>("gate_wood_closed"),
        RID.File<BlockModel>("gate_wood_open")));

    #endregion ACCESS BLOCKS

    #region FLUID FLOW BLOCKS

    /// <summary>
    ///     The fluid barrier can be used to control fluid flow. It can be opened and closed.
    ///     It does not prevent gasses from flowing through it.
    /// </summary>
    public Block FluidBarrier { get; } = registry.Register(new FluidBarrierBlock(
        Language.Barrier,
        nameof(FluidBarrier),
        TextureLayout.Uniform("fluid_barrier_closed"),
        TextureLayout.Uniform("fluid_barrier_open")));

    /// <summary>
    ///     The industrial steel pipe can be used to control fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block SteelPipe { get; } = registry.Register(new PipeBlock<IIndustrialPipeConnectable>(
        Language.SteelPipe,
        nameof(SteelPipe),
        diameter: 0.375f,
        RID.File<BlockModel>("steel_pipe_center"),
        RID.File<BlockModel>("steel_pipe_connector"),
        RID.File<BlockModel>("steel_pipe_surface")));

    /// <summary>
    ///     The wooden pipe offers a primitive way of controlling fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public Block WoodenPipe { get; } = registry.Register(new PipeBlock<IPrimitivePipeConnectable>(
        Language.WoodenPipe,
        nameof(WoodenPipe),
        diameter: 0.3125f,
        RID.File<BlockModel>("wood_pipe_center"),
        RID.File<BlockModel>("wood_pipe_connector"),
        RID.File<BlockModel>("wood_pipe_surface")));

    /// <summary>
    ///     This pipe is a special steel pipe that can only form straight connections.
    ///     It is ideal for parallel pipes.
    /// </summary>
    public Block StraightSteelPipe { get; } = registry.Register(new StraightSteelPipeBlock(
        Language.SteelPipeStraight,
        nameof(StraightSteelPipe),
        diameter: 0.375f,
        RID.File<BlockModel>("steel_pipe_straight")));

    /// <summary>
    ///     This is a special steel pipe that can be closed. It prevents all fluid flow.
    /// </summary>
    public Block PipeValve { get; } = registry.Register(new SteelPipeValveBlock(
        Language.ValvePipe,
        nameof(PipeValve),
        diameter: 0.375f,
        RID.File<BlockModel>("steel_pipe_valve_open"),
        RID.File<BlockModel>("steel_pipe_valve_closed")));

    /// <summary>
    ///     The pump can lift fluids up when interacted with.
    ///     It can only lift up to a threshold of 16 blocks.
    /// </summary>
    public Block Pump { get; } = registry.Register(new PumpBlock(
        Language.Pump,
        nameof(Pump),
        pumpDistance: 16,
        TextureLayout.Uniform("pump")));

    #endregion FLUID FLOW BLOCKS

    #region SPECIAL BLOCKS

    /// <summary>
    ///     Fire is a dangerous block that spreads onto nearby flammable blocks.
    ///     When spreading, fire burns blocks which can destroy them.
    /// </summary>
    public Block Fire { get; } = registry.Register(new FireBlock(
        Language.Fire,
        nameof(Fire),
        RID.File<BlockModel>("fire_complete"),
        RID.File<BlockModel>("fire_side"),
        RID.File<BlockModel>("fire_top")));

    /// <summary>
    ///     This is a magical pulsating block.
    /// </summary>
    public Block Pulsating { get; } = registry.Register(new TintedBlock(
        Language.PulsatingBlock,
        nameof(Pulsating),
        BlockFlags.Basic,
        TextureLayout.Uniform("pulsating"),
        isAnimated: true));

    /// <summary>
    ///     The eternal flame, once lit, will never go out naturally.
    /// </summary>
    public Block EternalFlame { get; } = registry.Register(new EternalFlame(
        Language.EternalFlame,
        nameof(EternalFlame),
        TextureLayout.Uniform("eternal_flame")));

    /// <summary>
    ///     The path is a dirt block with its top layer trampled.
    /// </summary>
    public Block Path { get; } = registry.Register(new InsetDirtBlock(
        Language.Path,
        nameof(Path),
        TextureLayout.Uniform("dirt"),
        TextureLayout.Uniform("dirt_wet"),
        supportsFullGrowth: false));

    #endregion SPECIAL BLOCKS

    #region NEW BLOCKS

    /// <summary>
    ///     Granite is found next to volcanic activity.
    /// </summary>
    public Block Granite { get; } = registry.Register(new BasicBlock(
        Language.Granite,
        nameof(Granite),
        BlockFlags.Basic,
        TextureLayout.Uniform("granite")));

    /// <summary>
    ///     Sandstone is found all over the world and especially in the desert.
    /// </summary>
    public Block Sandstone { get; } = registry.Register(new BasicBlock(
        Language.Sandstone,
        nameof(Sandstone),
        BlockFlags.Basic,
        TextureLayout.Uniform("sandstone")));

    /// <summary>
    ///     Limestone is found all over the world and especially in oceans.
    /// </summary>
    public Block Limestone { get; } = registry.Register(new BasicBlock(
        Language.Limestone,
        nameof(Limestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("limestone")));

    /// <summary>
    ///     Marble is a rarer stone type.
    /// </summary>
    public Block Marble { get; } = registry.Register(new BasicBlock(
        Language.Marble,
        nameof(Marble),
        BlockFlags.Basic,
        TextureLayout.Uniform("marble")));

    /// <summary>
    ///     Clay is found beneath the ground and blocks groundwater flow.
    /// </summary>
    public Block Clay { get; } = registry.Register(new BasicBlock(
        Language.Clay,
        nameof(Clay),
        BlockFlags.Basic,
        TextureLayout.Uniform("clay")));

    /// <summary>
    ///     Permafrost is a type of soil that is frozen solid.
    /// </summary>
    public Block Permafrost { get; } = registry.Register(new BasicBlock(
        Language.Permafrost,
        nameof(Permafrost),
        BlockFlags.Basic,
        TextureLayout.Uniform("permafrost")));

    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public Block Core { get; } = registry.Register(new BasicBlock(
        Language.Core,
        nameof(Core),
        BlockFlags.Basic,
        TextureLayout.Uniform("core")));

    /// <summary>
    ///     A block made out of frozen water.
    /// </summary>
    public Block Ice { get; } = registry.Register(new ModifiableHeightBlock(
        Language.Ice,
        nameof(Ice),
        TextureLayout.Uniform("ice")));

    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public Block Error { get; } = registry.Register(new BasicBlock(
        Language.Error,
        nameof(Error),
        BlockFlags.Basic,
        TextureLayout.Uniform("missing_texture")));

    /// <summary>
    ///     Roots grow at the bottom of trees.
    /// </summary>
    public Block Roots { get; } = registry.Register(new PermeableNaturalBlock(
        Language.Roots,
        nameof(Roots),
        hasNeutralTint: false,
        BlockFlags.Basic,
        TextureLayout.Uniform("roots")));

    /// <summary>
    ///     Salt is contained in sea water, it becomes usable after the water evaporates.
    /// </summary>
    public Block Salt { get; } = registry.Register(new SaltBlock(
        Language.Salt,
        nameof(Salt),
        TextureLayout.Uniform("salt")));

    /// <summary>
    ///     Worked granite is a processed granite block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedGranite { get; } = registry.Register(new BasicBlock(
        Language.GraniteWorked,
        nameof(WorkedGranite),
        BlockFlags.Basic,
        TextureLayout.Uniform("granite_worked")));

    /// <summary>
    ///     Worked sandstone is a processed sandstone block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedSandstone { get; } = registry.Register(new BasicBlock(
        Language.SandstoneWorked,
        nameof(WorkedSandstone),
        BlockFlags.Basic,
        TextureLayout.Uniform("sandstone_worked")));

    /// <summary>
    ///     Worked limestone is a processed limestone block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedLimestone { get; } = registry.Register(new BasicBlock(
        Language.LimestoneWorked,
        nameof(WorkedLimestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("limestone_worked")));

    /// <summary>
    ///     Worked marble is a processed marble block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedMarble { get; } = registry.Register(new BasicBlock(
        Language.MarbleWorked,
        nameof(WorkedMarble),
        BlockFlags.Basic,
        TextureLayout.Uniform("marble_worked")));

    /// <summary>
    ///     Worked pumice is a processed pumice block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedPumice { get; } = registry.Register(new BasicBlock(
        Language.PumiceWorked,
        nameof(WorkedPumice),
        BlockFlags.Basic,
        TextureLayout.Uniform("pumice_worked")));

    /// <summary>
    ///     Worked obsidian is a processed obsidian block.
    ///     The block can be used for construction.
    /// </summary>
    public Block WorkedObsidian { get; } = registry.Register(new BasicBlock(
        Language.ObsidianWorked,
        nameof(WorkedObsidian),
        BlockFlags.Basic,
        TextureLayout.Uniform("obsidian_worked")));

    /// <summary>
    ///     Worked granite with decorations carved into one side.
    ///     The carvings show a pattern of geometric shapes.
    /// </summary>
    public Block DecoratedGranite { get; } = registry.Register(new OrientedBlock(
        Language.GraniteDecorated,
        nameof(DecoratedGranite),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("granite_worked_decorated", "granite_worked")));

    /// <summary>
    ///     Worked sandstone with decorations carved into one side.
    ///     The carvings depict the desert sun.
    /// </summary>
    public Block DecoratedSandstone { get; } = registry.Register(new OrientedBlock(
        Language.SandstoneDecorated,
        nameof(DecoratedSandstone),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("sandstone_worked_decorated", "sandstone_worked")));

    /// <summary>
    ///     Worked limestone with decorations carved into one side.
    ///     The carvings show the ocean and life within it.
    /// </summary>
    public Block DecoratedLimestone { get; } = registry.Register(new OrientedBlock(
        Language.LimestoneDecorated,
        nameof(DecoratedLimestone),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("limestone_worked_decorated", "limestone_worked")));

    /// <summary>
    ///     Worked marble with decorations carved into one side.
    ///     The carvings depict an ancient temple.
    /// </summary>
    public Block DecoratedMarble { get; } = registry.Register(new OrientedBlock(
        Language.MarbleDecorated,
        nameof(DecoratedMarble),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("marble_worked_decorated", "marble_worked")));

    /// <summary>
    ///     Worked pumice with decorations carved into one side.
    ///     The carvings depict heat rising from the earth.
    /// </summary>
    public Block DecoratedPumice { get; } = registry.Register(new OrientedBlock(
        Language.PumiceDecorated,
        nameof(DecoratedPumice),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("pumice_worked_decorated", "pumice_worked")));

    /// <summary>
    ///     Worked obsidian with decorations carved into one side.
    ///     The carvings depict an ancient artifact.
    /// </summary>
    public Block DecoratedObsidian { get; } = registry.Register(new OrientedBlock(
        Language.ObsidianDecorated,
        nameof(DecoratedObsidian),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("obsidian_worked_decorated", "obsidian_worked")));

    /// <summary>
    ///     Marble cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block GraniteCobblestone { get; } = registry.Register(new BasicBlock(
        Language.GraniteCobbles,
        nameof(GraniteCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("granite_cobbles")));

    /// <summary>
    ///     Sandstone cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block SandstoneCobblestone { get; } = registry.Register(new BasicBlock(
        Language.SandstoneCobbles,
        nameof(SandstoneCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("sandstone_cobbles")));

    /// <summary>
    ///     Limestone cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block LimestoneCobblestone { get; } = registry.Register(new BasicBlock(
        Language.LimestoneCobbles,
        nameof(LimestoneCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("limestone_cobbles")));

    /// <summary>
    ///     Marble cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block MarbleCobblestone { get; } = registry.Register(new BasicBlock(
        Language.MarbleCobbles,
        nameof(MarbleCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("marble_cobbles")));

    /// <summary>
    ///     Pumice cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block PumiceCobblestone { get; } = registry.Register(new BasicBlock(
        Language.PumiceCobbles,
        nameof(PumiceCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("pumice_cobbles")));

    /// <summary>
    ///     Obsidian cobbles, connected by mortar, to form basic road paving.
    ///     The rough surface is not ideal for carts.
    /// </summary>
    public Block ObsidianCobblestone { get; } = registry.Register(new BasicBlock(
        Language.ObsidianCobbles,
        nameof(ObsidianCobblestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("obsidian_cobbles")));

    /// <summary>
    ///     Paving made out of processed granite.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block GranitePaving { get; } = registry.Register(new BasicBlock(
        Language.GranitePaving,
        nameof(GranitePaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("granite_paving")));

    /// <summary>
    ///     Paving made out of processed sandstone.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block SandstonePaving { get; } = registry.Register(new BasicBlock(
        Language.SandstonePaving,
        nameof(SandstonePaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("sandstone_paving")));

    /// <summary>
    ///     Paving made out of processed limestone.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block LimestonePaving { get; } = registry.Register(new BasicBlock(
        Language.LimestonePaving,
        nameof(LimestonePaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("limestone_paving")));

    /// <summary>
    ///     Paving made out of processed marble.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block MarblePaving { get; } = registry.Register(new BasicBlock(
        Language.MarblePaving,
        nameof(MarblePaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("marble_paving")));

    /// <summary>
    ///     Paving made out of processed pumice.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block PumicePaving { get; } = registry.Register(new BasicBlock(
        Language.PumicePaving,
        nameof(PumicePaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("pumice_paving")));

    /// <summary>
    ///     Paving made out of processed obsidian.
    ///     The processing ensures a smoother surface.
    /// </summary>
    public Block ObsidianPaving { get; } = registry.Register(new BasicBlock(
        Language.ObsidianPaving,
        nameof(ObsidianPaving),
        BlockFlags.Basic,
        TextureLayout.Uniform("obsidian_paving")));

    /// <summary>
    ///     When breaking granite, it turns into granite rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block GraniteRubble { get; } = registry.Register(new PermeableBlock(
        Language.GraniteRubble,
        nameof(GraniteRubble),
        TextureLayout.Uniform("granite_rubble")));

    /// <summary>
    ///     When breaking sandstone, it turns into sandstone rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block SandstoneRubble { get; } = registry.Register(new PermeableBlock(
        Language.SandstoneRubble,
        nameof(SandstoneRubble),
        TextureLayout.Uniform("sandstone_rubble")));

    /// <summary>
    ///     When breaking limestone, it turns into limestone rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block LimestoneRubble { get; } = registry.Register(new PermeableBlock(
        Language.LimestoneRubble,
        nameof(LimestoneRubble),
        TextureLayout.Uniform("limestone_rubble")));

    /// <summary>
    ///     When breaking marble, it turns into marble rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block MarbleRubble { get; } = registry.Register(new PermeableBlock(
        Language.MarbleRubble,
        nameof(MarbleRubble),
        TextureLayout.Uniform("marble_rubble")));

    /// <summary>
    ///     When breaking pumice, it turns into pumice rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block PumiceRubble { get; } = registry.Register(new PermeableBlock(
        Language.PumiceRubble,
        nameof(PumiceRubble),
        TextureLayout.Uniform("pumice_rubble")));

    /// <summary>
    ///     When breaking obsidian, it turns into obsidian rubble.
    ///     The block is loose and as such allows water to flow through it.
    /// </summary>
    public Block ObsidianRubble { get; } = registry.Register(new PermeableBlock(
        Language.ObsidianRubble,
        nameof(ObsidianRubble),
        TextureLayout.Uniform("obsidian_rubble")));

    /// <summary>
    ///     A wall made out of granite rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block GraniteWall { get; } = registry.Register(new WallBlock(
        Language.GraniteWall,
        nameof(GraniteWall),
        "granite_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall made out of sandstone rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block SandstoneWall { get; } = registry.Register(new WallBlock(
        Language.SandstoneWall,
        nameof(SandstoneWall),
        "sandstone_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall made out of limestone rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block LimestoneWall { get; } = registry.Register(new WallBlock(
        Language.LimestoneWall,
        nameof(LimestoneWall),
        "limestone_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));


    /// <summary>
    ///     A wall made out of marble rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block MarbleWall { get; } = registry.Register(new WallBlock(
        Language.MarbleWall,
        nameof(MarbleWall),
        "marble_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall made out of pumice rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block PumiceWall { get; } = registry.Register(new WallBlock(
        Language.PumiceWall,
        nameof(PumiceWall),
        "pumice_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall made out of obsidian rubble.
    ///     Walls are used to create barriers and can connect to other blocks.
    /// </summary>
    public Block ObsidianWall { get; } = registry.Register(new WallBlock(
        Language.ObsidianWall,
        nameof(ObsidianWall),
        "obsidian_rubble",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     Granite, cut into bricks and connected with mortar.
    /// </summary>
    public Block GraniteBricks { get; } = registry.Register(new ConstructionBlock(
        Language.GraniteBricks,
        nameof(GraniteBricks),
        TextureLayout.Uniform("granite_bricks")));

    /// <summary>
    ///     Sandstone, cut into bricks and connected with mortar.
    /// </summary>
    public Block SandstoneBricks { get; } = registry.Register(new ConstructionBlock(
        Language.SandstoneBricks,
        nameof(SandstoneBricks),
        TextureLayout.Uniform("sandstone_bricks")));

    /// <summary>
    ///     Limestone, cut into bricks and connected with mortar.
    /// </summary>
    public Block LimestoneBricks { get; } = registry.Register(new ConstructionBlock(
        Language.LimestoneBricks,
        nameof(LimestoneBricks),
        TextureLayout.Uniform("limestone_bricks")));

    /// <summary>
    ///     Marble, cut into bricks and connected with mortar.
    /// </summary>
    public Block MarbleBricks { get; } = registry.Register(new ConstructionBlock(
        Language.MarbleBricks,
        nameof(MarbleBricks),
        TextureLayout.Uniform("marble_bricks")));

    /// <summary>
    ///     Pumice, cut into bricks and connected with mortar.
    /// </summary>
    public Block PumiceBricks { get; } = registry.Register(new ConstructionBlock(
        Language.PumiceBricks,
        nameof(PumiceBricks),
        TextureLayout.Uniform("pumice_bricks")));

    /// <summary>
    ///     Obsidian, cut into bricks and connected with mortar.
    /// </summary>
    public Block ObsidianBricks { get; } = registry.Register(new ConstructionBlock(
        Language.ObsidianBricks,
        nameof(ObsidianBricks),
        TextureLayout.Uniform("obsidian_bricks")));

    /// <summary>
    ///     A wall constructed using granite bricks.
    /// </summary>
    public Block GraniteBrickWall { get; } = registry.Register(new WallBlock(
        Language.GraniteBrickWall,
        nameof(GraniteBrickWall),
        "granite_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall constructed using sandstone bricks.
    /// </summary>
    public Block SandstoneBrickWall { get; } = registry.Register(new WallBlock(
        Language.SandstoneBrickWall,
        nameof(SandstoneBrickWall),
        "sandstone_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall constructed using limestone bricks.
    /// </summary>
    public Block LimestoneBrickWall { get; } = registry.Register(new WallBlock(
        Language.LimestoneBrickWall,
        nameof(LimestoneBrickWall),
        "limestone_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall constructed using marble bricks.
    /// </summary>
    public Block MarbleBrickWall { get; } = registry.Register(new WallBlock(
        Language.MarbleBrickWall,
        nameof(MarbleBrickWall),
        "marble_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall constructed using pumice bricks.
    /// </summary>
    public Block PumiceBrickWall { get; } = registry.Register(new WallBlock(
        Language.PumiceBrickWall,
        nameof(PumiceBrickWall),
        "pumice_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A wall constructed using obsidian bricks.
    /// </summary>
    public Block ObsidianBrickWall { get; } = registry.Register(new WallBlock(
        Language.ObsidianBrickWall,
        nameof(ObsidianBrickWall),
        "obsidian_bricks",
        RID.File<BlockModel>("wall_post"),
        RID.File<BlockModel>("wall_extension"),
        RID.File<BlockModel>("wall_extension_straight")));

    /// <summary>
    ///     A column, made out of granite.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block GraniteColumn { get; } = registry.Register(new ConstructionBlock(
        Language.GraniteColumn,
        nameof(GraniteColumn),
        TextureLayout.Column("granite_column", "granite_worked")));

    /// <summary>
    ///     A column, made out of sandstone.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block SandstoneColumn { get; } = registry.Register(new ConstructionBlock(
        Language.SandstoneColumn,
        nameof(SandstoneColumn),
        TextureLayout.Column("sandstone_column", "sandstone_worked")));

    /// <summary>
    ///     A column, made out of limestone.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block LimestoneColumn { get; } = registry.Register(new ConstructionBlock(
        Language.LimestoneColumn,
        nameof(LimestoneColumn),
        TextureLayout.Column("limestone_column", "limestone_worked")));

    /// <summary>
    ///     A column, made out of marble.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block MarbleColumn { get; } = registry.Register(new ConstructionBlock(
        Language.MarbleColumn,
        nameof(MarbleColumn),
        TextureLayout.Column("marble_column", "marble_worked")));

    /// <summary>
    ///     A column, made out of pumice.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block PumiceColumn { get; } = registry.Register(new ConstructionBlock(
        Language.PumiceColumn,
        nameof(PumiceColumn),
        TextureLayout.Column("pumice_column", "pumice_worked")));

    /// <summary>
    ///     A column, made out of obsidian.
    ///     Columns serve both as decoration and as a structural element.
    /// </summary>
    public Block ObsidianColumn { get; } = registry.Register(new ConstructionBlock(
        Language.ObsidianColumn,
        nameof(ObsidianColumn),
        TextureLayout.Column("obsidian_column", "obsidian_worked")));

    /// <summary>
    ///     Lignite is a type of coal.
    ///     It is the lowest rank of coal but can be found near the surface.
    /// </summary>
    public Block Lignite { get; } = registry.Register(new BasicBlock(
        Language.CoalLignite,
        nameof(Lignite),
        BlockFlags.Basic,
        TextureLayout.Uniform("coal_lignite")));

    /// <summary>
    ///     Bituminous coal is a type of coal.
    ///     It is of medium rank and is the most abundant type of coal.
    /// </summary>
    public Block BituminousCoal { get; } = registry.Register(new BasicBlock(
        Language.CoalBituminous,
        nameof(BituminousCoal),
        BlockFlags.Basic,
        TextureLayout.Uniform("coal_bituminous")));

    /// <summary>
    ///     Anthracite is a type of coal.
    ///     It is the highest rank of coal and is the hardest and most carbon-rich.
    /// </summary>
    public Block Anthracite { get; } = registry.Register(new BasicBlock(
        Language.CoalAnthracite,
        nameof(Anthracite),
        BlockFlags.Basic,
        TextureLayout.Uniform("coal_anthracite")));

    /// <summary>
    ///     Magnetite is a type of iron ore.
    /// </summary>
    public Block Magnetite { get; } = registry.Register(new BasicBlock(
        Language.OreMagnetite,
        nameof(Magnetite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_iron_magnetite")));

    /// <summary>
    ///     Hematite is a type of iron ore.
    /// </summary>
    public Block Hematite { get; } = registry.Register(new BasicBlock(
        Language.OreHematite,
        nameof(Hematite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_iron_hematite")));

    /// <summary>
    ///     Native gold is gold ore, containing mostly gold with some impurities.
    /// </summary>
    public Block NativeGold { get; } = registry.Register(new BasicBlock(
        Language.OreNativeGold,
        nameof(NativeGold),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_gold_native")));

    /// <summary>
    ///     Native silver is silver ore, containing mostly silver with some impurities.
    /// </summary>
    public Block NativeSilver { get; } = registry.Register(new BasicBlock(
        Language.OreNativeSilver,
        nameof(NativeSilver),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_silver_native")));

    /// <summary>
    ///     Native platinum is platinum ore, containing mostly platinum with some impurities.
    /// </summary>
    public Block NativePlatinum { get; } = registry.Register(new BasicBlock(
        Language.OreNativePlatinum,
        nameof(NativePlatinum),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_platinum_native")));

    /// <summary>
    ///     Native copper is copper ore, containing mostly copper with some impurities.
    /// </summary>
    public Block NativeCopper { get; } = registry.Register(new BasicBlock(
        Language.OreNativeCopper,
        nameof(NativeCopper),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_copper_native")));

    /// <summary>
    ///     Chalcopyrite is a copper ore.
    ///     It is the most abundant copper ore but is not as rich in copper as other ores.
    /// </summary>
    public Block Chalcopyrite { get; } = registry.Register(new BasicBlock(
        Language.OreChalcopyrite,
        nameof(Chalcopyrite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_copper_chalcopyrite")));

    /// <summary>
    ///     Malachite is a copper ore.
    ///     It is rich in copper, but is not as abundant as other ores.
    /// </summary>
    public Block Malachite { get; } = registry.Register(new BasicBlock(
        Language.OreMalachite,
        nameof(Malachite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_copper_malachite")));

    /// <summary>
    ///     Electrum is a naturally occurring alloy of gold and silver.
    /// </summary>
    public Block Electrum { get; } = registry.Register(new BasicBlock(
        Language.OreElectrum,
        nameof(Electrum),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_electrum_native")));

    /// <summary>
    ///     Bauxite is an aluminum ore.
    /// </summary>
    public Block Bauxite { get; } = registry.Register(new BasicBlock(
        Language.OreBauxite,
        nameof(Bauxite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_aluminium_bauxite")));

    /// <summary>
    ///     Galena is a lead ore that is rich in lead and silver.
    /// </summary>
    public Block Galena { get; } = registry.Register(new BasicBlock(
        Language.OreGalena,
        nameof(Galena),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_lead_galena")));

    /// <summary>
    ///     Cassiterite is a tin ore.
    /// </summary>
    public Block Cassiterite { get; } = registry.Register(new BasicBlock(
        Language.OreCassiterite,
        nameof(Cassiterite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_tin_cassiterite")));

    /// <summary>
    ///     Cinnabar is a mercury ore.
    /// </summary>
    public Block Cinnabar { get; } = registry.Register(new BasicBlock(
        Language.OreCinnabar,
        nameof(Cinnabar),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_mercury_cinnabar")));

    /// <summary>
    ///     Sphalerite is a zinc ore.
    /// </summary>
    public Block Sphalerite { get; } = registry.Register(new BasicBlock(
        Language.OreSphalerite,
        nameof(Sphalerite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_zinc_sphalerite")));

    /// <summary>
    ///     Chromite is a chromium ore.
    /// </summary>
    public Block Chromite { get; } = registry.Register(new BasicBlock(
        Language.OreChromite,
        nameof(Chromite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_chromium_chromite")));

    /// <summary>
    ///     Pyrolusite is a manganese ore.
    /// </summary>
    public Block Pyrolusite { get; } = registry.Register(new BasicBlock(
        Language.OrePyrolusite,
        nameof(Pyrolusite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_manganese_pyrolusite")));

    /// <summary>
    ///     Rutile is a titanium ore.
    /// </summary>
    public Block Rutile { get; } = registry.Register(new BasicBlock(
        Language.OreRutile,
        nameof(Rutile),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_titanium_rutile")));

    /// <summary>
    ///     Pentlandite is a nickel ore which is also rich in iron.
    /// </summary>
    public Block Pentlandite { get; } = registry.Register(new BasicBlock(
        Language.OrePentlandite,
        nameof(Pentlandite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_nickel_pentlandite")));

    /// <summary>
    ///     Zircon is a zirconium ore.
    /// </summary>
    public Block Zircon { get; } = registry.Register(new BasicBlock(
        Language.OreZircon,
        nameof(Zircon),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_zirconium_zircon")));

    /// <summary>
    ///     Dolomite is a carbonate rock, rich in magnesium.
    /// </summary>
    public Block Dolomite { get; } = registry.Register(new BasicBlock(
        Language.OreDolomite,
        nameof(Dolomite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_magnesium_dolomite")));

    /// <summary>
    ///     Celestine is a strontium ore.
    /// </summary>
    public Block Celestine { get; } = registry.Register(new BasicBlock(
        Language.OreCelestine,
        nameof(Celestine),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_strontium_celestine")));

    /// <summary>
    ///     Uraninite is a uranium ore.
    /// </summary>
    public Block Uraninite { get; } = registry.Register(new BasicBlock(
        Language.OreUraninite,
        nameof(Uraninite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_uranium_uraninite")));

    /// <summary>
    ///     Bismuthinite is a bismuth ore.
    /// </summary>
    public Block Bismuthinite { get; } = registry.Register(new BasicBlock(
        Language.OreBismuthinite,
        nameof(Bismuthinite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_bismuth_bismuthinite")));

    /// <summary>
    ///     Beryl is a beryllium ore.
    ///     This generic beryl is of low grade in comparison to beryls like emerald and aquamarine.
    /// </summary>
    public Block Beryl { get; } = registry.Register(new BasicBlock(
        Language.OreBeryl,
        nameof(Beryl),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_beryllium_beryl")));

    /// <summary>
    ///     Molybdenite is a molybdenum ore.
    /// </summary>
    public Block Molybdenite { get; } = registry.Register(new BasicBlock(
        Language.OreMolybdenite,
        nameof(Molybdenite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_molybdenum_molybdenite")));

    /// <summary>
    ///     Cobaltite is a cobalt ore.
    /// </summary>
    public Block Cobaltite { get; } = registry.Register(new BasicBlock(
        Language.OreCobaltite,
        nameof(Cobaltite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_cobalt_cobaltite")));

    /// <summary>
    ///     Spodumene is a lithium ore.
    /// </summary>
    public Block Spodumene { get; } = registry.Register(new BasicBlock(
        Language.OreSpodumene,
        nameof(Spodumene),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_lithium_spodumene")));

    /// <summary>
    ///     Vanadinite is a vanadium ore.
    /// </summary>
    public Block Vanadinite { get; } = registry.Register(new BasicBlock(
        Language.OreVanadinite,
        nameof(Vanadinite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_vanadium_vanadinite")));

    /// <summary>
    ///     Scheelite is a tungsten ore.
    /// </summary>
    public Block Scheelite { get; } = registry.Register(new BasicBlock(
        Language.OreScheelite,
        nameof(Scheelite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_tungsten_scheelite")));

    /// <summary>
    ///     Greenockite is a cadmium ore.
    /// </summary>
    public Block Greenockite { get; } = registry.Register(new BasicBlock(
        Language.OreGreenockite,
        nameof(Greenockite),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_cadmium_greenockite")));

    /// <summary>
    ///     When iron is exposed to oxygen and moisture, it rusts.
    ///     This blocks is a large accumulation of rust.
    /// </summary>
    public Block Rust { get; } = registry.Register(new BasicBlock(
        Language.Rust,
        nameof(Rust),
        BlockFlags.Basic,
        TextureLayout.Uniform("rust")));

    #endregion NEW BLOCKS

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Blocks>();

    [LoggerMessage(EventId = LogID.Blocks + 0, Level = LogLevel.Warning, Message = "No Block with ID {ID} could be found, returning {Air} instead")]
    private static partial void LogUnknownID(ILogger logger, UInt32 id, String air);

    [LoggerMessage(EventId = LogID.Blocks + 1, Level = LogLevel.Warning, Message = "No Block with named ID {NamedID} could be found, returning {Air} instead")]
    private static partial void LogUnknownNamedID(ILogger logger, String namedID, String air);

    #endregion LOGGING
}

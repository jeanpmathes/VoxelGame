// <copyright file="BlockManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Blocks;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

#pragma warning disable S1192 // Definition class

public partial class Block
{
    /// <summary>
    ///     The maximum amount of different blocks that can be registered.
    /// </summary>
    private const int BlockLimit = 1 << Section.DataShift;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Block>();

    private static readonly List<Block> blockList = new();
    private static readonly Dictionary<string, Block> namedBlockDictionary = new();

    /// <summary>
    ///     Gets the count of registered blocks.
    /// </summary>
    public static int Count => blockList.Count;

    /// <summary>
    ///     Translates a block ID to a reference to the block that has that ID. If the ID is not valid, air is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Block TranslateID(uint id)
    {
        if (blockList.Count > id) return blockList[(int) id];

        logger.LogWarning(
            Events.UnknownBlock,
            "No Block with ID {ID} could be found, returning {Air} instead",
            id,
            Air.NamedID);

        return Air;
    }

    /// <summary>
    ///     Translate a named ID to the block that has that ID.
    /// </summary>
    /// <param name="namedId">The named ID to translate.</param>
    /// <returns>The block, or null if no block with the ID exists.</returns>
    public static Block? TranslateNamedID(string namedId)
    {
        namedBlockDictionary.TryGetValue(namedId, out Block? block);

        return block;
    }

    /// <summary>
    ///     Calls the setup method on all blocks.
    /// </summary>
    public static void LoadBlocks(ITextureIndexProvider indexProvider)
    {
        using (logger.BeginScope("Block Loading"))
        {
            foreach (Block block in blockList)
            {
                block.Setup(indexProvider);

                logger.LogDebug(Events.BlockLoad, "Loaded block [{Block}] with ID {ID}", block, block.ID);
            }

            logger.LogInformation(Events.BlockLoad, "Block setup complete, {Count} blocks loaded", Count);
        }
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    internal static class Specials
    {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
        public static readonly ConcreteBlock Concrete = (ConcreteBlock) Block.Concrete;
        public static readonly GroundedModifiableHeightBlock Snow = (GroundedModifiableHeightBlock) Block.Snow;
        public static readonly ModifiableHeightBlock Ice = (ModifiableHeightBlock) Block.Ice;
        public static readonly RotatedBlock Log = (RotatedBlock) Block.Log;
        public static readonly FlatBlock Vines = (FlatBlock) Block.Vines;
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
    }

    #region NATURAL BLOCKS

    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public static readonly Block Air = new AirBlock(Language.Air, nameof(Air));

    /// <summary>
    ///     Dirt with some grass on top. Plants can be placed on top of this.
    ///     The grass can burn, creating ash.
    /// </summary>
    public static readonly Block Grass = new GrassBlock(
        Language.Grass,
        nameof(Grass),
        TextureLayout.UniqueColumn("grass_side", "dirt", "grass"),
        TextureLayout.UniqueColumn("grass_side_wet", "dirt_wet", "grass_wet"));

    /// <summary>
    ///     Grass that was burned. Water can burn the ash away.
    /// </summary>
    public static readonly Block GrassBurned = new CoveredGrassSpreadableBlock(
        Language.AshCoveredDirt,
        nameof(GrassBurned),
        TextureLayout.UniqueColumn("ash_side", "dirt", "ash"),
        hasNeutralTint: false);

    /// <summary>
    ///     Simple dirt. Grass next to it can spread over it.
    /// </summary>
    public static readonly Block Dirt = new DirtBlock(
        Language.Dirt,
        nameof(Dirt),
        TextureLayout.Uniform("dirt"),
        TextureLayout.Uniform("dirt_wet"));

    /// <summary>
    ///     Tilled dirt that allows many plants to grow.
    ///     While plants can also grow on normal grass, this block allows full growth.
    /// </summary>
    public static readonly Block Farmland = new InsetDirtBlock(
        Language.Farmland,
        nameof(Farmland),
        TextureLayout.UniqueTop("dirt", "farmland"),
        TextureLayout.UniqueTop("dirt_wet", "farmland_wet"),
        supportsFullGrowth: true);

    /// <summary>
    ///     A tall grassy plant. Fluids will destroy it, if the level is too high.
    /// </summary>
    public static readonly Block TallGrass = new CrossPlantBlock(
        Language.TallGrass,
        nameof(TallGrass),
        "tall_grass",
        BlockFlags.Replaceable,
        BoundingVolume.CrossBlock);

    /// <summary>
    ///     A much larger version of the normal tall grass.
    /// </summary>
    public static readonly Block VeryTallGrass = new DoubleCrossPlantBlock(
        Language.VeryTallGrass,
        nameof(VeryTallGrass),
        "very_tall_grass",
        topTexOffset: 1,
        BoundingVolume.CrossBlock);

    /// <summary>
    ///     A simple flower.
    /// </summary>
    public static readonly Block Flower = new CrossPlantBlock(
        Language.Flower,
        nameof(Flower),
        "flower",
        BlockFlags.Replaceable,
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f)));

    /// <summary>
    ///     A very tall flower.
    /// </summary>
    public static readonly Block TallFlower = new DoubleCrossPlantBlock(
        Language.TallFlower,
        nameof(TallFlower),
        "tall_flower",
        topTexOffset: 1,
        BoundingVolume.CrossBlock);

    /// <summary>
    ///     When stone is destroyed, rubble is what remains.
    /// </summary>
    public static readonly Block Rubble = new ConstructionBlock(
        Language.Rubble,
        nameof(Rubble),
        TextureLayout.Uniform("rubble"));

    /// <summary>
    ///     Mud is created when water and dirt mix.
    /// </summary>
    public static readonly Block Mud = new MudBlock(
        Language.Mud,
        nameof(Mud),
        TextureLayout.Uniform("mud"),
        maxVelocity: 0.1f);

    /// <summary>
    ///     Pumice is created when lava rapidly cools down, while being in contact with a lot of water.
    /// </summary>
    public static readonly Block Pumice = new BasicBlock(
        Language.Pumice,
        nameof(Pumice),
        BlockFlags.Basic,
        TextureLayout.Uniform("pumice"));

    /// <summary>
    ///     Obsidian is a dark type of stone, that forms from lava.
    /// </summary>
    public static readonly Block Obsidian = new BasicBlock(
        Language.Obsidian,
        nameof(Obsidian),
        BlockFlags.Basic,
        TextureLayout.Uniform("obsidian"));

    /// <summary>
    ///     Snow covers the ground, and can have different heights.
    /// </summary>
    public static readonly Block Snow = new GroundedModifiableHeightBlock(
        Language.Snow,
        nameof(Snow),
        TextureLayout.Uniform("snow"));

    /// <summary>
    ///     Leaves are transparent parts of the tree. They are flammable.
    /// </summary>
    public static readonly Block Leaves = new NaturalBlock(
        Language.Leaves,
        nameof(Leaves),
        hasNeutralTint: true,
        new BlockFlags
        {
            IsSolid = true,
            RenderFaceAtNonOpaques = true
        },
        TextureLayout.Uniform("leaves"));

    /// <summary>
    ///     Log is the unprocessed, wooden part of a tree. As it is made of wood, it is flammable.
    /// </summary>
    public static readonly Block Log = new RotatedBlock(
        Language.Log,
        nameof(Log),
        BlockFlags.Basic,
        TextureLayout.Column("log", sideOffset: 0, endOffset: 1));

    /// <summary>
    ///     Processed wood that can be used as construction material. It is flammable.
    /// </summary>
    public static readonly Block Wood = new OrganicConstructionBlock(
        Language.Wood,
        nameof(Wood),
        TextureLayout.Uniform("wood"));

    /// <summary>
    ///     Sand naturally forms and allows water to flow through it.
    /// </summary>
    public static readonly Block Sand = new PermeableBlock(
        Language.Sand,
        nameof(Sand),
        TextureLayout.Uniform("sand"));

    /// <summary>
    ///     Gravel, which is made out of small pebbles, allows water to flow through it.
    /// </summary>
    public static readonly Block Gravel = new PermeableBlock(
        Language.Gravel,
        nameof(Gravel),
        TextureLayout.Uniform("gravel"));

    /// <summary>
    ///     Coal ore is stone that contains coal.
    /// </summary>
    public static readonly Block OreCoal = new BasicBlock(
        Language.CoalOre,
        nameof(OreCoal),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_coal"));

    /// <summary>
    ///     Iron ore is stone that contains iron.
    /// </summary>
    public static readonly Block OreIron = new BasicBlock(
        Language.IronOre,
        nameof(OreIron),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_iron"));

    /// <summary>
    ///     Gold ore is stone that contains gold.
    /// </summary>
    public static readonly Block OreGold = new BasicBlock(
        Language.GoldOre,
        nameof(OreGold),
        BlockFlags.Basic,
        TextureLayout.Uniform("ore_gold"));

    /// <summary>
    ///     Ahs is the remainder of burning processes.
    /// </summary>
    public static readonly Block Ash = new BasicBlock(
        Language.Ash,
        nameof(Ash),
        BlockFlags.Basic,
        TextureLayout.Uniform("ash"));

    #endregion NATURAL BLOCKS

    #region PLANT BLOCKS

    /// <summary>
    ///     A cactus slowly grows upwards. It can only be placed on sand.
    /// </summary>
    public static readonly Block Cactus = new GrowingBlock(
        Language.Cactus,
        nameof(Cactus),
        TextureLayout.Column("cactus", sideOffset: 0, endOffset: 1),
        Sand,
        maxHeight: 4);

    /// <summary>
    ///     Pumpkins are the fruit of the pumpkin plant. They have to be placed on solid ground.
    /// </summary>
    public static readonly Block Pumpkin = new GroundedBlock(
        Language.Pumpkin,
        nameof(Pumpkin),
        BlockFlags.Basic,
        TextureLayout.Column("pumpkin_side", "pumpkin_top"));

    /// <summary>
    ///     Melons are the fruit of the melon plant. They have to be placed on solid ground.
    /// </summary>
    public static readonly Block Melon = new GroundedBlock(
        Language.Melon,
        nameof(Melon),
        BlockFlags.Basic,
        TextureLayout.Column("melon_side", "melon_top"));

    /// <summary>
    ///     Spiderwebs slow the movement of entities and can be used to trap enemies.
    /// </summary>
    public static readonly Block Spiderweb = new SpiderWebBlock(
        Language.SpiderWeb,
        nameof(Spiderweb),
        "spider_web",
        maxVelocity: 0.01f);

    /// <summary>
    ///     Vines grow downwards, and can hang freely. It is possible to climb them.
    /// </summary>
    public static readonly Block Vines = new GrowingFlatBlock(
        Language.Vines,
        nameof(Vines),
        "vines",
        climbingVelocity: 2f,
        slidingVelocity: 1f);

    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public static readonly Block Flax = new CropBlock(
        Language.Flax,
        nameof(Flax),
        "flax",
        second: 0,
        third: 1,
        fourth: 2,
        fifth: 3,
        sixth: 3,
        final: 4,
        dead: 5);

    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public static readonly Block Potatoes = new CropBlock(
        Language.Potatoes,
        nameof(Potatoes),
        "potato",
        second: 1,
        third: 1,
        fourth: 2,
        fifth: 2,
        sixth: 3,
        final: 4,
        dead: 5);

    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They requires water to fully grow.
    /// </summary>
    public static readonly Block Onions = new CropBlock(
        Language.Onions,
        nameof(Onions),
        "onion",
        second: 0,
        third: 1,
        fourth: 1,
        fifth: 2,
        sixth: 2,
        final: 3,
        dead: 4);

    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public static readonly Block Wheat = new CropBlock(
        Language.Wheat,
        nameof(Wheat),
        "wheat",
        second: 0,
        third: 1,
        fourth: 1,
        fifth: 2,
        sixth: 2,
        final: 3,
        dead: 4);

    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public static readonly Block Maize = new DoubleCropBlock(
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
        (5, 8));

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public static readonly Block PumpkinPlant = new FruitCropBlock(
        Language.PumpkinPlant,
        nameof(PumpkinPlant),
        "pumpkin_plant",
        Pumpkin);

    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public static readonly Block MelonPlant = new FruitCropBlock(
        Language.MelonPlant,
        nameof(MelonPlant),
        "melon_plant",
        Melon);

    #endregion PLANT BLOCKS

    #region BUILDING BLOCKS

    /// <summary>
    ///     Glass is transparent block.
    /// </summary>
    public static readonly Block Glass = new GlassBlock(
        Language.Glass,
        nameof(Glass),
        TextureLayout.Uniform("glass"));

    /// <summary>
    ///     Tiled glass is like glass, but made out of four tiles.
    /// </summary>
    public static readonly Block GlassTiled = new GlassBlock(
        Language.TiledGlass,
        nameof(GlassTiled),
        TextureLayout.Uniform("glass_tiled"));

    /// <summary>
    ///     The steel block is a metal construction block.
    /// </summary>
    public static readonly Block Steel = new ConstructionBlock(
        Language.Steel,
        nameof(Steel),
        TextureLayout.Uniform("steel"));

    /// <summary>
    ///     Worked stone is a processed stone block.
    /// </summary>
    public static readonly Block StoneWorked = new BasicBlock(
        Language.WorkedStone,
        nameof(StoneWorked),
        BlockFlags.Basic,
        TextureLayout.Uniform("stone_worked"));

    /// <summary>
    ///     A ladder allows climbing up and down.
    /// </summary>
    public static readonly Block Ladder = new FlatBlock(
        Language.Ladder,
        nameof(Ladder),
        "ladder",
        climbingVelocity: 3f,
        slidingVelocity: 1f);

    /// <summary>
    ///     Small tiles for construction of floors and walls.
    /// </summary>
    public static readonly Block TilesSmall = new ConstructionBlock(
        Language.SmallTiles,
        nameof(TilesSmall),
        TextureLayout.Uniform("small_tiles"));

    /// <summary>
    ///     Large tiles for construction of floors and walls.
    /// </summary>
    public static readonly Block TilesLarge = new ConstructionBlock(
        Language.LargeTiles,
        nameof(TilesLarge),
        TextureLayout.Uniform("large_tiles"));

    /// <summary>
    ///     Black checkerboard tiles come in different colors.
    /// </summary>
    public static readonly Block TilesCheckerboardBlack = new TintedBlock(
        Language.CheckerboardTilesBlack,
        nameof(TilesCheckerboardBlack),
        BlockFlags.Basic,
        TextureLayout.Uniform("checkerboard_tiles_black"));

    /// <summary>
    ///     White checkerboard tiles come in different colors.
    /// </summary>
    public static readonly Block TilesCheckerboardWhite = new TintedBlock(
        Language.CheckerboardTilesWhite,
        nameof(TilesCheckerboardWhite),
        BlockFlags.Basic,
        TextureLayout.Uniform("checkerboard_tiles_white"));

    /// <summary>
    ///     Bricks are a simple construction material.
    /// </summary>
    public static readonly Block Bricks = new ConstructionBlock(
        Language.Bricks,
        nameof(Bricks),
        TextureLayout.Uniform("bricks"));

    /// <summary>
    ///     Paving stone is a simple construction material, ideal for paths.
    /// </summary>
    public static readonly Block PavingStone = new ConstructionBlock(
        Language.PavingStone,
        nameof(PavingStone),
        TextureLayout.Uniform("paving_stone"));

    /// <summary>
    ///     Red plastic is a construction material.
    /// </summary>
    public static readonly Block RedPlastic = new ConstructionBlock(
        Language.RedPlastic,
        nameof(RedPlastic),
        TextureLayout.Uniform("red_plastic"));

    /// <summary>
    ///     Concrete is a flexible construction material that can have different heights and colors.
    ///     It can be build using fluid concrete.
    /// </summary>
    public static readonly Block Concrete = new ConcreteBlock(
        Language.Concrete,
        nameof(Concrete),
        TextureLayout.Uniform("concrete"));

    #endregion BUILDING BLOCKS

    #region DECORATION BLOCKS

    /// <summary>
    ///     This block is like a processed stone block, but with a decorative face added.
    /// </summary>
    public static readonly Block StoneFace = new OrientedBlock(
        Language.StoneFace,
        nameof(StoneFace),
        BlockFlags.Basic,
        TextureLayout.UniqueFront("stone_worked_face", "stone_worked"));

    /// <summary>
    ///     The vase is a decorative block that must be placed on solid ground.
    /// </summary>
    public static readonly Block Vase = new CustomModelBlock(
        Language.Vase,
        nameof(Vase),
        BlockFlags.Solid,
        "vase",
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3d(x: 0.25f, y: 0.375f, z: 0.25f)));

    /// <summary>
    ///     The bed can be placed to set a different spawn point.
    ///     It is possible to change to color of a bed.
    /// </summary>
    public static readonly Block Bed = new BedBlock(Language.Bed, nameof(Bed), "bed");

    /// <summary>
    ///     Wool is a flammable material, that allows its color to be changed.
    /// </summary>
    public static readonly Block Wool = new OrganicTintedBlock(
        Language.Wool,
        nameof(Wool),
        TextureLayout.Uniform("wool"));

    /// <summary>
    ///     Decorated wool is similar to wool, decorated with golden ornaments.
    /// </summary>
    public static readonly Block WoolDecorated = new OrganicTintedBlock(
        Language.DecoratedWool,
        nameof(WoolDecorated),
        TextureLayout.Uniform("wool_decorated"));

    /// <summary>
    ///     Carpets can be used to cover the floor. Their color can be changed.
    /// </summary>
    public static readonly Block Carpet = new TintedCustomModelBlock(
        Language.Carpet,
        nameof(Carpet),
        BlockFlags.Solid,
        "carpet",
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)));

    /// <summary>
    ///     Decorated carpets are similar to carpets, decorated with golden ornaments.
    /// </summary>
    public static readonly Block CarpetDecorated = new TintedCustomModelBlock(
        Language.DecoratedCarpet,
        nameof(CarpetDecorated),
        BlockFlags.Solid,
        "carpet_decorated",
        new BoundingVolume(new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3d(x: 0.5f, y: 0.03125f, z: 0.5f)));

    /// <summary>
    ///     Glass panes are a thin alternative to glass blocks.
    ///     They connect to some neighboring blocks.
    /// </summary>
    public static readonly Block GlassPane = new ThinConnectingBlock(
        Language.GlassPane,
        nameof(GlassPane),
        "pane_glass_post",
        "pane_glass_side",
        "pane_glass_extension");

    /// <summary>
    ///     Steel bars are a thin, but strong barrier.
    /// </summary>
    public static readonly Block Bars = new ThinConnectingBlock(
        Language.Bars,
        nameof(Bars),
        "bars_post",
        "bars_side",
        "bars_extension");

    #endregion DECORATION BLOCKS

    #region ACCESS BLOCKS

    /// <summary>
    ///     The wooden fence can be used as way of marking areas. It does not prevent jumping over it.
    ///     As this fence is made out of wood, it is flammable. Fences can connect to other blocks.
    /// </summary>
    public static readonly Block FenceWood = new FenceBlock(
        Language.WoodenFence,
        nameof(FenceWood),
        "wood",
        "fence_post",
        "fence_extension");

    /// <summary>
    ///     The rubble wall is a stone barrier that can be used as a way of marking areas.
    ///     They do not prevent jumping over it, and can connect to other blocks.
    /// </summary>
    public static readonly Block WallRubble = new WallBlock(
        Language.RubbleWall,
        nameof(WallRubble),
        "rubble",
        "wall_post",
        "wall_extension",
        "wall_extension_straight");

    /// <summary>
    ///     The brick wall is similar to all other walls, and made out of bricks.
    ///     They do not prevent jumping over them, and can connect to other blocks.
    /// </summary>
    public static readonly Block WallBricks = new WallBlock(
        Language.BrickWall,
        nameof(WallBricks),
        "bricks",
        "wall_post",
        "wall_extension",
        "wall_extension_straight");

    /// <summary>
    ///     The steel door allows closing of a room. It can be opened and closed.
    /// </summary>
    public static readonly Block DoorSteel = new DoorBlock(
        Language.SteelDoor,
        nameof(DoorSteel),
        "door_steel_closed",
        "door_steel_open");

    /// <summary>
    ///     The wooden door allows closing of a room. It can be opened and closed.
    ///     As this door is made out of wood, it is flammable.
    /// </summary>
    public static readonly Block DoorWood = new OrganicDoorBlock(
        Language.WoodenDoor,
        nameof(DoorWood),
        "door_wood_closed",
        "door_wood_open");

    /// <summary>
    ///     Fence gates are meant as a passage trough fences and walls.
    /// </summary>
    public static readonly Block GateWood = new GateBlock(
        Language.WoodenGate,
        nameof(GateWood),
        "gate_wood_closed",
        "gate_wood_open");

    #endregion ACCESS BLOCKS

    #region LIQUID FLOW BLOCKS

    /// <summary>
    ///     The fluid barrier can be used to control fluid flow. It can be opened and closed.
    ///     It does not prevent gasses from flowing through it.
    /// </summary>
    public static readonly Block FluidBarrier = new FluidBarrierBlock(
        Language.Barrier,
        nameof(FluidBarrier),
        TextureLayout.Uniform("fluid_barrier_closed"),
        TextureLayout.Uniform("fluid_barrier_open"));

    /// <summary>
    ///     The industrial steel pipe can be used to control fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public static readonly Block SteelPipe = new PipeBlock<IIndustrialPipeConnectable>(
        Language.SteelPipe,
        nameof(SteelPipe),
        diameter: 0.375f,
        "steel_pipe_center",
        "steel_pipe_connector",
        "steel_pipe_surface");

    /// <summary>
    ///     The wooden pipe offers a primitive way of controlling fluid flow.
    ///     It connects to other pipes.
    /// </summary>
    public static readonly Block WoodenPipe = new PipeBlock<IPrimitivePipeConnectable>(
        Language.WoodenPipe,
        nameof(WoodenPipe),
        diameter: 0.3125f,
        "wood_pipe_center",
        "wood_pipe_connector",
        "wood_pipe_surface");

    /// <summary>
    ///     This pipe is a special steel pipe that can only form straight connections.
    ///     It is ideal for parallel pipes.
    /// </summary>
    public static readonly Block StraightSteelPipe = new StraightSteelPipeBlock(
        Language.SteelPipeStraight,
        nameof(StraightSteelPipe),
        diameter: 0.375f,
        "steel_pipe_straight");

    /// <summary>
    ///     This is a special steel pipe that can be closed. It prevents all fluid flow.
    /// </summary>
    public static readonly Block PipeValve = new SteelPipeValveBlock(
        Language.ValvePipe,
        nameof(PipeValve),
        diameter: 0.375f,
        "steel_pipe_valve_open",
        "steel_pipe_valve_closed");

    /// <summary>
    ///     The pump can lift fluids up when interacted with.
    ///     It can only lift up to a threshold of 16 blocks.
    /// </summary>
    public static readonly Block Pump = new PumpBlock(
        Language.Pump,
        nameof(Pump),
        pumpDistance: 16,
        TextureLayout.Uniform("pump"));

    #endregion LIQUID FLOW BLOCKS

    #region SPECIAL BLOCKS

    /// <summary>
    ///     Fire is a dangerous block that spreads onto nearby flammable blocks.
    ///     When spreading, fire burns blocks which can destroy them.
    /// </summary>
    public static readonly Block Fire = new FireBlock(
        Language.Fire,
        nameof(Fire),
        "fire_complete",
        "fire_side",
        "fire_top");

    /// <summary>
    ///     This is a magical pulsating block.
    /// </summary>
    public static readonly Block Pulsating = new TintedBlock(
        Language.PulsatingBlock,
        nameof(Pulsating),
        BlockFlags.Basic,
        TextureLayout.Uniform("pulsating"),
        isAnimated: true);

    /// <summary>
    ///     The eternal flame, once lit, will never go out naturally.
    /// </summary>
    public static readonly Block EternalFlame = new EternalFlame(
        Language.EternalFlame,
        nameof(EternalFlame),
        TextureLayout.Uniform("eternal_flame"));

    /// <summary>
    ///     The path is a dirt block with its top layer trampled.
    /// </summary>
    public static readonly Block Path = new InsetDirtBlock(
        Language.Path,
        nameof(Path),
        TextureLayout.Uniform("dirt"),
        TextureLayout.Uniform("dirt_wet"),
        supportsFullGrowth: false);

    #endregion SPECIAL BLOCKS

    #region NEW BLOCKS

    /// <summary>
    ///     Granite is found next to volcanic activity.
    /// </summary>
    public static readonly Block Granite = new BasicBlock(
        Language.Granite,
        nameof(Granite),
        BlockFlags.Basic,
        TextureLayout.Uniform("granite"));

    /// <summary>
    ///     Sandstone is found all over the world and especially in the desert.
    /// </summary>
    public static readonly Block Sandstone = new BasicBlock(
        Language.Sandstone,
        nameof(Sandstone),
        BlockFlags.Basic,
        TextureLayout.Uniform("sandstone"));

    /// <summary>
    ///     Limestone is found all over the world and especially in oceans.
    /// </summary>
    public static readonly Block Limestone = new BasicBlock(
        Language.Limestone,
        nameof(Limestone),
        BlockFlags.Basic,
        TextureLayout.Uniform("limestone"));

    /// <summary>
    ///     Marble is a rarer stone type.
    /// </summary>
    public static readonly Block Marble = new BasicBlock(
        Language.Marble,
        nameof(Marble),
        BlockFlags.Basic,
        TextureLayout.Uniform("marble"));

    /// <summary>
    ///     Clay is found beneath the ground and blocks ground water flow.
    /// </summary>
    public static readonly Block Clay = new BasicBlock(
        Language.Clay,
        nameof(Clay),
        BlockFlags.Basic,
        TextureLayout.Uniform("clay"));

    /// <summary>
    ///     Permafrost is a type of soil that is frozen solid.
    /// </summary>
    public static readonly Block Permafrost = new BasicBlock(
        Language.Permafrost,
        nameof(Permafrost),
        BlockFlags.Basic,
        TextureLayout.Uniform("permafrost"));

    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public static readonly Block Core = new BasicBlock(
        Language.Core,
        nameof(Core),
        BlockFlags.Basic,
        TextureLayout.Uniform("core"));

    /// <summary>
    ///     A block made out of frozen water.
    /// </summary>
    public static readonly Block Ice = new ModifiableHeightBlock(
        Language.Ice,
        nameof(Ice),
        TextureLayout.Uniform("ice"));

    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public static readonly Block Error = new BasicBlock(
        Language.Error,
        nameof(Error),
        BlockFlags.Basic,
        TextureLayout.Uniform("missing_texture"));

    /// <summary>
    ///     Roots grow at the bottom of trees.
    /// </summary>
    public static readonly Block Roots = new PermeableNaturalBlock(
        Language.Roots,
        nameof(Roots),
        hasNeutralTint: false,
        BlockFlags.Basic,
        TextureLayout.Uniform("roots"));

    #endregion NEW BLOCKS
}

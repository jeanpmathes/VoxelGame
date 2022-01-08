// <copyright file="BlockManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Blocks;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Block : IBlockBase
    {
        /// <summary>
        ///     The maximum amount of different blocks that can be registered.
        /// </summary>
        public const int BlockLimit = 1 << Section.DataShift;

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
        public static Block TranslateID(uint id)
        {
            if (blockList.Count > id) return blockList[(int) id];

            logger.LogWarning(
                Events.UnknownBlock,
                "No Block with ID {ID} could be found, returning {Air} instead",
                id,
                Air.NamedId);

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

                    logger.LogDebug(Events.BlockLoad, "Loaded block [{Block}] with ID {ID}", block, block.Id);
                }

                logger.LogInformation(Events.BlockLoad, "Block setup complete, {Count} blocks loaded", Count);
            }
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal static class Specials
        {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
            public static readonly ConcreteBlock Concrete = (ConcreteBlock) Block.Concrete;
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
        }

        #region NATURAL BLOCKS

        /// <summary>
        ///     The air block that fills the world. Could also be interpreted as "no block".
        /// </summary>
        public static readonly Block Air = new AirBlock(Language.Air, nameof(Air));

        /// <summary>
        ///     Dirt with some grass on top.
        /// </summary>
        public static readonly Block Grass = new GrassBlock(
            Language.Grass,
            nameof(Grass),
            TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"),
            TextureLayout.UnqiueColumn("grass_side_wet", "dirt_wet", "grass_wet"));

        /// <summary>
        ///     Grass that was burned.
        /// </summary>
        public static readonly Block GrassBurned = new CoveredGrassSpreadableBlock(
            Language.AshCoveredDirt,
            nameof(GrassBurned),
            TextureLayout.UnqiueColumn("ash_side", "dirt", "ash"),
            hasNeutralTint: false);

        /// <summary>
        ///     Simple dirt block.
        /// </summary>
        public static readonly Block Dirt = new DirtBlock(
            Language.Dirt,
            nameof(Dirt),
            TextureLayout.Uniform("dirt"),
            TextureLayout.Uniform("dirt_wet"));

        /// <summary>
        ///     Tilled dirt that allows many plants to grow.
        /// </summary>
        public static readonly Block Farmland = new InsetDirtBlock(
            Language.Farmland,
            nameof(Farmland),
            TextureLayout.UnqiueTop("dirt", "farmland"),
            TextureLayout.UnqiueTop("dirt_wet", "farmland_wet"),
            supportsFullGrowth: true);

        /// <summary>
        ///     Tall grass.
        /// </summary>
        public static readonly Block TallGrass = new CrossPlantBlock(
            Language.TallGrass,
            nameof(TallGrass),
            "tall_grass",
            BlockFlags.Replaceable,
            BoundingBox.CrossBlock);

        /// <summary>
        ///     Very tall grass.
        /// </summary>
        public static readonly Block VeryTallGrass = new DoubleCrossPlantBlock(
            Language.VeryTallGrass,
            nameof(VeryTallGrass),
            "very_tall_grass",
            topTexOffset: 1,
            BoundingBox.CrossBlock);

        /// <summary>
        ///     A flower.
        /// </summary>
        public static readonly Block Flower = new CrossPlantBlock(
            Language.Flower,
            nameof(Flower),
            "flower",
            BlockFlags.Replaceable,
            new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.175f, y: 0.5f, z: 0.175f)));

        /// <summary>
        ///     A very tall flower.
        /// </summary>
        public static readonly Block TallFlower = new DoubleCrossPlantBlock(
            Language.TallFlower,
            nameof(TallFlower),
            "tall_flower",
            topTexOffset: 1,
            BoundingBox.CrossBlock);

        /// <summary>
        ///     Stone, that makes up most of the world.
        /// </summary>
        public static readonly Block Stone = new BasicBlock(
            Language.Stone,
            nameof(Stone),
            BlockFlags.Basic,
            TextureLayout.Uniform("stone"));

        /// <summary>
        ///     Broken stone.
        /// </summary>
        public static readonly Block Rubble = new ConstructionBlock(
            Language.Rubble,
            nameof(Rubble),
            TextureLayout.Uniform("rubble"));

        /// <summary>
        ///     A mix of dirt and water.
        /// </summary>
        public static readonly Block Mud = new MudBlock(
            Language.Mud,
            nameof(Mud),
            TextureLayout.Uniform("mud"),
            maxVelocity: 0.1f);

        /// <summary>
        ///     A type of stone that forms from lava that comes in contact with water.
        /// </summary>
        public static readonly Block Pumice = new BasicBlock(
            Language.Pumice,
            nameof(Pumice),
            BlockFlags.Basic,
            TextureLayout.Uniform("pumice"));

        /// <summary>
        ///     A black type of stone.
        /// </summary>
        public static readonly Block Obsidian = new BasicBlock(
            Language.Obsidian,
            nameof(Obsidian),
            BlockFlags.Basic,
            TextureLayout.Uniform("obsidian"));

        /// <summary>
        ///     Snow.
        /// </summary>
        public static readonly Block Snow = new ModifiableHeightBlock(
            Language.Snow,
            nameof(Snow),
            TextureLayout.Uniform("snow"));

        /// <summary>
        ///     Leaves, a part of trees.
        /// </summary>
        public static readonly Block Leaves = new NaturalBlock(
            Language.Leaves,
            nameof(Leaves),
            new BlockFlags
            {
                IsSolid = true,
                RenderFaceAtNonOpaques = true
            },
            TextureLayout.Uniform("leaves"));

        /// <summary>
        ///     Wood as part of a log.
        /// </summary>
        public static readonly Block Log = new RotatedBlock(
            Language.Log,
            nameof(Log),
            BlockFlags.Basic,
            TextureLayout.Column("log", sideOffset: 0, endOffset: 1));

        /// <summary>
        ///     Raw wood, extracted from logs.
        /// </summary>
        public static readonly Block Wood = new OrganicConstructionBlock(
            Language.Wood,
            nameof(Wood),
            TextureLayout.Uniform("wood"));

        /// <summary>
        ///     Sand. Allows water to flow through it.
        /// </summary>
        public static readonly Block Sand = new PermeableBlock(
            Language.Sand,
            nameof(Sand),
            TextureLayout.Uniform("sand"));

        /// <summary>
        ///     Gravel. Allows water to flow through it.
        /// </summary>
        public static readonly Block Gravel = new PermeableBlock(
            Language.Gravel,
            nameof(Gravel),
            TextureLayout.Uniform("gravel"));

        /// <summary>
        ///     Coal ore.
        /// </summary>
        public static readonly Block OreCoal = new BasicBlock(
            Language.CoalOre,
            nameof(OreCoal),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_coal"));

        /// <summary>
        ///     Iron ore.
        /// </summary>
        public static readonly Block OreIron = new BasicBlock(
            Language.IronOre,
            nameof(OreIron),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_iron"));

        /// <summary>
        ///     Gold ore.
        /// </summary>
        public static readonly Block OreGold = new BasicBlock(
            Language.GoldOre,
            nameof(OreGold),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_gold"));

        /// <summary>
        ///     Ash.
        /// </summary>
        public static readonly Block Ash = new BasicBlock(
            Language.Ash,
            nameof(Ash),
            BlockFlags.Basic,
            TextureLayout.Uniform("ash"));

        #endregion NATURAL BLOCKS

        #region PLANT BLOCKS

        /// <summary>
        ///     A slow growing cactus.
        /// </summary>
        public static readonly Block Cactus = new GrowingBlock(
            Language.Cactus,
            nameof(Cactus),
            TextureLayout.Column("cactus", sideOffset: 0, endOffset: 1),
            Sand,
            maxHeight: 4);

        /// <summary>
        ///     The pumpkin plant fruit.
        /// </summary>
        public static readonly Block Pumpkin = new GroundedBlock(
            Language.Pumpkin,
            nameof(Pumpkin),
            BlockFlags.Basic,
            TextureLayout.Column("pumpkin_side", "pumpkin_top"));

        /// <summary>
        ///     The melon plant fruit.
        /// </summary>
        public static readonly Block Melon = new GroundedBlock(
            Language.Melon,
            nameof(Melon),
            BlockFlags.Basic,
            TextureLayout.Column("melon_side", "melon_top"));

        /// <summary>
        ///     A sticky web that hinders movement.
        /// </summary>
        public static readonly Block Spiderweb = new SpiderWebBlock(
            Language.SpiderWeb,
            nameof(Spiderweb),
            "spider_web",
            maxVelocity: 0.01f);

        /// <summary>
        ///     Vines, that grow and allow climbing.
        /// </summary>
        public static readonly Block Vines = new GrowingFlatBlock(
            Language.Vines,
            nameof(Vines),
            "vines",
            climbingVelocity: 2f,
            slidingVelocity: 1f);

        /// <summary>
        ///     Flax.
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
        ///     Potatoes.
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
        ///     Onions.
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
        ///     Wheat.
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
        ///     Maize.
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
        ///     Pumpkin plant.
        /// </summary>
        public static readonly Block PumpkinPlant = new FruitCropBlock(
            Language.PumpkinPlant,
            nameof(PumpkinPlant),
            "pumpkin_plant",
            Pumpkin);

        /// <summary>
        ///     Melon plant.
        /// </summary>
        public static readonly Block MelonPlant = new FruitCropBlock(
            Language.MelonPlant,
            nameof(MelonPlant),
            "melon_plant",
            Melon);

        #endregion PLANT BLOCKS

        #region BUILDING BLOCKS

        /// <summary>
        ///     Glass, a see-through block.
        /// </summary>
        public static readonly Block Glass = new GlassBlock(
            Language.Glass,
            nameof(Glass),
            TextureLayout.Uniform("glass"));

        /// <summary>
        ///     Tiled glass, a see-through block.
        /// </summary>
        public static readonly Block GlassTiled = new GlassBlock(
            Language.TiledGlass,
            nameof(GlassTiled),
            TextureLayout.Uniform("glass_tiled"));

        /// <summary>
        ///     Steel block.
        /// </summary>
        public static readonly Block Steel = new ConstructionBlock(
            Language.Steel,
            nameof(Steel),
            TextureLayout.Uniform("steel"));

        /// <summary>
        ///     Worked stone.
        /// </summary>
        public static readonly Block StoneWorked = new BasicBlock(
            Language.WorkedStone,
            nameof(StoneWorked),
            BlockFlags.Basic,
            TextureLayout.Uniform("stone_worked"));

        /// <summary>
        ///     Ladder, a block that can be climbed.
        /// </summary>
        public static readonly Block Ladder = new FlatBlock(
            Language.Ladder,
            nameof(Ladder),
            "ladder",
            climbingVelocity: 3f,
            slidingVelocity: 1f);

        /// <summary>
        ///     Small tiles.
        /// </summary>
        public static readonly Block TilesSmall = new ConstructionBlock(
            Language.SmallTiles,
            nameof(TilesSmall),
            TextureLayout.Uniform("small_tiles"));

        /// <summary>
        ///     Large tiles.
        /// </summary>
        public static readonly Block TilesLarge = new ConstructionBlock(
            Language.LargeTiles,
            nameof(TilesLarge),
            TextureLayout.Uniform("large_tiles"));

        /// <summary>
        ///     Black checkerboard tiles, their color can be changed.
        /// </summary>
        public static readonly Block TilesCheckerboardBlack = new TintedBlock(
            Language.CheckerboardTilesBlack,
            nameof(TilesCheckerboardBlack),
            BlockFlags.Basic,
            TextureLayout.Uniform("checkerboard_tiles_black"));

        /// <summary>
        ///     White checkerboard tiles, their color can be changed.
        /// </summary>
        public static readonly Block TilesCheckerboardWhite = new TintedBlock(
            Language.CheckerboardTilesWhite,
            nameof(TilesCheckerboardWhite),
            BlockFlags.Basic,
            TextureLayout.Uniform("checkerboard_tiles_white"));

        /// <summary>
        ///     Simple bricks.
        /// </summary>
        public static readonly Block Bricks = new ConstructionBlock(
            Language.Bricks,
            nameof(Bricks),
            TextureLayout.Uniform("bricks"));

        /// <summary>
        ///     Paving stone.
        /// </summary>
        public static readonly Block PavingStone = new ConstructionBlock(
            Language.PavingStone,
            nameof(PavingStone),
            TextureLayout.Uniform("paving_stone"));

        /// <summary>
        ///     Red plastic.
        /// </summary>
        public static readonly Block RedPlastic = new ConstructionBlock(
            Language.RedPlastic,
            nameof(RedPlastic),
            TextureLayout.Uniform("red_plastic"));

        /// <summary>
        ///     Concrete. The color can be changed.
        /// </summary>
        public static readonly Block Concrete = new ConcreteBlock(
            Language.Concrete,
            nameof(Concrete),
            TextureLayout.Uniform("concrete"));

        #endregion BUILDING BLOCKS

        #region DECORATION BLOCKS

        /// <summary>
        ///     A face in stone.
        /// </summary>
        public static readonly Block StoneFace = new OrientedBlock(
            Language.StoneFace,
            nameof(StoneFace),
            BlockFlags.Basic,
            TextureLayout.UnqiueFront("stone_worked_face", "stone_worked"));

        /// <summary>
        ///     A vase.
        /// </summary>
        public static readonly Block Vase = new CustomModelBlock(
            Language.Vase,
            nameof(Vase),
            BlockFlags.Solid,
            "vase",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3(x: 0.25f, y: 0.375f, z: 0.25f)));

        /// <summary>
        ///     A bed. Placing it sets the spawn point.
        /// </summary>
        public static readonly Block Bed = new BedBlock(Language.Bed, nameof(Bed), "bed");

        /// <summary>
        ///     Wool. The color can be changed.
        /// </summary>
        public static readonly Block Wool = new OrganicTintedBlock(
            Language.Wool,
            nameof(Wool),
            TextureLayout.Uniform("wool"));

        /// <summary>
        ///     Wool with decorations. The color can be changed.
        /// </summary>
        public static readonly Block WoolDecorated = new OrganicTintedBlock(
            Language.DecoratedWool,
            nameof(WoolDecorated),
            TextureLayout.Uniform("wool_decorated"));

        /// <summary>
        ///     Carper. The color can be changed.
        /// </summary>
        public static readonly Block Carpet = new TintedCustomModelBlock(
            Language.Carpet,
            nameof(Carpet),
            BlockFlags.Solid,
            "carpet",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f)));

        /// <summary>
        ///     Decorated carpet, the color can be changed.
        /// </summary>
        public static readonly Block CarpetDecorated = new TintedCustomModelBlock(
            Language.DecoratedCarpet,
            nameof(CarpetDecorated),
            BlockFlags.Solid,
            "carpet_decorated",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f)));

        /// <summary>
        ///     A thin glass panel.
        /// </summary>
        public static readonly Block GlassPane = new ThinConnectingBlock(
            Language.GlassPane,
            nameof(GlassPane),
            "pane_glass_post",
            "pane_glass_side",
            "pane_glass_extension");

        /// <summary>
        ///     Steel bars.
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
        ///     A wooden fence.
        /// </summary>
        public static readonly Block FenceWood = new FenceBlock(
            Language.WoodenFence,
            nameof(FenceWood),
            "wood",
            "fence_post",
            "fence_extension");

        /// <summary>
        ///     A wall made out of Rubble.
        /// </summary>
        public static readonly Block WallRubble = new WallBlock(
            Language.RubbleWall,
            nameof(WallRubble),
            "rubble",
            "wall_post",
            "wall_extension",
            "wall_extension_straight");

        /// <summary>
        ///     A wall made out of brick.
        /// </summary>
        public static readonly Block WallBricks = new WallBlock(
            Language.BrickWall,
            nameof(WallBricks),
            "bricks",
            "wall_post",
            "wall_extension",
            "wall_extension_straight");

        /// <summary>
        ///     A door out of steel.
        /// </summary>
        public static readonly Block DoorSteel = new DoorBlock(
            Language.SteelDoor,
            nameof(DoorSteel),
            "door_steel_closed",
            "door_steel_open");

        /// <summary>
        ///     A door out of wood.
        /// </summary>
        public static readonly Block DoorWood = new OrganicDoorBlock(
            Language.WoodenDoor,
            nameof(DoorWood),
            "door_wood_closed",
            "door_wood_open");

        /// <summary>
        ///     A fence gate.
        /// </summary>
        public static readonly Block GateWood = new GateBlock(
            Language.WoodenGate,
            nameof(GateWood),
            "gate_wood_closed",
            "gate_wood_open");

        #endregion ACCESS BLOCKS

        #region LIQUID FLOW BLOCKS

        /// <summary>
        ///     A barrier to control flow of liquids.
        /// </summary>
        public static readonly Block LiquidBarrier = new LiquidBarrierBlock(
            Language.Barrier,
            nameof(LiquidBarrier),
            TextureLayout.Uniform("liquid_barrier_closed"),
            TextureLayout.Uniform("liquid_barrier_open"));

        /// <summary>
        ///     An industrial steel pipe.
        /// </summary>
        public static readonly Block SteelPipe = new PipeBlock<IIndustrialPipeConnectable>(
            Language.SteelPipe,
            nameof(SteelPipe),
            diameter: 0.375f,
            "steel_pipe_center",
            "steel_pipe_connector",
            "steel_pipe_surface");

        /// <summary>
        ///     A primitive wooden pipe.
        /// </summary>
        public static readonly Block WoodenPipe = new PipeBlock<IPrimitivePipeConnectable>(
            Language.WoodenPipe,
            nameof(WoodenPipe),
            diameter: 0.3125f,
            "wood_pipe_center",
            "wood_pipe_connector",
            "wood_pipe_surface");

        /// <summary>
        ///     A special steel pipe that can only form straight connections.
        /// </summary>
        public static readonly Block StraightSteelPipe = new StraightSteelPipeBlock(
            Language.SteelPipeStraight,
            nameof(StraightSteelPipe),
            diameter: 0.375f,
            "steel_pipe_straight");

        /// <summary>
        ///     A special steel pipe that can be closed.
        /// </summary>
        public static readonly Block PipeValve = new SteelPipeValveBlock(
            Language.ValvePipe,
            nameof(PipeValve),
            diameter: 0.375f,
            "steel_pipe_valve_open",
            "steel_pipe_valve_closed");

        /// <summary>
        ///     A pump that lifts up liquids when interacted with.
        /// </summary>
        public static readonly Block Pump = new PumpBlock(
            Language.Pump,
            nameof(Pump),
            pumpDistance: 16,
            TextureLayout.Uniform("pump"));

        #endregion LIQUID FLOW BLOCKS

        #region SPECIAL BLOCKS

        /// <summary>
        ///     Fire. Burns flammable blocks.
        /// </summary>
        public static readonly Block Fire = new FireBlock(
            Language.Fire,
            nameof(Fire),
            "fire_complete",
            "fire_side",
            "fire_top");

        /// <summary>
        ///     A magical pulsating block.
        /// </summary>
        public static readonly Block Pulsating = new TintedBlock(
            Language.PulsatingBlock,
            nameof(Pulsating),
            BlockFlags.Basic,
            TextureLayout.Uniform("pulsating"),
            isAnimated: true);

        /// <summary>
        ///     A block that, once ignited, will burn for ever time.
        /// </summary>
        public static readonly Block EternalFlame = new EternalFlame(
            Language.EternalFlame,
            nameof(EternalFlame),
            TextureLayout.Uniform("eternal_flame"));

        /// <summary>
        ///     A walkable path.
        /// </summary>
        public static readonly Block Path = new InsetDirtBlock(
            Language.Path,
            nameof(Path),
            TextureLayout.Uniform("dirt"),
            TextureLayout.Uniform("dirt_wet"),
            supportsFullGrowth: false);

        #endregion SPECIAL BLOCKS

        #region NEW BLOCKS

        // Will be filled soon...

        #endregion NEW BLOCKS
    }
}
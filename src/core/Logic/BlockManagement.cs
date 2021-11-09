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

        public static Block TranslateNamedID(string namedId)
        {
            if (namedBlockDictionary.TryGetValue(namedId, out Block? block)) return block;

            logger.LogWarning(
                Events.UnknownBlock,
                "No Block with the named ID {ID} could be found, returning {Fallback} instead",
                namedId,
                nameof(Air));

            return Air;
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

        public static readonly Block Air = new AirBlock(Language.Air, nameof(Air));

        public static readonly Block Grass = new GrassBlock(
            Language.Grass,
            nameof(Grass),
            TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"),
            TextureLayout.UnqiueColumn("grass_side_wet", "dirt_wet", "grass_wet"));

        public static readonly Block GrassBurned = new CoveredGrassSpreadableBlock(
            Language.AshCoveredDirt,
            nameof(GrassBurned),
            TextureLayout.UnqiueColumn("ash_side", "dirt", "ash"),
            hasNeutralTint: false);

        public static readonly Block Dirt = new DirtBlock(
            Language.Dirt,
            nameof(Dirt),
            TextureLayout.Uniform("dirt"),
            TextureLayout.Uniform("dirt_wet"));

        public static readonly Block Farmland = new InsetDirtBlock(
            Language.Farmland,
            nameof(Farmland),
            TextureLayout.UnqiueTop("dirt", "farmland"),
            TextureLayout.UnqiueTop("dirt_wet", "farmland_wet"),
            supportsFullGrowth: true);

        public static readonly Block TallGrass = new CrossPlantBlock(
            Language.TallGrass,
            nameof(TallGrass),
            "tall_grass",
            BlockFlags.Replaceable,
            BoundingBox.CrossBlock);

        public static readonly Block VeryTallGrass = new DoubleCrossPlantBlock(
            Language.VeryTallGrass,
            nameof(VeryTallGrass),
            "very_tall_grass",
            topTexOffset: 1,
            BoundingBox.CrossBlock);

        public static readonly Block Flower = new CrossPlantBlock(
            Language.Flower,
            nameof(Flower),
            "flower",
            BlockFlags.Replaceable,
            new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.175f, y: 0.5f, z: 0.175f)));

        public static readonly Block TallFlower = new DoubleCrossPlantBlock(
            Language.TallFlower,
            nameof(TallFlower),
            "tall_flower",
            topTexOffset: 1,
            BoundingBox.CrossBlock);

        public static readonly Block Stone = new BasicBlock(
            Language.Stone,
            nameof(Stone),
            BlockFlags.Basic,
            TextureLayout.Uniform("stone"));

        public static readonly Block Rubble = new ConstructionBlock(
            Language.Rubble,
            nameof(Rubble),
            TextureLayout.Uniform("rubble"));

        public static readonly Block Mud = new MudBlock(
            Language.Mud,
            nameof(Mud),
            TextureLayout.Uniform("mud"),
            maxVelocity: 0.1f);

        public static readonly Block Pumice = new BasicBlock(
            Language.Pumice,
            nameof(Pumice),
            BlockFlags.Basic,
            TextureLayout.Uniform("pumice"));

        public static readonly Block Obsidian = new BasicBlock(
            Language.Obsidian,
            nameof(Obsidian),
            BlockFlags.Basic,
            TextureLayout.Uniform("obsidian"));

        public static readonly Block Snow = new ModifiableHeightBlock(
            Language.Snow,
            nameof(Snow),
            TextureLayout.Uniform("snow"));

        public static readonly Block Leaves = new NaturalBlock(
            Language.Leaves,
            nameof(Leaves),
            new BlockFlags
            {
                IsSolid = true,
                RenderFaceAtNonOpaques = true
            },
            TextureLayout.Uniform("leaves"));

        public static readonly Block Log = new RotatedBlock(
            Language.Log,
            nameof(Log),
            BlockFlags.Basic,
            TextureLayout.Column("log", sideOffset: 0, endOffset: 1));

        public static readonly Block Wood = new OrganicConstructionBlock(
            Language.Wood,
            nameof(Wood),
            TextureLayout.Uniform("wood"));

        public static readonly Block Sand = new PermeableBlock(
            Language.Sand,
            nameof(Sand),
            TextureLayout.Uniform("sand"));

        public static readonly Block Gravel = new PermeableBlock(
            Language.Gravel,
            nameof(Gravel),
            TextureLayout.Uniform("gravel"));

        public static readonly Block OreCoal = new BasicBlock(
            Language.CoalOre,
            nameof(OreCoal),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_coal"));

        public static readonly Block OreIron = new BasicBlock(
            Language.IronOre,
            nameof(OreIron),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_iron"));

        public static readonly Block OreGold = new BasicBlock(
            Language.GoldOre,
            nameof(OreGold),
            BlockFlags.Basic,
            TextureLayout.Uniform("ore_gold"));

        public static readonly Block Ash = new BasicBlock(
            Language.Ash,
            nameof(Ash),
            BlockFlags.Basic,
            TextureLayout.Uniform("ash"));

        #endregion NATURAL BLOCKS

        #region PLANT BLOCKS

        public static readonly Block Cactus = new GrowingBlock(
            Language.Cactus,
            nameof(Cactus),
            TextureLayout.Column("cactus", sideOffset: 0, endOffset: 1),
            Sand,
            maxHeight: 4);

        public static readonly Block Pumpkin = new GroundedBlock(
            Language.Pumpkin,
            nameof(Pumpkin),
            BlockFlags.Basic,
            TextureLayout.Column("pumpkin_side", "pumpkin_top"));

        public static readonly Block Melon = new GroundedBlock(
            Language.Melon,
            nameof(Melon),
            BlockFlags.Basic,
            TextureLayout.Column("melon_side", "melon_top"));

        public static readonly Block Spiderweb = new SpiderWebBlock(
            Language.SpiderWeb,
            nameof(Spiderweb),
            "spider_web",
            maxVelocity: 0.01f);

        public static readonly Block Vines = new GrowingFlatBlock(
            Language.Vines,
            nameof(Vines),
            "vines",
            climbingVelocity: 2f,
            slidingVelocity: 1f);

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

        public static readonly Block PumpkinPlant = new FruitCropBlock(
            Language.PumpkinPlant,
            nameof(PumpkinPlant),
            "pumpkin_plant",
            Pumpkin);

        public static readonly Block MelonPlant = new FruitCropBlock(
            Language.MelonPlant,
            nameof(MelonPlant),
            "melon_plant",
            Melon);

        #endregion PLANT BLOCKS

        #region BUILDING BLOCKS

        public static readonly Block Glass = new GlassBlock(
            Language.Glass,
            nameof(Glass),
            TextureLayout.Uniform("glass"));

        public static readonly Block GlassTiled = new GlassBlock(
            Language.TiledGlass,
            nameof(GlassTiled),
            TextureLayout.Uniform("glass_tiled"));

        public static readonly Block Steel = new ConstructionBlock(
            Language.Steel,
            nameof(Steel),
            TextureLayout.Uniform("steel"));

        public static readonly Block StoneWorked = new BasicBlock(
            Language.WorkedStone,
            nameof(StoneWorked),
            BlockFlags.Basic,
            TextureLayout.Uniform("stone_worked"));

        public static readonly Block Ladder = new FlatBlock(
            Language.Ladder,
            nameof(Ladder),
            "ladder",
            climbingVelocity: 3f,
            slidingVelocity: 1f);

        public static readonly Block TilesSmall = new ConstructionBlock(
            Language.SmallTiles,
            nameof(TilesSmall),
            TextureLayout.Uniform("small_tiles"));

        public static readonly Block TilesLarge = new ConstructionBlock(
            Language.LargeTiles,
            nameof(TilesLarge),
            TextureLayout.Uniform("large_tiles"));

        public static readonly Block TilesCheckerboardBlack = new TintedBlock(
            Language.CheckerboardTilesBlack,
            nameof(TilesCheckerboardBlack),
            BlockFlags.Basic,
            TextureLayout.Uniform("checkerboard_tiles_black"));

        public static readonly Block TilesCheckerboardWhite = new TintedBlock(
            Language.CheckerboardTilesWhite,
            nameof(TilesCheckerboardWhite),
            BlockFlags.Basic,
            TextureLayout.Uniform("checkerboard_tiles_white"));

        public static readonly Block Bricks = new ConstructionBlock(
            Language.Bricks,
            nameof(Bricks),
            TextureLayout.Uniform("bricks"));

        public static readonly Block PavingStone = new ConstructionBlock(
            Language.PavingStone,
            nameof(PavingStone),
            TextureLayout.Uniform("paving_stone"));

        public static readonly Block RedPlastic = new ConstructionBlock(
            Language.RedPlastic,
            nameof(RedPlastic),
            TextureLayout.Uniform("red_plastic"));

        public static readonly Block Concrete = new ConcreteBlock(
            Language.Concrete,
            nameof(Concrete),
            TextureLayout.Uniform("concrete"));

        #endregion BUILDING BLOCKS

        #region DECORATION BLOCKS

        public static readonly Block StoneFace = new OrientedBlock(
            Language.StoneFace,
            nameof(StoneFace),
            BlockFlags.Basic,
            TextureLayout.UnqiueFront("stone_worked_face", "stone_worked"));

        public static readonly Block Vase = new CustomModelBlock(
            Language.Vase,
            nameof(Vase),
            BlockFlags.Solid,
            "vase",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.375f, z: 0.5f), new Vector3(x: 0.25f, y: 0.375f, z: 0.25f)));

        public static readonly Block Bed = new BedBlock(Language.Bed, nameof(Bed), "bed");

        public static readonly Block Wool = new OrganicTintedBlock(
            Language.Wool,
            nameof(Wool),
            TextureLayout.Uniform("wool"));

        public static readonly Block WoolDecorated = new OrganicTintedBlock(
            Language.DecoratedWool,
            nameof(WoolDecorated),
            TextureLayout.Uniform("wool_decorated"));

        public static readonly Block Carpet = new TintedCustomModelBlock(
            Language.Carpet,
            nameof(Carpet),
            BlockFlags.Solid,
            "carpet",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f)));

        public static readonly Block CarpetDecorated = new TintedCustomModelBlock(
            Language.DecoratedCarpet,
            nameof(CarpetDecorated),
            BlockFlags.Solid,
            "carpet_decorated",
            new BoundingBox(new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f), new Vector3(x: 0.5f, y: 0.03125f, z: 0.5f)));

        public static readonly Block GlassPane = new ThinConnectingBlock(
            Language.GlassPane,
            nameof(GlassPane),
            "pane_glass_post",
            "pane_glass_side",
            "pane_glass_extension");

        public static readonly Block Bars = new ThinConnectingBlock(
            Language.Bars,
            nameof(Bars),
            "bars_post",
            "bars_side",
            "bars_extension");

        #endregion DECORATION BLOCKS

        #region ACCESS BLOCKS

        public static readonly Block FenceWood = new FenceBlock(
            Language.WoodenFence,
            nameof(FenceWood),
            "wood",
            "fence_post",
            "fence_extension");

        public static readonly Block WallRubble = new WallBlock(
            Language.RubbleWall,
            nameof(WallRubble),
            "rubble",
            "wall_post",
            "wall_extension",
            "wall_extension_straight");

        public static readonly Block WallBricks = new WallBlock(
            Language.BrickWall,
            nameof(WallBricks),
            "bricks",
            "wall_post",
            "wall_extension",
            "wall_extension_straight");

        public static readonly Block DoorSteel = new DoorBlock(
            Language.SteelDoor,
            nameof(DoorSteel),
            "door_steel_closed",
            "door_steel_open");

        public static readonly Block DoorWood = new OrganicDoorBlock(
            Language.WoodenDoor,
            nameof(DoorWood),
            "door_wood_closed",
            "door_wood_open");

        public static readonly Block GateWood = new GateBlock(
            Language.WoodenGate,
            nameof(GateWood),
            "gate_wood_closed",
            "gate_wood_open");

        #endregion ACCESS BLOCKS

        #region LIQUID FLOW BLOCKS

        public static readonly Block LiquidBarrier = new LiquidBarrierBlock(
            Language.Barrier,
            nameof(LiquidBarrier),
            TextureLayout.Uniform("liquid_barrier_closed"),
            TextureLayout.Uniform("liquid_barrier_open"));

        public static readonly Block SteelPipe = new PipeBlock<IIndustrialPipeConnectable>(
            Language.SteelPipe,
            nameof(SteelPipe),
            diameter: 0.375f,
            "steel_pipe_center",
            "steel_pipe_connector",
            "steel_pipe_surface");

        public static readonly Block WoodenPipe = new PipeBlock<IPrimitivePipeConnectable>(
            Language.WoodenPipe,
            nameof(WoodenPipe),
            diameter: 0.3125f,
            "wood_pipe_center",
            "wood_pipe_connector",
            "wood_pipe_surface");

        public static readonly Block StraightSteelPipe = new StraightSteelPipeBlock(
            Language.SteelPipeStraight,
            nameof(StraightSteelPipe),
            diameter: 0.375f,
            "steel_pipe_straight");

        public static readonly Block PipeValve = new SteelPipeValveBlock(
            Language.ValvePipe,
            nameof(PipeValve),
            diameter: 0.375f,
            "steel_pipe_valve_open",
            "steel_pipe_valve_closed");

        public static readonly Block Pump = new PumpBlock(
            Language.Pump,
            nameof(Pump),
            pumpDistance: 16,
            TextureLayout.Uniform("pump"));

        #endregion LIQUID FLOW BLOCKS

        #region SPECIAL BLOCKS

        public static readonly Block Fire = new FireBlock(
            Language.Fire,
            nameof(Fire),
            "fire_complete",
            "fire_side",
            "fire_top");

        public static readonly Block Pulsating = new TintedBlock(
            Language.PulsatingBlock,
            nameof(Pulsating),
            BlockFlags.Basic,
            TextureLayout.Uniform("pulsating"),
            isAnimated: true);

        public static readonly Block EternalFlame = new EternalFlame(
            Language.EternalFlame,
            nameof(EternalFlame),
            TextureLayout.Uniform("eternal_flame"));

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

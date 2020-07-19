// <copyright file="BlockManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Logic.Blocks;
using VoxelGame.Physics;
using VoxelGame.Resources.Language;

namespace VoxelGame.Logic
{
    public abstract partial class Block
    {
        private static readonly ILogger logger = Program.CreateLogger<Block>();

        public const int BlockLimit = 2048;

        private static readonly Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();
        private static readonly Dictionary<string, Block> namedBlockDictionary = new Dictionary<string, Block>();

        public static readonly Block AIR = new AirBlock(Language.Air, nameof(AIR));
        public static readonly Block GRASS = new CoveredDirtBlock(Language.Grass, nameof(GRASS), TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"), true);
        public static readonly Block TALL_GRASS = new CrossPlantBlock(Language.TallGrass, nameof(TALL_GRASS), "tall_grass", true, BoundingBox.Block);
        public static readonly Block VERY_TALL_GRASS = new DoubleCrossPlantBlock(Language.VeryTallGrass, nameof(VERY_TALL_GRASS), "very_tall_grass", 1, BoundingBox.Block);
        public static readonly Block DIRT = new DirtBlock(Language.Dirt, nameof(DIRT), TextureLayout.Uniform("dirt"));
        public static readonly Block FARMLAND = new CoveredDirtBlock(Language.Farmland, nameof(FARMLAND), TextureLayout.UnqiueTop("dirt", "farmland"), false);
        public static readonly Block STONE = new BasicBlock(Language.Stone, nameof(STONE), TextureLayout.Uniform("stone"));
        public static readonly Block RUBBLE = new ConstructionBlock(Language.Rubble, nameof(RUBBLE), TextureLayout.Uniform("rubble"));
        public static readonly Block LOG = new RotatedBlock(Language.Log, nameof(LOG), TextureLayout.Column("log", 0, 1));
        public static readonly Block WOOD = new ConstructionBlock(Language.Wood, nameof(WOOD), TextureLayout.Uniform("wood"));
        public static readonly Block SAND = new BasicBlock(Language.Sand, nameof(SAND), TextureLayout.Uniform("sand"));
        public static readonly Block GRAVEL = new BasicBlock(Language.Gravel, nameof(GRAVEL), TextureLayout.Uniform("gravel"));
        public static readonly Block LEAVES = new BasicBlock(Language.Leaves, nameof(LEAVES), TextureLayout.Uniform("leaves"), isOpaque: false);
        public static readonly Block GLASS = new BasicBlock(Language.Glass, nameof(GLASS), TextureLayout.Uniform("glass"), isOpaque: false, renderFaceAtNonOpaques: false);
        public static readonly Block ORE_COAL = new BasicBlock(Language.CoalOre, nameof(ORE_COAL), TextureLayout.Uniform("ore_coal"));
        public static readonly Block ORE_IRON = new BasicBlock(Language.IronOre, nameof(ORE_IRON), TextureLayout.Uniform("ore_iron"));
        public static readonly Block ORE_GOLD = new BasicBlock(Language.GoldOre, nameof(ORE_GOLD), TextureLayout.Uniform("ore_gold"));
        public static readonly Block SNOW = new BasicBlock(Language.Snow, nameof(SNOW), TextureLayout.Uniform("snow"));
        public static readonly Block FLOWER = new CrossPlantBlock(Language.Flower, nameof(FLOWER), "flower", true, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block TALL_FLOWER = new DoubleCrossPlantBlock(Language.TallFlower, nameof(TALL_FLOWER), "tall_flower", 1, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block SPIDERWEB = new SpiderWebBlock(Language.SpiderWeb, nameof(SPIDERWEB), "spider_web", 0.01f);
        public static readonly Block CAVEPAINTING = new OrientedBlock(Language.CavePainting, nameof(CAVEPAINTING), TextureLayout.UnqiueFront("stone_cavepainting", "stone"));
        public static readonly Block LADDER = new FlatBlock(Language.Ladder, nameof(LADDER), "ladder", 3f, 1f);
        public static readonly Block VINES = new GrowingFlatBlock(Language.Vines, nameof(VINES), "vines", 2f, 1f);
        public static readonly Block FENCE_WOOD = new FenceBlock(Language.WoodenFence, nameof(FENCE_WOOD), "wood", "fence_post", "fence_extension");
        public static readonly Block FLAX = new CropBlock(Language.Flax, nameof(FLAX), "flax", 0, 1, 2, 3, 3, 4, 5);
        public static readonly Block POTATOES = new CropBlock(Language.Potatoes, nameof(POTATOES), "potato", 1, 1, 2, 2, 3, 4, 5);
        public static readonly Block ONIONS = new CropBlock(Language.Onions, nameof(ONIONS), "onion", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block WHEAT = new CropBlock(Language.Wheat, nameof(WHEAT), "wheat", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block MAIZE = new DoubeCropBlock(Language.Maize, nameof(MAIZE), "maize", 0, 1, 2, 2, (3, 6), (3, 6), (4, 7), (5, 8));
        public static readonly Block TILES_SMALL = new ConstructionBlock(Language.SmallTiles, nameof(TILES_SMALL), TextureLayout.Uniform("small_tiles"));
        public static readonly Block TILES_LARGE = new ConstructionBlock(Language.LargeTiles, nameof(TILES_LARGE), TextureLayout.Uniform("large_tiles"));
        public static readonly Block TILES_CHECKERBOARD_BLACK = new TintedBlock(Language.CheckerboardTilesBlack, nameof(TILES_CHECKERBOARD_BLACK), TextureLayout.Uniform("checkerboard_tiles_black"));
        public static readonly Block TILES_CHECKERBOARD_WHITE = new TintedBlock(Language.CheckerboardTilesWhite, nameof(TILES_CHECKERBOARD_WHITE), TextureLayout.Uniform("checkerboard_tiles_white"));
        public static readonly Block CACTUS = new GrowingBlock(Language.Cactus, nameof(CACTUS), TextureLayout.Column("cactus", 0, 1), Block.SAND, 4);
        public static readonly Block VASE = new CustomModelBlock(Language.Vase, nameof(VASE), "vase", new BoundingBox(new Vector3(0.5f, 0.375f, 0.5f), new Vector3(0.25f, 0.375f, 0.25f)));
        public static readonly Block BRICKS = new ConstructionBlock(Language.Bricks, nameof(BRICKS), TextureLayout.Uniform("bricks"));
        public static readonly Block PAVING_STONE = new ConstructionBlock(Language.PavingStone, nameof(PAVING_STONE), TextureLayout.Uniform("paving_stone"));
        public static readonly Block WALL_RUBBLE = new WallBlock(Language.RubbleWall, nameof(WALL_RUBBLE), "rubble", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block WALL_BRICKS = new WallBlock(Language.BrickWall, nameof(WALL_BRICKS), "bricks", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block BED = new BedBlock(Language.Bed, nameof(BED), "bed");
        public static readonly Block STEEL = new ConstructionBlock(Language.Steel, nameof(STEEL), TextureLayout.Uniform("steel"));
        public static readonly Block DOOR_STEEL = new DoorBlock(Language.SteelDoor, nameof(DOOR_STEEL), "door_steel_closed", "door_steel_open");
        public static readonly Block DOOR_WOOD = new DoorBlock(Language.WoodenDoor, nameof(DOOR_WOOD), "door_wood_closed", "door_wood_open");
        public static readonly Block GATE_WOOD = new GateBlock(Language.WoodenGate, nameof(GATE_WOOD), "gate_wood_closed", "gate_wood_open");
        public static readonly Block PUMPKIN = new GroundedBlock(Language.Pumpkin, nameof(PUMPKIN), TextureLayout.Column("pumpkin_side", "pumpkin_top"));
        public static readonly Block MELON = new GroundedBlock(Language.Melon, nameof(MELON), TextureLayout.Column("melon_side", "melon_top"));
        public static readonly Block PUMPKIN_PLANT = new FruitCropBlock(Language.PumpkinPlant, nameof(PUMPKIN_PLANT), "pumpkin_plant", 0, 1, 2, 3, 4, PUMPKIN);
        public static readonly Block MELON_PLANT = new FruitCropBlock(Language.MelonPlant, nameof(MELON_PLANT), "melon_plant", 0, 1, 2, 3, 4, MELON);
        public static readonly Block WOOL = new TintedBlock(Language.Wool, nameof(WOOL), TextureLayout.Uniform("wool"));
        public static readonly Block CARPET = new TintedCustomModelBlock(Language.Carpet, nameof(CARPET), "carpet", new BoundingBox(new Vector3(0.5f, 0.03125f, 0.5f), new Vector3(0.5f, 0.03125f, 0.5f)));
        public static readonly Block FIRE = new FireBlock(Language.Fire, nameof(FIRE), "fire");

        /// <summary>
        /// Translates a block ID to a reference to the block that has that ID. If the ID is not valid, air is returned.
        /// </summary>
        /// <param name="id">The ID of the block to return.</param>
        /// <returns>The block with the ID or air if the ID is not valid.</returns>
        public static Block TranslateID(ushort id)
        {
            if (blockDictionary.TryGetValue(id, out Block? block))
            {
                return block;
            }
            else
            {
                logger.LogWarning("No Block with the ID {id} could be found, returning {fallback} instead.", id, nameof(Block.AIR));

                return Block.AIR;
            }
        }

        public static Block TranslateNamedID(string namedId)
        {
            if (namedBlockDictionary.TryGetValue(namedId, out Block? block))
            {
                return block;
            }
            else
            {
                logger.LogWarning("No Block with the named ID {id} could be found, returning {fallback} instead.", namedId, nameof(Block.AIR));

                return Block.AIR;
            }
        }

        /// <summary>
        /// Gets the count of registered blocks.
        /// </summary>
        public static int Count { get => blockDictionary.Count; }

        /// <summary>
        /// Calls the setup method on all blocks.
        /// </summary>
        public static void LoadBlocks()
        {
            using (logger.BeginScope("Block Loading"))
            {
                foreach (Block block in blockDictionary.Values)
                {
                    block.Setup();

                    logger.LogDebug(LoggingEvents.BlockLoad, "Loaded the block [{block}] with ID {id}.", block, block.Id);
                }

                logger.LogInformation("Block setup complete. A total of {count} blocks have been loaded.", Count);
            }
        }
    }
}
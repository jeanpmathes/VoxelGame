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

        public static readonly Block AIR = new AirBlock(Language.Air);
        public static readonly Block GRASS = new CoveredDirtBlock(Language.Grass, TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"), true);
        public static readonly Block TALL_GRASS = new CrossPlantBlock(Language.TallGrass, "tall_grass", true, BoundingBox.Block);
        public static readonly Block VERY_TALL_GRASS = new DoubleCrossPlantBlock(Language.VeryTallGrass, "very_tall_grass", 1, BoundingBox.Block);
        public static readonly Block DIRT = new DirtBlock(Language.Dirt, TextureLayout.Uniform("dirt"));
        public static readonly Block FARMLAND = new CoveredDirtBlock(Language.Farmland, TextureLayout.UnqiueTop("dirt", "farmland"), false);
        public static readonly Block STONE = new BasicBlock(Language.Stone, TextureLayout.Uniform("stone"));
        public static readonly Block RUBBLE = new ConstructionBlock(Language.Rubble, TextureLayout.Uniform("rubble"));
        public static readonly Block LOG = new RotatedBlock(Language.Log, TextureLayout.Column("log", 0, 1));
        public static readonly Block WOOD = new ConstructionBlock(Language.Wood, TextureLayout.Uniform("wood"));
        public static readonly Block SAND = new BasicBlock(Language.Sand, TextureLayout.Uniform("sand"));
        public static readonly Block GRAVEL = new BasicBlock(Language.Gravel, TextureLayout.Uniform("gravel"));
        public static readonly Block LEAVES = new BasicBlock(Language.Leaves, TextureLayout.Uniform("leaves"), isOpaque: false);
        public static readonly Block GLASS = new BasicBlock(Language.Glass, TextureLayout.Uniform("glass"), isOpaque: false, renderFaceAtNonOpaques: false);
        public static readonly Block ORE_COAL = new BasicBlock(Language.CoalOre, TextureLayout.Uniform("ore_coal"));
        public static readonly Block ORE_IRON = new BasicBlock(Language.IronOre, TextureLayout.Uniform("ore_iron"));
        public static readonly Block ORE_GOLD = new BasicBlock(Language.GoldOre, TextureLayout.Uniform("ore_gold"));
        public static readonly Block SNOW = new BasicBlock(Language.Snow, TextureLayout.Uniform("snow"));
        public static readonly Block FLOWER = new CrossPlantBlock(Language.Flower, "flower", false, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block TALL_FLOWER = new DoubleCrossPlantBlock(Language.TallFlower, "tall_flower", 1, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block SPIDERWEB = new SpiderWebBlock(Language.SpiderWeb, "spider_web", 0.01f);
        public static readonly Block CAVEPAINTING = new OrientedBlock(Language.CavePainting, TextureLayout.UnqiueFront("stone_cavepainting", "stone"));
        public static readonly Block LADDER = new FlatBlock(Language.Ladder, "ladder", 3f, 1f);
        public static readonly Block VINES = new GrowingFlatBlock(Language.Vines, "vines", 2f, 1f);
        public static readonly Block FENCE_WOOD = new FenceBlock(Language.WoodenFence, "wood", "fence_post", "fence_extension");
        public static readonly Block FLAX = new CropBlock(Language.Flax, "flax", 0, 1, 2, 3, 3, 4, 5);
        public static readonly Block POTATOES = new CropBlock(Language.Potatoes, "potato", 1, 1, 2, 2, 3, 4, 5);
        public static readonly Block ONIONS = new CropBlock(Language.Onions, "onion", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block WHEAT = new CropBlock(Language.Wheat, "wheat", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block MAIZE = new DoubeCropBlock(Language.Maize, "maize", 0, 1, 2, 2, (3, 6), (3, 6), (4, 7), (5, 8));
        public static readonly Block TILES_SMALL = new ConstructionBlock(Language.SmallTiles, TextureLayout.Uniform("small_tiles"));
        public static readonly Block TILES_LARGE = new ConstructionBlock(Language.LargeTiles, TextureLayout.Uniform("large_tiles"));
        public static readonly Block TILES_CHECKERBOARD_BLACK = new TintedBlock(Language.CheckerboardTilesBlack, TextureLayout.Uniform("checkerboard_tiles_black"));
        public static readonly Block TILES_CHECKERBOARD_WHITE = new TintedBlock(Language.CheckerboardTilesWhite, TextureLayout.Uniform("checkerboard_tiles_white"));
        public static readonly Block CACTUS = new GrowingBlock(Language.Cactus, TextureLayout.Column("cactus", 0, 1), Block.SAND, 4);
        public static readonly Block VASE = new CustomModelBlock(Language.Vase, "vase", true, new BoundingBox(new Vector3(0.5f, 0.375f, 0.5f), new Vector3(0.25f, 0.375f, 0.25f)));
        public static readonly Block BRICKS = new ConstructionBlock(Language.Bricks, TextureLayout.Uniform("bricks"));
        public static readonly Block PAVING_STONE = new ConstructionBlock(Language.PavingStone, TextureLayout.Uniform("paving_stone"));
        public static readonly Block WALL_RUBBLE = new WallBlock(Language.RubbleWall, "rubble", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block WALL_BRICKS = new WallBlock(Language.BrickWall, "bricks", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block BED = new BedBlock(Language.Bed, "bed");
        public static readonly Block STEEL = new ConstructionBlock(Language.Steel, TextureLayout.Uniform("steel"));
        public static readonly Block DOOR_STEEL = new DoorBlock(Language.SteelDoor, "door_steel_closed", "door_steel_open");
        public static readonly Block DOOR_WOOD = new DoorBlock(Language.WoodenDoor, "door_wood_closed", "door_wood_open");
        public static readonly Block GATE_WOOD = new GateBlock(Language.WoodenGate, "gate_wood_closed", "gate_wood_open");

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

                    logger.LogDebug(LoggingEvents.BlockLoad, "Loaded the block {block} with ID {id}.", block, block.Id);
                }

                logger.LogInformation("Block setup complete. A total of {count} blocks have been loaded.", Count);
            }
        }
    }
}
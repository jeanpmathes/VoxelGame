// <copyright file="BlockManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System.Collections.Generic;
using VoxelGame.Logic.Blocks;
using VoxelGame.Logic.Blocks.Misc;
using VoxelGame.Logic.Blocks.Nature;
using VoxelGame.Physics;
using VoxelGame.Resources.Language;

namespace VoxelGame.Logic
{
    public abstract partial class Block : IBlockBase
    {
        private static readonly ILogger logger = Program.CreateLogger<Block>();

        public const int BlockLimit = 2048;

        private static readonly Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();
        private static readonly Dictionary<string, Block> namedBlockDictionary = new Dictionary<string, Block>();

        #region NATURAL BLOCKS

        public static readonly Block Air = new AirBlock(Language.Air, nameof(Air));
        public static readonly Block Grass = new GrassBlock(Language.Grass, nameof(Grass), TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"));
        public static readonly Block GrassBurned = new CoveredDirtBlock(Language.AshCoveredDirt, nameof(GrassBurned), TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"), false);
        public static readonly Block Dirt = new DirtBlock(Language.Dirt, nameof(Dirt), TextureLayout.Uniform("dirt"));
        public static readonly Block Farmland = new CoveredDirtBlock(Language.Farmland, nameof(Farmland), TextureLayout.UnqiueTop("dirt", "farmland"), false);
        public static readonly Block TallGrass = new CrossPlantBlock(Language.TallGrass, nameof(TallGrass), "tall_grass", true, BoundingBox.CrossBlock);
        public static readonly Block VeryTallGrass = new DoubleCrossPlantBlock(Language.VeryTallGrass, nameof(VeryTallGrass), "very_tall_grass", 1, BoundingBox.CrossBlock);
        public static readonly Block Flower = new CrossPlantBlock(Language.Flower, nameof(Flower), "flower", true, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.175f, 0.5f, 0.175f)));
        public static readonly Block TallFlower = new DoubleCrossPlantBlock(Language.TallFlower, nameof(TallFlower), "tall_flower", 1, BoundingBox.CrossBlock);
        public static readonly Block Stone = new BasicBlock(Language.Stone, nameof(Stone), TextureLayout.Uniform("stone"));
        public static readonly Block Rubble = new ConstructionBlock(Language.Rubble, nameof(Rubble), TextureLayout.Uniform("rubble"));
        public static readonly Block Snow = new BasicBlock(Language.Snow, nameof(Snow), TextureLayout.Uniform("snow"));
        public static readonly Block Leaves = new NaturalBlock(Language.Leaves, nameof(Leaves), TextureLayout.Uniform("leaves"), isOpaque: false);
        public static readonly Block Log = new RotatedBlock(Language.Log, nameof(Log), TextureLayout.Column("log", 0, 1));
        public static readonly Block Wood = new OrganicConstructionBlock(Language.Wood, nameof(Wood), TextureLayout.Uniform("wood"));
        public static readonly Block Sand = new BasicBlock(Language.Sand, nameof(Sand), TextureLayout.Uniform("sand"));
        public static readonly Block Gravel = new BasicBlock(Language.Gravel, nameof(Gravel), TextureLayout.Uniform("gravel"));
        public static readonly Block OreCoal = new BasicBlock(Language.CoalOre, nameof(OreCoal), TextureLayout.Uniform("ore_coal"));
        public static readonly Block OreIron = new BasicBlock(Language.IronOre, nameof(OreIron), TextureLayout.Uniform("ore_iron"));
        public static readonly Block OreGold = new BasicBlock(Language.GoldOre, nameof(OreGold), TextureLayout.Uniform("ore_gold"));

        #endregion NATURAL BLOCKS

        #region PLANT BLOCKS

        public static readonly Block Cactus = new GrowingBlock(Language.Cactus, nameof(Cactus), TextureLayout.Column("cactus", 0, 1), Block.Sand, 4);
        public static readonly Block Pumpkin = new GroundedBlock(Language.Pumpkin, nameof(Pumpkin), TextureLayout.Column("pumpkin_side", "pumpkin_top"));
        public static readonly Block Melon = new GroundedBlock(Language.Melon, nameof(Melon), TextureLayout.Column("melon_side", "melon_top"));
        public static readonly Block Spiderweb = new SpiderWebBlock(Language.SpiderWeb, nameof(Spiderweb), "spider_web", 0.01f);
        public static readonly Block Vines = new GrowingFlatBlock(Language.Vines, nameof(Vines), "vines", 2f, 1f);
        public static readonly Block Flax = new CropBlock(Language.Flax, nameof(Flax), "flax", 0, 1, 2, 3, 3, 4, 5);
        public static readonly Block Potatoes = new CropBlock(Language.Potatoes, nameof(Potatoes), "potato", 1, 1, 2, 2, 3, 4, 5);
        public static readonly Block Onions = new CropBlock(Language.Onions, nameof(Onions), "onion", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block Wheat = new CropBlock(Language.Wheat, nameof(Wheat), "wheat", 0, 1, 1, 2, 2, 3, 4);
        public static readonly Block Maize = new DoubeCropBlock(Language.Maize, nameof(Maize), "maize", 0, 1, 2, 2, (3, 6), (3, 6), (4, 7), (5, 8));
        public static readonly Block PumpkinPlant = new FruitCropBlock(Language.PumpkinPlant, nameof(PumpkinPlant), "pumpkin_plant", 0, 1, 2, 3, 4, Pumpkin);
        public static readonly Block MelonPlant = new FruitCropBlock(Language.MelonPlant, nameof(MelonPlant), "melon_plant", 0, 1, 2, 3, 4, Melon);

        #endregion PLANT BLOCKS

        #region BUILDING BLOCKS

        public static readonly Block Glass = new BasicBlock(Language.Glass, nameof(Glass), TextureLayout.Uniform("glass"), isOpaque: false, renderFaceAtNonOpaques: false);
        public static readonly Block Steel = new ConstructionBlock(Language.Steel, nameof(Steel), TextureLayout.Uniform("steel"));
        public static readonly Block StoneWorked = new BasicBlock(Language.WorkedStone, nameof(StoneWorked), TextureLayout.Uniform("stone_worked"));
        public static readonly Block Ladder = new FlatBlock(Language.Ladder, nameof(Ladder), "ladder", 3f, 1f);
        public static readonly Block TilesSmall = new ConstructionBlock(Language.SmallTiles, nameof(TilesSmall), TextureLayout.Uniform("small_tiles"));
        public static readonly Block TilesLarge = new ConstructionBlock(Language.LargeTiles, nameof(TilesLarge), TextureLayout.Uniform("large_tiles"));
        public static readonly Block TilesCheckerboardBlack = new TintedBlock(Language.CheckerboardTilesBlack, nameof(TilesCheckerboardBlack), TextureLayout.Uniform("checkerboard_tiles_black"));
        public static readonly Block TilesCheckerboardWhite = new TintedBlock(Language.CheckerboardTilesWhite, nameof(TilesCheckerboardWhite), TextureLayout.Uniform("checkerboard_tiles_white"));
        public static readonly Block Bricks = new ConstructionBlock(Language.Bricks, nameof(Bricks), TextureLayout.Uniform("bricks"));
        public static readonly Block PavingStone = new ConstructionBlock(Language.PavingStone, nameof(PavingStone), TextureLayout.Uniform("paving_stone"));

        #endregion BUILDING BLOCKS

        #region DECORATION BLOCKS

        public static readonly Block StoneFace = new OrientedBlock(Language.StoneFace, nameof(StoneFace), TextureLayout.UnqiueFront("stone_worked_face", "stone_worked"));
        public static readonly Block Vase = new CustomModelBlock(Language.Vase, nameof(Vase), "vase", new BoundingBox(new Vector3(0.5f, 0.375f, 0.5f), new Vector3(0.25f, 0.375f, 0.25f)));
        public static readonly Block Bed = new BedBlock(Language.Bed, nameof(Bed), "bed");
        public static readonly Block Wool = new OrganicTintedBlock(Language.Wool, nameof(Wool), TextureLayout.Uniform("wool"));
        public static readonly Block Carpet = new TintedCustomModelBlock(Language.Carpet, nameof(Carpet), "carpet", new BoundingBox(new Vector3(0.5f, 0.03125f, 0.5f), new Vector3(0.5f, 0.03125f, 0.5f)));

        #endregion DECORATION BLOCKS

        #region ACCESS BLOCKS

        public static readonly Block FenceWood = new FenceBlock(Language.WoodenFence, nameof(FenceWood), "wood", "fence_post", "fence_extension");
        public static readonly Block WallRubble = new WallBlock(Language.RubbleWall, nameof(WallRubble), "rubble", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block WallBricks = new WallBlock(Language.BrickWall, nameof(WallBricks), "bricks", "wall_post", "wall_extension", "wall_extension_straight");
        public static readonly Block DoorSteel = new DoorBlock(Language.SteelDoor, nameof(DoorSteel), "door_steel_closed", "door_steel_open");
        public static readonly Block DoorWood = new OrganicDoorBlock(Language.WoodenDoor, nameof(DoorWood), "door_wood_closed", "door_wood_open");
        public static readonly Block GateWood = new GateBlock(Language.WoodenGate, nameof(GateWood), "gate_wood_closed", "gate_wood_open");

        #endregion ACCESS BLOCKS

        #region SPECIAL BLOCKS

        public static readonly Block Fire = new FireBlock(Language.Fire, nameof(Fire), "fire");
        public static readonly Block Pulsating = new TintedBlock(Language.PulsatingBlock, nameof(Pulsating), TextureLayout.Uniform("pulsating"), isAnimated: true);
        public static readonly Block EternalFlame = new EternalFlame(Language.EternalFlame, nameof(EternalFlame), TextureLayout.Uniform("eternal_flame"));

        #endregion SPECIAL BLOCKS

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
                logger.LogWarning("No Block with the ID {id} could be found, returning {fallback} instead.", id, nameof(Block.Air));

                return Block.Air;
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
                logger.LogWarning("No Block with the named ID {id} could be found, returning {fallback} instead.", namedId, nameof(Block.Air));

                return Block.Air;
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
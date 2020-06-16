// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using Resources;
using System.Collections.Generic;
using VoxelGame.Logic.Blocks;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract class Block
    {
        #region STATIC BLOCK MANAGMENT

#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static Block AIR;
        public static Block GRASS;
        public static Block TALL_GRASS;
        public static Block DIRT;
        public static Block FARMLAND;
        public static Block STONE;
        public static Block RUBBLE;
        public static Block LOG;
        public static Block WOOD;
        public static Block LEAVES;
        public static Block SAND;
        public static Block GRAVEL;
        public static Block GLASS;
        public static Block ORE_COAL;
        public static Block ORE_IRON;
        public static Block ORE_GOLD;
        public static Block SNOW;
        public static Block FLOWER;
        public static Block SPIDERWEB;
        public static Block CAVEPAINTING;
        public static Block LADDER;
        public static Block VINES;
        public static Block FENCE_WOOD;
        public static Block FLAX;
        public static Block POTATOES;
        public static Block ONIONS;
        public static Block WHEAT;
        public static Block TILES_SMALL;
        public static Block TILES_LARGE;
        public static Block TILES_CHECKERBOARD;
        public static Block CACTUS;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public const int BlockLimit = 2048;

        private static readonly Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();

        /// <summary>
        /// Translates a block ID to a reference to the block that has that ID. If the ID is not valid, air is returned.
        /// </summary>
        /// <param name="id">The ID of the block to return.</param>
        /// <returns>The block with the ID or air if the ID is not valid.</returns>
        public static Block TranslateID(ushort id)
        {
            if (blockDictionary.TryGetValue(id, out Block block))
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

        public static void LoadBlocks()
        {
            AIR = new AirBlock(Language.Air);
            GRASS = new CoveredDirtBlock(Language.Grass, TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"), true);
            TALL_GRASS = new CrossPlantBlock(Language.TallGrass, "tall_grass", true, BoundingBox.Block);
            DIRT = new DirtBlock(Language.Dirt, TextureLayout.Uniform("dirt"));
            FARMLAND = new CoveredDirtBlock("Farmland", TextureLayout.UnqiueTop("dirt", "farmland"), false);
            STONE = new BasicBlock(Language.Stone, TextureLayout.Uniform("stone"), true, true, true);
            RUBBLE = new ConstructionBlock(Language.Rubble, TextureLayout.Uniform("rubble"));
            LOG = new RotatedBlock(Language.Log, TextureLayout.Column("log", 0, 1), true, true, true);
            WOOD = new ConstructionBlock(Language.Wood, TextureLayout.Uniform("wood"));
            SAND = new BasicBlock(Language.Sand, TextureLayout.Uniform("sand"), true, true, true);
            GRAVEL = new BasicBlock(Language.Gravel, TextureLayout.Uniform("gravel"), true, true, true);
            LEAVES = new BasicBlock(Language.Leaves, TextureLayout.Uniform("leaves"), false, true, true);
            GLASS = new BasicBlock(Language.Glass, TextureLayout.Uniform("glass"), false, false, true);
            ORE_COAL = new BasicBlock(Language.CoalOre, TextureLayout.Uniform("ore_coal"), true, true, true);
            ORE_IRON = new BasicBlock(Language.IronOre, TextureLayout.Uniform("ore_iron"), true, true, true);
            ORE_GOLD = new BasicBlock(Language.GoldOre, TextureLayout.Uniform("ore_gold"), true, true, true);
            SNOW = new BasicBlock(Language.Snow, TextureLayout.Uniform("snow"), true, true, true);
            FLOWER = new CrossPlantBlock(Language.Flower, "flower", false, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
            SPIDERWEB = new SpiderWebBlock(Language.SpiderWeb, "spider_web", 0.01f);
            CAVEPAINTING = new OrientedBlock(Language.CavePainting, TextureLayout.UnqiueFront("stone_cavepainting", "stone"), true, true, true);
            LADDER = new FlatBlock(Language.Ladder, "ladder", 3f, 1f, false);
            VINES = new FlatBlock(Language.Vines, "vines", 2f, 1f, true);
            FENCE_WOOD = new FenceBlock(Language.WoodenFence, "wood");
            FLAX = new CropBlock(Language.Flax, "flax", 0, 1, 2, 3, 3, 4, 5);
            POTATOES = new CropBlock(Language.Potatoes, "potato", 1, 1, 2, 2, 3, 4, 5);
            ONIONS = new CropBlock(Language.Onions, "onion", 0, 1, 1, 2, 2, 3, 4);
            WHEAT = new CropBlock(Language.Wheat, "wheat", 0, 1, 1, 2, 2, 3, 4);
            TILES_SMALL = new ConstructionBlock(Language.SmallTiles, TextureLayout.Uniform("small_tiles"));
            TILES_LARGE = new ConstructionBlock(Language.LargeTiles, TextureLayout.Uniform("large_tiles"));
            TILES_CHECKERBOARD = new TintedBlock(Language.CheckerboardTiles, TextureLayout.Uniform("checkerboard_tiles"));
            CACTUS = new GrowingBlock(Language.Cactus, TextureLayout.Column("cactus", 0, 1), Block.SAND, 4);
        }

        #endregion STATIC BLOCK MANAGMENT

        /// <summary>
        /// Gets the block id which can be any value from 0 to 4095.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets the name of the block, which is also used for finding the right texture.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether this block completely fills a 1x1x1 volume or not.
        /// </summary>
        public bool IsFull { get; }

        /// <summary>
        /// Gets whether it is possible to see through this block. This will affect the rendering of this block and the blocks around him.
        /// </summary>
        public bool IsOpaque { get; }

        /// <summary>
        /// This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to another non-opaque block.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; }

        /// <summary>
        /// Gets whether this block hinders movement.
        /// </summary>
        public bool IsSolid { get; }

        /// <summary>
        /// Gets whether the collision method should be called in case of a collision with an entity.
        /// </summary>
        public bool RecieveCollisions { get; }

        /// <summary>
        /// Gets whether this block should be checked in collision calculations even if it is not solid.
        /// </summary>
        public bool IsTrigger { get; }

        /// <summary>
        /// Gets whether this block can be replaced when placing a block.
        /// </summary>
        public bool IsReplaceable { get; }

        /// <summary>
        /// Gets the section buffer this blocks mesh data should be stored in.
        /// </summary>
        public TargetBuffer TargetBuffer { get; }

        private BoundingBox boundingBox;

        protected Block(string name, bool isFull, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid, bool recieveCollisions, bool isTrigger, bool isReplaceable, BoundingBox boundingBox, TargetBuffer targetBuffer)
        {
            Name = name;
            IsFull = isFull;
            IsOpaque = isOpaque;
            RenderFaceAtNonOpaques = renderFaceAtNonOpaques;
            IsSolid = isSolid;
            RecieveCollisions = recieveCollisions;
            IsTrigger = isTrigger;
            IsReplaceable = isReplaceable;

            this.boundingBox = boundingBox;

            TargetBuffer = targetBuffer;

            if (targetBuffer == TargetBuffer.Simple && !isFull)
            {
                throw new System.ArgumentException($"TargetBuffer '{nameof(TargetBuffer.Simple)}' requires {nameof(isFull)} to be {!isFull}.", nameof(targetBuffer));
            }

            if (blockDictionary.Count < BlockLimit)
            {
                blockDictionary.Add((ushort)blockDictionary.Count, this);
                Id = (ushort)(blockDictionary.Count - 1);
            }
            else
            {
                throw new System.InvalidOperationException($"Not more than {BlockLimit} blocks are allowed.");
            }
        }

        public virtual BoundingBox GetBoundingBox(int x, int y, int z)
        {
            return new BoundingBox(boundingBox.Center + new Vector3(x, y, z), boundingBox.Extents);
        }

        /// <summary>
        /// Tries to place a block in the world.
        /// </summary>
        /// <param name="x">The x position where a block should be placed.</param>
        /// <param name="y">The y position where a block should be placed.</param>
        /// <param name="z">The z position where a block should be placed.</param>
        /// <param name="entity">The entity that tries to place the block. May be null.</param>
        /// <returns>Returns true if placing the block was successful.</returns>
        public virtual bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable != true)
            {
                return false;
            }

            Game.World.SetBlock(this, 0, x, y, z);

            return true;
        }

        /// <summary>
        /// Destroys a block in the world if it is the same type as this block.
        /// </summary>
        /// <param name="x">The x position of the block to destroy.</param>
        /// <param name="y">The y position of the block to destroy.</param>
        /// <param name="z">The z position of the block to destroy.</param>
        /// <param name="entity">The entity which caused the destruction, or null if no entity caused it.</param>
        /// <returns>Returns true if the block has been destroyed.</returns>
        public virtual bool Destroy(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _) != this)
            {
                return false;
            }

            Game.World.SetBlock(Block.AIR, 0, x, y, z);

            return true;
        }

        /// <summary>
        /// Returns the mesh of a block side at a certain position.
        /// </summary>
        /// <param name="side">The side of the block that is required.</param>
        /// <param name="data">The block data of the block at the position.</param>
        /// <param name="vertices">Vertices of the mesh. Every vertex is made up of 8 floats: XYZ, UV, NOP</param>
        /// <param name="indices">The indices of the mesh that determine how triangles are constructed.</param>
        /// <returns>The amount of vertices in the mesh.</returns>
        public abstract uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

        public virtual void BlockUpdate(int x, int y, int z, byte data)
        {
        }

        public virtual void EntityCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
        }

        public virtual void RandomUpdate(int x, int y, int z, byte data)
        {
        }

        public sealed override string ToString()
        {
            return $"Block [{Name}]";
        }
    }
}
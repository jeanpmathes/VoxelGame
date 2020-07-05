// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System.Collections.Generic;
using VoxelGame.Logic.Blocks;
using VoxelGame.Physics;
using VoxelGame.Resources.Language;
using VoxelGame.Visuals;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract class Block
    {
        #region STATIC BLOCK MANAGMENT

        private static readonly Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();

        public static readonly Block AIR = new AirBlock(Language.Air);
        public static readonly Block GRASS = new CoveredDirtBlock(Language.Grass, TextureLayout.UnqiueColumn("grass_side", "dirt", "grass"), true);
        public static readonly Block TALL_GRASS = new CrossPlantBlock(Language.TallGrass, "tall_grass", true, BoundingBox.Block);
        public static readonly Block VERY_TALL_GRASS = new DoubleCrossPlantBlock(Language.VeryTallGrass, "very_tall_grass", 1, BoundingBox.Block);
        public static readonly Block DIRT = new DirtBlock(Language.Dirt, TextureLayout.Uniform("dirt"));
        public static readonly Block FARMLAND = new CoveredDirtBlock(Language.Farmland, TextureLayout.UnqiueTop("dirt", "farmland"), false);
        public static readonly Block STONE = new BasicBlock(Language.Stone, TextureLayout.Uniform("stone"), true, true, true, false);
        public static readonly Block RUBBLE = new ConstructionBlock(Language.Rubble, TextureLayout.Uniform("rubble"));
        public static readonly Block LOG = new RotatedBlock(Language.Log, TextureLayout.Column("log", 0, 1), true, true, true);
        public static readonly Block WOOD = new ConstructionBlock(Language.Wood, TextureLayout.Uniform("wood"));
        public static readonly Block SAND = new BasicBlock(Language.Sand, TextureLayout.Uniform("sand"), true, true, true, false);
        public static readonly Block GRAVEL = new BasicBlock(Language.Gravel, TextureLayout.Uniform("gravel"), true, true, true, false);
        public static readonly Block LEAVES = new BasicBlock(Language.Leaves, TextureLayout.Uniform("leaves"), false, true, true, false);
        public static readonly Block GLASS = new BasicBlock(Language.Glass, TextureLayout.Uniform("glass"), false, false, true, false);
        public static readonly Block ORE_COAL = new BasicBlock(Language.CoalOre, TextureLayout.Uniform("ore_coal"), true, true, true, false);
        public static readonly Block ORE_IRON = new BasicBlock(Language.IronOre, TextureLayout.Uniform("ore_iron"), true, true, true, false);
        public static readonly Block ORE_GOLD = new BasicBlock(Language.GoldOre, TextureLayout.Uniform("ore_gold"), true, true, true, false);
        public static readonly Block SNOW = new BasicBlock(Language.Snow, TextureLayout.Uniform("snow"), true, true, true, false);
        public static readonly Block FLOWER = new CrossPlantBlock(Language.Flower, "flower", false, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block TALL_FLOWER = new DoubleCrossPlantBlock(Language.TallFlower, "tall_flower", 1, new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)));
        public static readonly Block SPIDERWEB = new SpiderWebBlock(Language.SpiderWeb, "spider_web", 0.01f);
        public static readonly Block CAVEPAINTING = new OrientedBlock(Language.CavePainting, TextureLayout.UnqiueFront("stone_cavepainting", "stone"), true, true, true);
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

        public const int BlockLimit = 2048;

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

        public static void LoadBlocks()
        {
            foreach (Block block in blockDictionary.Values)
            {
                block.Setup();
            }
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
        /// Gets whether this block responds to interactions.
        /// </summary>
        public bool IsInteractable { get; }

        /// <summary>
        /// Gets the section buffer this blocks mesh data should be stored in.
        /// </summary>
        public TargetBuffer TargetBuffer { get; }

        /// <summary>
        /// Gets whether this block is solid and full.
        /// </summary>
        public bool IsSolidAndFull => IsSolid && IsFull;

        private BoundingBox boundingBox;

        protected Block(string name, bool isFull, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid, bool recieveCollisions, bool isTrigger, bool isReplaceable, bool isInteractable, BoundingBox boundingBox, TargetBuffer targetBuffer)
        {
            Name = name;
            IsFull = isFull;
            IsOpaque = isOpaque;
            RenderFaceAtNonOpaques = renderFaceAtNonOpaques;
            IsSolid = isSolid;
            RecieveCollisions = recieveCollisions;
            IsTrigger = isTrigger;
            IsReplaceable = isReplaceable;
            IsInteractable = isInteractable;

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

        protected virtual void Setup()
        {
        }

        /// <summary>
        /// Returns the bounding box of this block if it would be at the given position.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        /// <returns>The bounding box.</returns>
        public BoundingBox GetBoundingBox(int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) == this)
            {
                return GetBoundingBox(x, y, z, data);
            }
            else
            {
                return boundingBox.Translated(x, y, z);
            }
        }

        protected virtual BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            return boundingBox.Translated(x, y, z);
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

        /// <summary>
        /// Tries to place a block in the world.
        /// </summary>
        /// <param name="x">The x position where a block should be placed.</param>
        /// <param name="y">The y position where a block should be placed.</param>
        /// <param name="z">The z position where a block should be placed.</param>
        /// <param name="entity">The entity that tries to place the block. May be null.</param>
        /// <returns>Returns true if placing the block was successful.</returns>
        public bool Place(int x, int y, int z, Entities.PhysicsEntity? entity)
        {
            return Place(x, y, z, Game.World.GetBlock(x, y, z, out _)?.IsReplaceable, entity);
        }

        protected virtual bool Place(int x, int y, int z, bool? replaceable, Entities.PhysicsEntity? entity)
        {
            if (replaceable != true)
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
        public bool Destroy(int x, int y, int z, Entities.PhysicsEntity? entity)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) == this)
            {
                return Destroy(x, y, z, data, entity);
            }
            else
            {
                return false;
            }
        }

        protected virtual bool Destroy(int x, int y, int z, byte data, Entities.PhysicsEntity? entity)
        {
            Game.World.SetBlock(Block.AIR, 0, x, y, z);

            return true;
        }

        /// <summary>
        /// This method is called when an entity collides with this block.
        /// </summary>
        /// <param name="entity">The entity that caused the collision.</param>
        /// <param name="x">The x position of the block the entity collided with.</param>
        /// <param name="y">The y position of the block the entity collided with.</param>
        /// <param name="z">The z position of the block the entity collided with.</param>
        public void EntityCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) == this)
            {
                EntityCollision(entity, x, y, z, data);
            }
        }

        protected virtual void EntityCollision(Entities.PhysicsEntity entity, int x, int y, int z, byte data)
        {
        }

        public void EntityInteract(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out byte data) == this)
            {
                EntityInteract(entity, x, y, z, data);
            }
        }

        protected virtual void EntityInteract(Entities.PhysicsEntity entity, int x, int y, int z, byte data)
        {
        }

        /// <summary>
        /// This method is called on blocks next to a position that was changed.
        /// </summary>
        /// <param name="x">The x position of the block next to the changed position.</param>
        /// <param name="y">The y position of the block next to the changed position.</param>
        /// <param name="z">The z position of the block next to the changed position.</param>
        /// <param name="data">The data of the block next to the changed position.</param>
        /// <param name="side">The side of the block where the change happened.</param>
        internal virtual void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
        }

        /// <summary>
        /// This method is called randomly on some blocks every update.
        /// </summary>
        internal virtual void RandomUpdate(int x, int y, int z, byte data)
        {
        }

        public sealed override string ToString()
        {
            return $"Block [{Name}]";
        }

        public sealed override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }

        public sealed override int GetHashCode()
        {
            return Id;
        }
    }
}
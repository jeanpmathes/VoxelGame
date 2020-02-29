// <copyright file="Block.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using OpenTK;
using System.Collections.Generic;

using VoxelGame.Physics;

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
        public static Block STONE;
        public static Block COBBLESTONE;
        public static Block LOG;
        public static Block LEAVES;
        public static Block SAND;
        public static Block GLASS;
        public static Block ORE_COAL;
        public static Block ORE_IRON;
        public static Block ORE_GOLD;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public static Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();

        public static void LoadBlocks()
        {
            AIR = new AirBlock("air");
            GRASS = new BasicBlock("grass", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 2), true, BoundingBox.Block);
            TALL_GRASS = new CrossBlock("tall_grass", BoundingBox.Block);
            DIRT = new BasicBlock("dirt", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            STONE = new BasicBlock("stone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            COBBLESTONE = new BasicBlock("cobblestone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            LOG = new BasicBlock("log", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 1), true, BoundingBox.Block);
            SAND = new BasicBlock("sand", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            LEAVES = new BasicBlock("leaves", false, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            GLASS = new BasicBlock("glass", false, false, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            ORE_COAL = new BasicBlock("ore_coal", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            ORE_IRON = new BasicBlock("ore_iron", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
            ORE_GOLD = new BasicBlock("ore_gold", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0), true, BoundingBox.Block);
        }

        #endregion

        public ushort Id { get;  }
        public string Name { get; }
        public bool IsFull { get; }
        public bool IsOpaque { get; }
        public bool IsSolid { get; }

        private BoundingBox boundingBox;

        /// <summary>
        /// This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to another non-opaque block.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; protected set; } = true;

        public Block(string name, bool isFull, bool isOpaque, bool isSolid, BoundingBox boundingBox)
        {
            Name = name;
            IsFull = isFull;
            IsOpaque = isOpaque;
            IsSolid = isSolid;

            this.boundingBox = boundingBox;

            if (blockDictionary.Count < 4096)
            {
                blockDictionary.Add((ushort)blockDictionary.Count, this);
                Id = (ushort)(blockDictionary.Count - 1);
            }
        }

        public virtual BoundingBox GetBoundingBox(int x, int y, int z)
        {
            return new BoundingBox(boundingBox.Center + new Vector3(x, y, z), boundingBox.Extents);
        }

        public abstract uint GetMesh(BlockSide side, ushort data, out float[] vertecies, out uint[] indicies);
    }
}
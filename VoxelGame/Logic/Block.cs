// <copyright file="Block.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract class Block
    {
        #region STATIC BLOCK MANAGMENT

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

        public static Dictionary<ushort, Block> blockDictionary = new Dictionary<ushort, Block>();

        public static void LoadBlocks()
        {
            AIR = new AirBlock("air");
            GRASS = new BasicBlock("grass", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 2));
            TALL_GRASS = new CrossBlock("tall_grass");
            DIRT = new BasicBlock("dirt", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            STONE = new BasicBlock("stone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            COBBLESTONE = new BasicBlock("cobblestone", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            LOG = new BasicBlock("log", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 1, 1));
            SAND = new BasicBlock("sand", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            LEAVES = new BasicBlock("leaves", false, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            GLASS = new BasicBlock("glass", false, false, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_COAL = new BasicBlock("ore_coal", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_IRON = new BasicBlock("ore_iron", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
            ORE_GOLD = new BasicBlock("ore_gold", true, true, new Tuple<int, int, int, int, int, int>(0, 0, 0, 0, 0, 0));
        }

        #endregion

        public ushort Id { get; private set; }
        public string Name { get; protected set; }
        public bool IsFull { get; protected set; }
        public bool IsOpaque { get; protected set; }

        /// <summary>
        /// This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to another non-opaque block.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; protected set; } = true;

        public Block(string name, bool isFull, bool isOpaque)
        {
            Name = name;
            IsFull = isFull;
            IsOpaque = isOpaque;

            if (blockDictionary.Count < 4096)
            {
                blockDictionary.Add((ushort)blockDictionary.Count, this);
                Id = (ushort)(blockDictionary.Count - 1);
            }
        }

        public abstract uint GetMesh(BlockSide side, ushort data, out float[] vertecies, out uint[] indicies);
    }
}
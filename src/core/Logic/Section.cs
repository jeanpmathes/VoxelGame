// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic
{
    [Serializable]
    public abstract class Section : IDisposable
    {
        public const int SectionSize = 16;

        public static readonly int SectionSizeExp = (int) Math.Log(Section.SectionSize, 2);
        public static readonly int SectionSizeExp2 = (int) Math.Log(Section.SectionSize, 2) * 2;

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public const int DataShift = 12;
        public const int LiquidShift = 18;
        public const int LevelShift = 23;
        public const int StaticShift = 26;
#pragma warning restore CA1707 // Identifiers should not contain underscores

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public const uint BlockMask = 0b0000_0000_0000_0000_0000_1111_1111_1111;
        public const uint DataMask = 0b0000_0000_0000_0011_1111_0000_0000_0000;
        public const uint LiquidMask = 0b0000_0000_0111_1100_0000_0000_0000_0000;
        public const uint LevelMask = 0b0000_0011_1000_0000_0000_0000_0000_0000;
        public const uint StaticMask = 0b0000_0100_0000_0000_0000_0000_0000_0000;
#pragma warning restore CA1707 // Identifiers should not contain underscores

        public static Vector3 Extents => new Vector3(SectionSize / 2f, SectionSize / 2f, SectionSize / 2f);

        [field: NonSerialized] protected World World { get; private set; } = null!;

#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly uint[] blocks;
#pragma warning restore CA1051 // Do not declare visible instance fields

        protected Section(World world)
        {
            blocks = new uint[SectionSize * SectionSize * SectionSize];
            Setup(world);
        }

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public void Setup(World world)
        {
            World = world;

            Setup();
        }

        protected abstract void Setup();

        public void SendRandomUpdates(int sectionX, int sectionY, int sectionZ)
        {
            uint val = GetPos(out int x, out int y, out int z);
            Decode(val, out Block block, out uint data, out _, out _, out _);

            block.RandomUpdate(
                World,
                x + (sectionX * SectionSize),
                y + (sectionY * SectionSize),
                z + (sectionZ * SectionSize),
                data);

            val = GetPos(out x, out y, out z);
            Decode(val, out _, out _, out Liquid liquid, out LiquidLevel level, out bool isStatic);

            liquid.RandomUpdate(
                World,
                x + (sectionX * SectionSize),
                y + (sectionY * SectionSize),
                z + (sectionZ * SectionSize),
                level,
                isStatic);

            uint GetPos(out int x, out int y, out int z)
            {
                int index = NumberGenerator.Random.Next(0, SectionSize * SectionSize * SectionSize);
                uint val = blocks[index];

                z = index & (SectionSize - 1);
                index = (index - z) >> SectionSizeExp;
                y = index & (SectionSize - 1);
                index = (index - y) >> SectionSizeExp;
                x = index;

                return val;
            }
        }

        /// <summary>
        /// Gets or sets the block at a section position.
        /// </summary>
        /// <param name="x">The x position of the block in this section.</param>
        /// <param name="y">The y position of the block in this section.</param>
        /// <param name="z">The z position of the block in this section.</param>
        /// <returns>The block at the given position.</returns>
        public uint this[int x, int y, int z]
        {
            get => blocks[(x << SectionSizeExp2) + (y << SectionSizeExp) + z];
            set => blocks[(x << SectionSizeExp2) + (y << SectionSizeExp) + z] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Decode(uint val, out Block block, out uint data, out Liquid liquid, out LiquidLevel level,
            out bool isStatic)
        {
            block = Block.TranslateID(val & BlockMask);
            data = (val & DataMask) >> DataShift;
            liquid = Liquid.TranslateID((val & LiquidMask) >> LiquidShift);
            level = (LiquidLevel) ((val & LevelMask) >> LevelShift);
            isStatic = (val & StaticMask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Encode(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic)
        {
            return (uint) ((((isStatic ? 1 : 0) << Section.StaticShift) & Section.StaticMask)
                           | (((uint) level << Section.LevelShift) & Section.LevelMask)
                           | ((liquid.Id << Section.LiquidShift) & Section.LiquidMask)
                           | ((data << Section.DataShift) & Section.DataMask)
                           | (block.Id & Section.BlockMask));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Block GetBlock(Vector3i position)
        {
            return Block.TranslateID(this[position.X, position.Y, position.Z] & BlockMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Block GetBlock(Vector3i position, out uint data)
        {
            uint val = this[position.X, position.Y, position.Z];

            data = (val << DataShift) & DataMask;

            return Block.TranslateID(val & BlockMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Liquid GetLiquid(Vector3i position, out int level)
        {
            uint val = this[position.X, position.Y, position.Z];

            level = (int) ((val & LevelMask) >> LevelShift);

            return Liquid.TranslateID((val & LiquidMask) >> LiquidShift);
        }

        #region IDisposable Support

        protected abstract void Dispose(bool disposing);

        ~Section()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
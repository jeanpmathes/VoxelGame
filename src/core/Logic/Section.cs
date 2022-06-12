// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A section, a part of a chunk. Sections are the smallest unit for meshing and rendering.
///     Sections are cubes.
/// </summary>
[Serializable]
public abstract class Section : IDisposable
{
    /// <summary>
    ///     The size of a section, which is the number of blocks in a single axis.
    /// </summary>
    public const int Size = 16;

    /// <summary>
    ///     The shift to get the data.
    /// </summary>
    public const int DataShift = 12;

    /// <summary>
    ///     The shift to get the fluid.
    /// </summary>
    public const int FluidShift = 18;

    /// <summary>
    ///     The shift to get the level.
    /// </summary>
    public const int LevelShift = 23;

    /// <summary>
    ///     The shift to get the isStatic value.
    /// </summary>
    public const int StaticShift = 26;

    /// <summary>
    ///     Mask to get only the block.
    /// </summary>
    public const uint BlockMask = 0b0000_0000_0000_0000_0000_1111_1111_1111;

    /// <summary>
    ///     Mask to get only the data.
    /// </summary>
    public const uint DataMask = 0b0000_0000_0000_0011_1111_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the fluid.
    /// </summary>
    public const uint FluidMask = 0b0000_0000_0111_1100_0000_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the level.
    /// </summary>
    public const uint LevelMask = 0b0000_0011_1000_0000_0000_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the isStatic value.
    /// </summary>
    public const uint StaticMask = 0b0000_0100_0000_0000_0000_0000_0000_0000;

    /// <summary>
    ///     Integer result of <c>lb(SectionSize)</c>.
    /// </summary>
    public static readonly int SizeExp = (int) Math.Log(Size, newBase: 2);

    /// <summary>
    ///     Integer result of <c>lb(SectionSize) * 2</c>.
    /// </summary>
    public static readonly int SizeExp2 = (int) Math.Log(Size, newBase: 2) * 2;

    /// <summary>
    ///     The blocks stored in this section.
    /// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields
    protected readonly uint[] blocks;
#pragma warning restore CA1051 // Do not declare visible instance fields

    /// <summary>
    ///     Creates a new section.
    /// </summary>
    /// <param name="world">The world.</param>
    [SuppressMessage(
        "ReSharper.DPA",
        "DPA0002: Excessive memory allocations in SOH",
        MessageId = "type: System.UInt32[]")]
    protected Section(World world)
    {
        blocks = new uint[Size * Size * Size];
        Setup(world);
    }

    /// <summary>
    ///     The extents of a section.
    /// </summary>
    public static Vector3d Extents => new(Size / 2f, Size / 2f, Size / 2f);

    /// <summary>
    ///     The world this section is in.
    /// </summary>
    [field: NonSerialized] protected World World { get; private set; } = null!;

    /// <summary>
    ///     Gets the content at a section position.
    /// </summary>
    /// <param name="x">The x position of the block data in this section.</param>
    /// <param name="y">The y position of the block data in this section.</param>
    /// <param name="z">The z position of the block data in this section.</param>
    /// <returns>The block data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetContent(int x, int y, int z)
    {
        return blocks[(x << SizeExp2) + (y << SizeExp) + z];
    }

    /// <summary>
    ///     Get the content at a world position.
    /// </summary>
    /// <param name="position">The world position. Must be in the section.</param>
    /// <returns>The content at the given position.</returns>
    public uint GetContent(Vector3i position)
    {
        return GetContent(position.X & (Size - 1), position.Y & (Size - 1), position.Z & (Size - 1));
    }

    /// <summary>
    ///     Gets or sets the content at a section position.
    /// </summary>
    /// <param name="x">The x position of the block data in this section.</param>
    /// <param name="y">The y position of the block data in this section.</param>
    /// <param name="z">The z position of the block data in this section.</param>
    /// <param name="data">The data to set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetContent(int x, int y, int z, uint data)
    {
        blocks[(x << SizeExp2) + (y << SizeExp) + z] = data;
    }

    /// <summary>
    ///     Set the content at a world position.
    /// </summary>
    /// <param name="position">The world position. Must be in the section.</param>
    /// <param name="value">The value to set at the specified position.</param>
    public void SetContent(Vector3i position, uint value)
    {
        SetContent(position.X & (Size - 1), position.Y & (Size - 1), position.Z & (Size - 1), value);
    }

    /// <summary>
    ///     Sets up all non serialized members.
    /// </summary>
    public void Setup(World world)
    {
        World = world;

        Setup();
    }

    /// <summary>
    ///     Setup the section after serialization.
    /// </summary>
    protected abstract void Setup();

    /// <summary>
    ///     Send random updates to blocks in this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    public void SendRandomUpdates(SectionPosition position)
    {
        uint val = GetPos(out Vector3i selectedPosition);
        Decode(val, out Block block, out uint data, out _, out _, out _);

        Vector3i blockPosition = selectedPosition + position.FirstBlock;

        block.RandomUpdate(
            World,
            blockPosition,
            data);

        val = GetPos(out selectedPosition);
        Decode(val, out _, out _, out Fluid fluid, out FluidLevel level, out bool isStatic);

        Vector3i fluidPosition = selectedPosition + position.FirstBlock;

        fluid.RandomUpdate(
            World,
            fluidPosition,
            level,
            isStatic);

        uint GetPos(out Vector3i randomPosition)
        {
            int index = NumberGenerator.Random.Next(minValue: 0, Size * Size * Size);
            uint posVal = blocks[index];

            randomPosition.Z = index & (Size - 1);
            index = (index - randomPosition.Z) >> SizeExp;
            randomPosition.Y = index & (Size - 1);
            index = (index - randomPosition.Y) >> SizeExp;
            randomPosition.X = index;

            return posVal;
        }
    }

    /// <summary>
    ///     Decode the section content into block and fluid information.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decode(uint val, out Block block, out uint data, out Fluid fluid, out FluidLevel level,
        out bool isStatic)
    {
        block = Block.TranslateID(val & BlockMask);
        data = (val & DataMask) >> DataShift;
        fluid = Fluid.TranslateID((val & FluidMask) >> FluidShift);
        level = (FluidLevel) ((val & LevelMask) >> LevelShift);
        isStatic = (val & StaticMask) != 0;
    }

    /// <summary>
    ///     Encode block and fluid information into section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Encode(Block block, uint data, Fluid fluid, FluidLevel level, bool isStatic)
    {
        return (uint) ((((isStatic ? 1 : 0) << StaticShift) & StaticMask)
                       | (((uint) level << LevelShift) & LevelMask)
                       | ((fluid.Id << FluidShift) & FluidMask)
                       | ((data << DataShift) & DataMask)
                       | (block.Id & BlockMask));
    }

    /// <summary>
    ///     Get the block at a given section position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The block at the position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block GetBlock(Vector3i position)
    {
        return Block.TranslateID(GetContent(position.X, position.Y, position.Z) & BlockMask);
    }

    /// <summary>
    ///     Get the block at a given section position, and the associated data.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="data">The data of the block.</param>
    /// <returns>The block at the position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block GetBlock(Vector3i position, out uint data)
    {
        uint val = GetContent(position.X, position.Y, position.Z);

        data = (val << DataShift) & DataMask;

        return Block.TranslateID(val & BlockMask);
    }

    /// <summary>
    ///     Get the fluid at a given section position.
    /// </summary>
    /// <param name="position">The section position.</param>
    /// <param name="level">The level of the fluid as int.</param>
    /// <returns>The fluid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fluid GetFluid(Vector3i position, out int level)
    {
        uint val = GetContent(position.X, position.Y, position.Z);

        level = (int) ((val & LevelMask) >> LevelShift);

        return Fluid.TranslateID((val & FluidMask) >> FluidShift);
    }

    #region IDisposable Support

    /// <summary>
    ///     Dispose of the section.
    /// </summary>
    /// <param name="disposing">Whether disposing is intentional or caused by GC.</param>
    protected abstract void Dispose(bool disposing);

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Section()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the section.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}

// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    ///     Creates a new section.
    /// </summary>
    protected Section(SectionPosition position)
    {
        blocks = new uint[Size * Size * Size];
        this.position = position;
    }

    /// <summary>
    ///     The extents of a section.
    /// </summary>
    public static Vector3d Extents => new(Size / 2f, Size / 2f, Size / 2f);

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
        Throw.IfDisposed(disposed);

        return blocks[(x << SizeExp2) + (y << SizeExp) + z];
    }

    /// <summary>
    ///     Get the content at a world position.
    /// </summary>
    /// <param name="blockPosition">The world position. Must be in the section.</param>
    /// <returns>The content at the given position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetContent(Vector3i blockPosition)
    {
        Throw.IfDisposed(disposed);

        return GetContent(blockPosition.X & (Size - 1), blockPosition.Y & (Size - 1), blockPosition.Z & (Size - 1));
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
        Throw.IfDisposed(disposed);

        blocks[(x << SizeExp2) + (y << SizeExp) + z] = data;
    }

    /// <summary>
    ///     Set the content at a world position.
    /// </summary>
    /// <param name="blockPosition">The world position. Must be in the section.</param>
    /// <param name="value">The value to set at the specified position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetContent(Vector3i blockPosition, uint value)
    {
        Throw.IfDisposed(disposed);

        SetContent(blockPosition.X & (Size - 1), blockPosition.Y & (Size - 1), blockPosition.Z & (Size - 1), value);
    }

    /// <summary>
    ///     Get the local 3D-index of a block for a world position.
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns>The local 3D-index.</returns>
    public static (int x, int y, int z) ToLocalPosition(Vector3i worldPosition)
    {
        return (worldPosition.X & (Size - 1), worldPosition.Y & (Size - 1), worldPosition.Z & (Size - 1));
    }

    /// <summary>
    ///     Check whether a local position is in bounds.
    /// </summary>
    /// <param name="localPosition">The local position.</param>
    /// <returns>Whether the position is in bounds.</returns>
    public static bool IsInBounds((int x, int y, int z) localPosition)
    {
        var inBounds = true;

        inBounds &= localPosition.x is >= 0 and < Size;
        inBounds &= localPosition.y is >= 0 and < Size;
        inBounds &= localPosition.z is >= 0 and < Size;

        return inBounds;
    }

    /// <summary>
    ///     Setup the section after serialization.
    /// </summary>
    public abstract void Setup(Section loaded);

    /// <summary>
    ///     Send random updates to blocks in this section.
    /// </summary>
    /// <param name="world">The world this section is in.</param>
    public void SendRandomUpdates(World world)
    {
        Throw.IfDisposed(disposed);

        uint val = GetPos(out Vector3i selectedPosition);
        Decode(val, out Block block, out uint data, out _, out _, out _);

        Vector3i blockPosition = selectedPosition + position.FirstBlock;

        block.RandomUpdate(
            world,
            blockPosition,
            data);

        val = GetPos(out selectedPosition);
        Decode(val, out _, out _, out Fluid fluid, out FluidLevel level, out bool isStatic);

        Vector3i fluidPosition = selectedPosition + position.FirstBlock;

        fluid.RandomUpdate(
            world,
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
        block = Blocks.Instance.TranslateID(val & BlockMask);
        data = (val & DataMask) >> DataShift;
        fluid = Fluids.Instance.TranslateID((val & FluidMask) >> FluidShift);
        level = (FluidLevel) ((val & LevelMask) >> LevelShift);
        isStatic = (val & StaticMask) != 0;
    }

    /// <summary>
    ///     Decode the section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decode(uint val, out Content content)
    {
        Decode(val, out Block block, out uint data, out Fluid fluid, out FluidLevel level, out bool isStatic);

        content = new Content(block.AsInstance(data), fluid.AsInstance(level, isStatic));
    }

    /// <summary>
    ///     Encode block and fluid information into section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Encode(IBlockBase block, uint data, Fluid fluid, FluidLevel level, bool isStatic)
    {
        return (uint) ((((isStatic ? 1 : 0) << StaticShift) & StaticMask)
                       | (((uint) level << LevelShift) & LevelMask)
                       | ((fluid.ID << FluidShift) & FluidMask)
                       | ((data << DataShift) & DataMask)
                       | (block.ID & BlockMask));
    }

    /// <summary>
    ///     Encode world content information into section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Encode(in Content content)
    {
        return Encode(content.Block.Block, content.Block.Data, content.Fluid.Fluid, content.Fluid.Level, content.Fluid.IsStatic);
    }

    /// <summary>
    ///     Encode block and fluid information into section content, with defaults for all values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Encode(IBlockBase? block = null, Fluid? fluid = null)
    {
        return Encode(block ?? Blocks.Instance.Air, data: 0, fluid ?? Fluids.Instance.None, FluidLevel.Eight, isStatic: true);
    }

    /// <summary>
    ///     Get the block at a given section position.
    /// </summary>
    /// <param name="blockPosition">The position.</param>
    /// <returns>The block at the position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance GetBlock(Vector3i blockPosition)
    {
        Throw.IfDisposed(disposed);

        uint val = GetContent(blockPosition.X, blockPosition.Y, blockPosition.Z);

        uint data = (val & DataMask) >> DataShift;

        return Blocks.Instance.TranslateID(val & BlockMask).AsInstance(data);
    }

    /// <summary>
    ///     Get the fluid at a given section position.
    /// </summary>
    /// <param name="blockPosition">The section position.</param>
    /// <returns>The fluid. It is always assumed to by static.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluidInstance GetFluid(Vector3i blockPosition)
    {
        Throw.IfDisposed(disposed);

        uint val = GetContent(blockPosition.X, blockPosition.Y, blockPosition.Z);

        var level = (FluidLevel) ((val & LevelMask) >> LevelShift);

        return Fluids.Instance.TranslateID((val & FluidMask) >> FluidShift).AsInstance(level);
    }

#pragma warning disable CA1051 // Do not declare visible instance fields
    /// <summary>
    ///     The blocks stored in this section.
    /// </summary>
    protected uint[] blocks;

    /// <summary>
    ///     The position of this section.
    /// </summary>
    [NonSerialized] protected SectionPosition position;
#pragma warning restore CA1051 // Do not declare visible instance fields

    #region IDisposable Support

    /// <summary>
    ///     Whether the section is disposed.
    /// </summary>
    [NonSerialized] protected bool disposed;

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

// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Sections;

/// <summary>
///     A section, a part of a chunk. Sections are the smallest unit for meshing and rendering.
///     Sections are cubes.
/// </summary>
public class Section : IDisposable
{
    /// <summary>
    ///     The size of a section, which is the number of blocks in a single axis.
    /// </summary>
    public const Int32 Size = 16;

    /// <summary>
    /// Number of entries in a section.
    /// </summary>
    public const Int32 Count = Size * Size * Size;

    /// <summary>
    ///     The shift to get the data.
    /// </summary>
    public const Int32 DataShift = 12;

    /// <summary>
    ///     The shift to get the fluid.
    /// </summary>
    public const Int32 FluidShift = 18;

    /// <summary>
    ///     The shift to get the level.
    /// </summary>
    public const Int32 LevelShift = 23;

    /// <summary>
    ///     The shift to get the isStatic value.
    /// </summary>
    public const Int32 StaticShift = 26;

    /// <summary>
    ///     Mask to get only the block.
    /// </summary>
    public const UInt32 BlockMask = 0b0000_0000_0000_0000_0000_1111_1111_1111;

    /// <summary>
    ///     Mask to get only the data.
    /// </summary>
    public const UInt32 DataMask = 0b0000_0000_0000_0011_1111_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the fluid.
    /// </summary>
    public const UInt32 FluidMask = 0b0000_0000_0111_1100_0000_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the level.
    /// </summary>
    public const UInt32 LevelMask = 0b0000_0011_1000_0000_0000_0000_0000_0000;

    /// <summary>
    ///     Mask to get only the isStatic value.
    /// </summary>
    public const UInt32 StaticMask = 0b0000_0100_0000_0000_0000_0000_0000_0000;

    /// <summary>
    ///     Integer result of <c>lb(SectionSize)</c>.
    /// </summary>
    public static readonly Int32 SizeExp = BitOperations.Log2(Size);

    /// <summary>
    ///     Integer result of <c>lb(SectionSize) * 2</c>.
    /// </summary>
    public static readonly Int32 SizeExp2 = SizeExp * 2;

    /// <summary>
    ///     Creates a new section.
    /// </summary>
    protected Section(ArraySegment<UInt32> blocks)
    {
        Debug.Assert(blocks.Count == Count);

        this.blocks = blocks;
    }

    /// <summary>
    ///     The extents of a section.
    /// </summary>
    public static Vector3d Extents => new(Size / 2f, Size / 2f, Size / 2f);

    /// <summary>
    ///     Initializes the section.
    /// </summary>
    /// <param name="newPosition">The position of the section.</param>
    public virtual void Initialize(SectionPosition newPosition)
    {
        position = newPosition;
    }

    /// <summary>
    ///     Reset the section, preparing it for reuse.
    /// </summary>
    public virtual void Reset()
    {
        // Nothing to do, block can stay as re-use will overwrite the content.
    }

    /// <summary>
    ///     Gets the content at a section position.
    /// </summary>
    /// <param name="x">The x position of the block data in this section.</param>
    /// <param name="y">The y position of the block data in this section.</param>
    /// <param name="z">The z position of the block data in this section.</param>
    /// <returns>The block data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt32 GetContent(Int32 x, Int32 y, Int32 z)
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
    public UInt32 GetContent(Vector3i blockPosition)
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
    public void SetContent(Int32 x, Int32 y, Int32 z, UInt32 data)
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
    public void SetContent(Vector3i blockPosition, UInt32 value)
    {
        Throw.IfDisposed(disposed);

        SetContent(blockPosition.X & (Size - 1), blockPosition.Y & (Size - 1), blockPosition.Z & (Size - 1), value);
    }

    /// <summary>
    ///     Get the local 3D-index of a block for a world position.
    /// </summary>
    /// <param name="worldPosition">The world position.</param>
    /// <returns>The local 3D-index.</returns>
    public static (Int32 x, Int32 y, Int32 z) ToLocalPosition(Vector3i worldPosition)
    {
        return (worldPosition.X & (Size - 1), worldPosition.Y & (Size - 1), worldPosition.Z & (Size - 1));
    }

    /// <summary>
    ///     Check whether a local position is in bounds.
    /// </summary>
    /// <param name="localPosition">The local position.</param>
    /// <returns>Whether the position is in bounds.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsInBounds((Int32 x, Int32 y, Int32 z) localPosition)
    {
        return IsInBounds(localPosition.x, localPosition.y, localPosition.z);
    }

    /// <summary>
    ///     Check whether a position is in bounds.
    /// </summary>
    /// <param name="x">The x component of the local position.</param>
    /// <param name="y">The y component of the local position.</param>
    /// <param name="z">The z component of the local position.</param>
    /// <returns>Whether the position is in bounds.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean IsInBounds(Int32 x, Int32 y, Int32 z)
    {
        var inBounds = true;

        inBounds &= x is >= 0 and < Size;
        inBounds &= y is >= 0 and < Size;
        inBounds &= z is >= 0 and < Size;

        return inBounds;
    }

    /// <summary>
    ///     Send one random update to a random block and fluid in this section.
    /// </summary>
    /// <param name="world">The world this section is in.</param>
    public void SendRandomUpdate(World world)
    {
        Throw.IfDisposed(disposed);

        UInt32 content = GetRandomPositionContent(out Vector3i localPosition);

        Decode(content,
            out Block block,
            out UInt32 data,
            out Fluid fluid,
            out FluidLevel level,
            out Boolean isStatic);

        Vector3i globalPosition = localPosition + position.FirstBlock;

        block.RandomUpdate(
            world,
            globalPosition,
            data);

        fluid.RandomUpdate(
            world,
            globalPosition,
            level,
            isStatic);

        UInt32 GetRandomPositionContent(out Vector3i randomPosition)
        {
            Int32 index = NumberGenerator.Random.Next(minValue: 0, Size * Size * Size);
            UInt32 posVal = blocks[index];

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
    public static void Decode(UInt32 value,
        out Block block, out UInt32 data,
        out Fluid fluid, out FluidLevel level, out Boolean isStatic)
    {
        block = Blocks.Instance.TranslateID(value & BlockMask);
        data = (value & DataMask) >> DataShift;
        fluid = Fluids.Instance.TranslateID((value & FluidMask) >> FluidShift);
        level = (FluidLevel) ((value & LevelMask) >> LevelShift);
        isStatic = (value & StaticMask) != 0;
    }

    /// <summary>
    ///     Decode the section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decode(UInt32 value, out Content content)
    {
        Decode(value, out Block block, out UInt32 data, out Fluid fluid, out FluidLevel level, out Boolean isStatic);

        content = new Content(block.AsInstance(data), fluid.AsInstance(level, isStatic));
    }

    /// <summary>
    ///     Encode block and fluid information into section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt32 Encode(IBlockBase block, UInt32 data, Fluid fluid, FluidLevel level, Boolean isStatic)
    {
        return (UInt32) ((((isStatic ? 1 : 0) << StaticShift) & StaticMask)
                         | (((UInt32) level << LevelShift) & LevelMask)
                         | ((fluid.ID << FluidShift) & FluidMask)
                         | ((data << DataShift) & DataMask)
                         | (block.ID & BlockMask));
    }

    /// <summary>
    ///     Encode world content information into section content.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt32 Encode(in Content content)
    {
        return Encode(content.Block.Block, content.Block.Data, content.Fluid.Fluid, content.Fluid.Level, content.Fluid.IsStatic);
    }

    /// <summary>
    ///     Encode block and fluid information into section content, with defaults for all values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt32 Encode(IBlockBase? block = null, Fluid? fluid = null)
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

        UInt32 val = GetContent(blockPosition.X, blockPosition.Y, blockPosition.Z);

        UInt32 data = (val & DataMask) >> DataShift;

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

        UInt32 val = GetContent(blockPosition.X, blockPosition.Y, blockPosition.Z);

        var level = (FluidLevel) ((val & LevelMask) >> LevelShift);

        return Fluids.Instance.TranslateID((val & FluidMask) >> FluidShift).AsInstance(level);
    }

#pragma warning disable CA1051 // Do not declare visible instance fields
    /// <summary>
    ///     The blocks stored in this section.
    /// </summary>
    protected ArraySegment<UInt32> blocks;

    /// <summary>
    ///     The position of this section.
    /// </summary>
    protected SectionPosition position;
#pragma warning restore CA1051 // Do not declare visible instance fields

    #region IDisposable Support

    /// <summary>
    ///     Whether the section is disposed.
    /// </summary>
    private Boolean disposed;

    /// <summary>
    ///     Dispose of the section.
    /// </summary>
    /// <param name="disposing">Whether disposing is intentional or caused by GC.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        disposed = true;
    }

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

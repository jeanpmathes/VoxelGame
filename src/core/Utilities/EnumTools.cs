// <copyright file="EnumTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Enumeration utilities.
/// </summary>
public static class EnumTools
{
    /// <summary>
    ///     Check whether the given enum type is a flags enum.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to check.</typeparam>
    /// <returns><c>true</c> if the enum type is a flags enum; otherwise, <c>false</c>.</returns>
    public static Boolean IsFlagsEnum<TEnum>() where TEnum : struct, Enum
    {
        return typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false);
    }

    /// <summary>
    ///     Get the number of distinct positions in the given flags type.
    ///     This is also the number of bits required to represent the flags values.
    /// </summary>
    /// <typeparam name="TEnum">The flags enum type.</typeparam>
    /// <returns>The number of distinct values in the flags enum.</returns>
    public static Int32 CountFlags<TEnum>() where TEnum : struct, Enum
    {
        Debug.Assert(IsFlagsEnum<TEnum>());

        UInt64 max = 0;

        foreach (TEnum flag in Enum.GetValues<TEnum>())
        {
            UInt64 value = GetUnsignedValue(flag);
            max = Math.Max(max, value);
        }

        return BitTools.MostSignificantBit(max) + 1;
    }
    
    /// <summary>
    /// Get a list of all named positions in the given flags enum type.
    /// A named position is a defined value in the enum that has exactly one bit set.
    /// </summary>
    /// <typeparam name="TEnum">The flags enum type.</typeparam>
    /// <returns>The list of named positions in the flags enum.</returns>
    public static IEnumerable<(String name, TEnum value)> GetPositions<TEnum>() where TEnum : struct, Enum
    {
        Debug.Assert(IsFlagsEnum<TEnum>());

        return PositionCache<TEnum>.Value;
    }
    
    private static class PositionCache<TEnum> where TEnum : struct, Enum
    {
        public static IEnumerable<(String name, TEnum value)> Value { get; } = GetPositions();

        private static List<(String name, TEnum value)> GetPositions()
        {
            List<(String name, TEnum value)> positions = [];
            
            foreach (TEnum flag in Enum.GetValues<TEnum>())
            {
                UInt64 value = GetUnsignedValue(flag);

                if (BitTools.CountSetBits(value) != 1) 
                    continue;

                String name = Enum.GetName(flag) ?? "Unknown";
                positions.Add((name, flag));
            }
            
            return positions;
        }
    }

    /// <summary>
    ///     Get the unsigned integer value of the given enum.
    /// </summary>
    /// <param name="e">The enum value to get the unsigned value of.</param>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>The unsigned integer value of the enum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt64 GetUnsignedValue<TEnum>(TEnum e) where TEnum : struct, Enum
    {
        UInt64 value;

        if (Unsafe.SizeOf<TEnum>() == sizeof(UInt32)) value = Unsafe.As<TEnum, UInt32>(ref e);
        else if (Unsafe.SizeOf<TEnum>() == sizeof(Byte)) value = Unsafe.As<TEnum, Byte>(ref e);
        else if (Unsafe.SizeOf<TEnum>() == sizeof(UInt16)) value = Unsafe.As<TEnum, UInt16>(ref e);
        else if (Unsafe.SizeOf<TEnum>() == sizeof(UInt64)) value = Unsafe.As<TEnum, UInt64>(ref e);
        else throw Exceptions.UnsupportedEnumValue(e);

        return value;
    }

    /// <summary>
    ///     Get the enum value from the given unsigned integer value.
    /// </summary>
    /// <param name="v64">The unsigned integer value to convert to an enum.</param>
    /// <typeparam name="TEnum">The enum type to convert to.</typeparam>
    /// <returns>The enum value corresponding to the given unsigned integer value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum GetEnumValue<TEnum>(UInt64 v64) where TEnum : struct, Enum
    {
        if (Unsafe.SizeOf<TEnum>() == sizeof(UInt32))
        {
            var v32 = (UInt32) v64;

            return Unsafe.As<UInt32, TEnum>(ref v32);
        }

        if (Unsafe.SizeOf<TEnum>() == sizeof(Byte))
        {
            var v8 = (Byte) v64;

            return Unsafe.As<Byte, TEnum>(ref v8);
        }

        if (Unsafe.SizeOf<TEnum>() == sizeof(UInt16))
        {
            var v16 = (UInt16) v64;

            return Unsafe.As<UInt16, TEnum>(ref v16);
        }

        if (Unsafe.SizeOf<TEnum>() == sizeof(UInt64)) return Unsafe.As<UInt64, TEnum>(ref v64);

        throw Exceptions.UnsupportedValue(v64);
    }
}

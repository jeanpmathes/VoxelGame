// <copyright file="IntegerConstant.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
///     Wraps an integer constant for comparison, e.g. for enum values.
/// </summary>
public readonly struct IntegerConstant : IEquatable<IntegerConstant>
{
    private readonly Boolean isUnsigned;
    private readonly UInt64 unsignedData;
    private readonly Int64 signedData;

    private IntegerConstant(Boolean isUnsigned, UInt64 unsignedData = 0, Int64 signedData = 0)
    {
        this.isUnsigned = isUnsigned;
        this.unsignedData = unsignedData;
        this.signedData = signedData;
    }

    /// <summary>
    ///     Whether the value is a flag value, meaning it is a power of two or zero.
    /// </summary>
    public Boolean IsFlag => isUnsigned
        ? (unsignedData & (unsignedData - 1)) == 0
        : (signedData & (signedData - 1)) == 0;

    /// <summary>
    ///     Creates an <see cref="IntegerConstant" /> from the given underlying type and value.
    /// </summary>
    /// <param name="underlying">The underlying type of the enum.</param>
    /// <param name="value">The value of the enum.</param>
    /// <returns>>The created <see cref="IntegerConstant" />.</returns>
    public static IntegerConstant From(ITypeSymbol? underlying, Object value)
    {
        SpecialType st = (underlying as INamedTypeSymbol)?.SpecialType ?? SpecialType.System_Int32;

        return st switch
        {
            SpecialType.System_SByte => new IntegerConstant(isUnsigned: false, signedData: (SByte) value),
            SpecialType.System_Int16 => new IntegerConstant(isUnsigned: false, signedData: (Int16) value),
            SpecialType.System_Int32 => new IntegerConstant(isUnsigned: false, signedData: (Int32) value),
            SpecialType.System_Int64 => new IntegerConstant(isUnsigned: false, signedData: (Int64) value),
            SpecialType.System_Byte => new IntegerConstant(isUnsigned: true, (Byte) value),
            SpecialType.System_UInt16 => new IntegerConstant(isUnsigned: true, (UInt16) value),
            SpecialType.System_UInt32 => new IntegerConstant(isUnsigned: true, (UInt32) value),
            SpecialType.System_UInt64 => new IntegerConstant(isUnsigned: true, (UInt64) value),
            _ => new IntegerConstant(isUnsigned: false, signedData: Convert.ToInt64(value, CultureInfo.InvariantCulture))
        };
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(IntegerConstant other)
    {
        return isUnsigned == other.isUnsigned && (isUnsigned ? unsignedData == other.unsignedData : signedData == other.signedData);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is IntegerConstant o && Equals(o);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        unchecked
        {
            Int32 hashCode = isUnsigned.GetHashCode();
            hashCode = (hashCode * 397) ^ unsignedData.GetHashCode();
            hashCode = (hashCode * 397) ^ signedData.GetHashCode();

            return hashCode;
        }
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(IntegerConstant left, IntegerConstant right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(IntegerConstant left, IntegerConstant right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}

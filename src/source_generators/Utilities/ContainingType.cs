// <copyright file="ContainingType.cs" company="VoxelGame">
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

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
///     Class to represent the containing type of declarations, able to handle nested types.
/// </summary>
public class ContainingType(String accessibility, String keyword, String name, String? typeParameters, String constraints, ContainingType? child)
{
    /// <summary>
    ///     The child containing type, if any.
    /// </summary>
    public ContainingType? Child { get; } = child;

    /// <summary>
    ///     The accessibility of the containing type (e.g., <c>public</c>, <c>internal</c>).
    /// </summary>
    public String Accessibility { get; } = accessibility;

    /// <summary>
    ///     The keyword of the containing type (e.g., <c>class</c>, <c>struct</c>).
    /// </summary>
    public String Keyword { get; } = keyword;

    /// <summary>
    ///     The name of the containing type.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     The type parameters of the containing type, if any (including angle brackets).
    /// </summary>
    public String? TypeParameters { get; } = typeParameters;

    /// <summary>
    ///     The constraints of the containing type, if any.
    /// </summary>
    public String Constraints { get; } = constraints;

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is not ContainingType other)
            return false;

        if (obj == this)
            return true;

        return Accessibility == other.Accessibility
               && Keyword == other.Keyword
               && Name == other.Name
               && TypeParameters == other.TypeParameters
               && Constraints == other.Constraints
               && Equals(Child, other.Child);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        unchecked
        {
            Int32 hashCode = Accessibility.GetHashCode();
            hashCode = (hashCode * 397) ^ Keyword.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ (TypeParameters != null ? TypeParameters.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Constraints.GetHashCode();
            hashCode = (hashCode * 397) ^ (Child != null ? Child.GetHashCode() : 0);

            return hashCode;
        }
    }
}

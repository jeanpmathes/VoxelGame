// <copyright file="GenerateRecordAttribute.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Marks an interface so a record implementation is generated.
///     Optionally accepts a base type. If the type is generic with a single type parameter,
///     the generated record type will be substituted as the type argument.
///     If the type is non-generic, it is implemented as-is. The parameter may be omitted.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class GenerateRecordAttribute : Attribute
{
    /// <summary>
    ///     Initializes the attribute with a base type.
    ///     If the type has exactly one generic parameter, the generated record will be supplied as the argument.
    /// </summary>
    public GenerateRecordAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    /// <summary>
    ///     Optional base type to implement in addition to the marked interface.
    ///     Can be a non-generic type or a generic type with exactly one type parameter.
    /// </summary>
    public Type? BaseType { get; }
}

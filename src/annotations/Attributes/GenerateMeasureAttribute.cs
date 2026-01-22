// <copyright file="GenerateMeasureAttribute.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Definitions;

namespace VoxelGame.Annotations.Attributes;

/// <summary>
///     Specifies that a measure type should be generated for a unit property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class GenerateMeasureAttribute : Attribute
{
    /// <summary>
    ///     Initialize a new instance of the <see cref="GenerateMeasureAttribute" /> class.
    /// </summary>
    /// <param name="measureName">The name of the generated measure type.</param>
    /// <param name="valuePropertyName">The name of the property exposing the value in base units.</param>
    /// <param name="prefixes">The prefixes that are supported by the measure.</param>
    public GenerateMeasureAttribute(String measureName, String valuePropertyName, AllowedPrefixes prefixes)
    {
        MeasureName = measureName;
        ValuePropertyName = valuePropertyName;
        Prefixes = prefixes;
    }

    /// <summary>
    ///     Gets the name of the generated measure type.
    /// </summary>
    public String MeasureName { get; }

    /// <summary>
    ///     Gets the name of the property exposing the value in base units.
    /// </summary>
    public String ValuePropertyName { get; }

    /// <summary>
    ///     Gets the prefixes that are supported by the measure.
    /// </summary>
    public AllowedPrefixes Prefixes { get; }

    /// <summary>
    ///     Gets or sets the summary text for the generated measure type.
    /// </summary>
    public String? MeasureSummary { get; set; }

    /// <summary>
    ///     Gets or sets the summary text for the generated value property.
    /// </summary>
    public String? ValueSummary { get; set; }
}

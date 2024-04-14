// <copyright file="Measure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A measure property, which contains a value that is associated with a unit.
/// </summary>
public class Measure : Property
{
    /// <summary>
    ///     Create a new measure property.
    /// </summary>
    public Measure(String name, IMeasure value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    ///     The value of the measure.
    /// </summary>
    public IMeasure Value { get; set; }

    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

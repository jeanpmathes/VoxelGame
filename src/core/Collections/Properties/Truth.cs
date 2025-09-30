// <copyright file="Truth.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A property that represents a truth value.
/// </summary>
public class Truth : Property
{
    /// <summary>
    ///     Creates a <see cref="Truth" /> with a name and a boolean value.
    /// </summary>
    /// <param name="name">The name of the truth property.</param>
    /// <param name="value">The boolean value of the truth property.</param>
    public Truth(String name, Boolean value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    ///     Get the boolean value of the truth property.
    /// </summary>
    public Boolean Value { get; }

    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

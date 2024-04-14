// <copyright file="Integer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     Property that holds an integer value.
/// </summary>
public class Integer : Property
{
    /// <summary>
    ///     Create a new <see cref="Integer" />.
    /// </summary>
    public Integer(String name, BigInteger value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    ///     The value of the <see cref="Integer" />.
    /// </summary>
    public BigInteger Value { get; }

    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

// <copyright file="Color.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
/// A property that represents a color value.
/// </summary>
public class Color : Property
{
    /// <summary>
    /// Creates a <see cref="Color"/> with a name and a color value.
    /// </summary>
    /// <param name="name">The name of the color property.</param>
    /// <param name="value">The color value of the color property.</param>
    public Color(String name, ColorS value) : base(name)
    {
        Value = value;
    }
    
    /// <summary>
    /// Get the color value of the color property.
    /// </summary>
    public ColorS Value { get; }
    
    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

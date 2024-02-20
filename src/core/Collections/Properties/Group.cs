// <copyright file="Group.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections;
using System.Collections.Generic;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A group combines multiple properties into a single unit.
/// </summary>
public class Group : Property, IEnumerable<Property>
{
    private readonly List<Property> children = new();

    /// <summary>
    ///     Creates a new group with the given name and children.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="children">The children of the group.</param>
    public Group(string name, IEnumerable<Property>? children = null) : base(name)
    {
        if (children != null)
            this.children.AddRange(children);
    }

    /// <inheritdoc />
    public IEnumerator<Property> GetEnumerator()
    {
        return children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Add a child to the group.
    /// </summary>
    /// <param name="property">The child to add.</param>
    public void Add(Property property)
    {
        children.Add(property);
    }

    internal sealed override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

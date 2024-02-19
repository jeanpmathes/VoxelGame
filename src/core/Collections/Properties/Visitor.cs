﻿// <copyright file="Visitor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A visitor for the property collection.
/// </summary>
public class Visitor
{
    /// <summary>
    ///     Creates a new visitor.
    /// </summary>
    protected Visitor() {}

    /// <exclude />
    public void Visit(Property property)
    {
        property.Accept(this);
    }

    /// <exclude />
    public virtual void Visit(Group group)
    {
        foreach (Property child in group) Visit(child);
    }

    /// <exclude />
    public virtual void Visit(Error error)
    {
        // Nothing to do here.
    }
}

// <copyright file="Chainable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Manual.Elements;
using VoxelGame.Manual.Modifiers;
using Boolean = VoxelGame.Manual.Elements.Boolean;

namespace VoxelGame.Manual;

/// <summary>
///     Allows the simple creating of ordered elements.
/// </summary>
public abstract class Chainable
{
    internal abstract void AddElement(IElement element);

    /// <summary>
    ///     Add text content to the section.
    /// </summary>
    /// <param name="content">The text content to add.</param>
    /// <param name="style">The style of the text to add.</param>
    /// <returns>The current chain.</returns>
    public Chainable Text(String content, TextStyle style = TextStyle.Normal)
    {
        AddElement(new Text(content, style));

        return this;
    }

    /// <summary>
    ///     Add a key box to the section.
    /// </summary>
    /// <param name="k">The key to describe.</param>
    /// <returns>The current chain.</returns>
    public Chainable Key(Object k)
    {
        AddElement(new Key(k));

        return this;
    }

    /// <summary>
    ///     Start a new line.
    /// </summary>
    /// <returns>This.</returns>
    public Chainable NewLine()
    {
        AddElement(new NewLine());

        return this;
    }

    /// <summary>
    ///     Add a representation of a boolean value.
    /// </summary>
    /// <param name="value">The value to represent.</param>
    /// <returns>This.</returns>
    public Chainable Boolean(System.Boolean value)
    {
        AddElement(new Boolean(value));

        return this;
    }

    /// <summary>
    ///     Add a list to the section.
    /// </summary>
    /// <param name="builder">The builder for the list.</param>
    /// <returns>This.</returns>
    public Chainable List(Func<Chainable, Chainable> builder)
    {
        List list = new();
        AddElement(list);

        builder(list);

        return this;
    }

    /// <summary>
    ///     Add an item. Only makes sense in a list.
    /// </summary>
    /// <param name="bullet">An optional bullet leading the item.</param>
    /// <returns>This.</returns>
    public Chainable Item(String? bullet = null)
    {
        AddElement(new Item(bullet));

        return this;
    }
}

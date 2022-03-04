// <copyright file="Chainable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Manual.Elements;
using VoxelGame.Manual.Modifiers;
using Boolean = VoxelGame.Manual.Elements.Boolean;

namespace VoxelGame.Manual
{
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
        public Chainable Text(string content, TextStyle style = TextStyle.Normal)
        {
            AddElement(new Text(content, style));

            return this;
        }

        /// <summary>
        ///     Add a key box to the section.
        /// </summary>
        /// <param name="k">The key to describe.</param>
        /// <returns>The current chain.</returns>
        public Chainable Key(object k)
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
        public Chainable Boolean(bool value)
        {
            AddElement(new Boolean(value));

            return this;
        }

        /// <summary>
        ///     Begin a list, and increase the level.
        /// </summary>
        /// <returns>The list chainable. Must be ended.</returns>
        public Chainable BeginList()
        {
            List list = new(this);
            AddElement(list);

            return list;
        }

        /// <summary>
        ///     Add an item to a list.
        /// </summary>
        /// <param name="bullet">An optional bullet leading the item.</param>
        /// <returns>This.</returns>
        public Chainable Item(string? bullet = null)
        {
            AddElement(new Item(bullet));

            return this;
        }

        /// <summary>
        ///     Decrease the level.
        /// </summary>
        /// <returns>The chainable of a lower level.</returns>
        public virtual Chainable End()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     End the chain. Only valid on the lowest level.
        /// </summary>
        /// <returns>The section defined by this chain.</returns>
        public virtual Section EndSection()
        {
            throw new InvalidOperationException();
        }
    }
}

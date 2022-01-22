// <copyright file="Chainable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.IO;
using VoxelGame.Manual.Elements;

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
        /// <returns>The current chain.</returns>
        public Chainable Text(string content)
        {
            AddElement(new Text(content));

            return this;
        }

        /// <summary>
        ///     Add a key box to the section.
        /// </summary>
        /// <param name="key">The key to describe.</param>
        /// <returns>The current chain.</returns>
        public Chainable Key(object key)
        {
            AddElement(new Key(key));

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

        internal void WriteItem(StreamWriter writer)
        {
            writer.Write(@"\item");
        }
    }
}

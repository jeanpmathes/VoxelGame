// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using VoxelGame.Manual.Elements;

namespace VoxelGame.Manual
{
    /// <summary>
    ///     A section of a document, describing a topic.
    /// </summary>
    public class Section
    {
        private readonly List<Element> elements = new();
        private readonly string title;

        private Section(string title)
        {
            this.title = title;
        }

        /// <summary>
        ///     Create a new section.
        /// </summary>
        /// <param name="title">The title of the section.</param>
        /// <returns>The created section.</returns>
        public static Section Create(string title)
        {
            return new Section(title);
        }

        /// <summary>
        ///     Add text content to the section.
        /// </summary>
        /// <param name="content">The text content to add.</param>
        /// <returns>This section.</returns>
        public Section Text(string content)
        {
            elements.Add(new Text(content));

            return this;
        }

        /// <summary>
        ///     Add a key box to the section.
        /// </summary>
        /// <param name="key">The key to describe.</param>
        /// <returns>This section.</returns>
        public Section Key(object key)
        {
            elements.Add(new Key(key));

            return this;
        }

        internal void Generate(StreamWriter writer, string parent)
        {
            writer.WriteLine(@$"\subsection{{{title}}}\label{{subsec:{parent.ToLower()}_{title.ToLower()}}}");

            foreach (Element element in elements) element.Generate(writer);

            writer.WriteLine();
        }
    }
}

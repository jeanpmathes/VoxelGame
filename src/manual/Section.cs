// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual
{
    /// <summary>
    ///     A section of a document, describing a topic.
    /// </summary>
    public class Section
    {
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

        internal void Generate(StreamWriter writer)
        {
            writer.WriteLine(@$"\subsection{{{title}}}\label{{subsec:{title.ToLower()}}}");
        }
    }
}

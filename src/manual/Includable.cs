// <copyright file="Includable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual
{
    /// <summary>
    ///     A document that can be included in the manual, at defined insertion points.
    /// </summary>
    public class Includable
    {
        private readonly string outputPath;

        /// <summary>
        ///     Create a new includable document.
        /// </summary>
        /// <param name="name">The name of this document.</param>
        /// <param name="outputPath">The output path to produce a file at.</param>
        public Includable(string name, string outputPath)
        {
            this.outputPath = Path.Combine(outputPath, $"{name}.tex");
        }

        /// <summary>
        ///     Generate the output file.
        /// </summary>
        public void Generate()
        {
            using (FileStream outputStream = File.Create(outputPath)) {}
        }
    }
}

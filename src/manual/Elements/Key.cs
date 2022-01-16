// <copyright file="Key.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual.Elements
{
    /// <summary>
    ///     A nicely formatted key box.
    /// </summary>
    internal class Key : Element
    {
        private readonly object key;

        public Key(object key)
        {
            this.key = key;
        }

        public override void Generate(StreamWriter writer)
        {
            writer.Write(@$" \keys{{{key}}} ");
        }
    }
}
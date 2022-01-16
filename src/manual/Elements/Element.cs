// <copyright file="Element.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual.Elements
{
    /// <summary>
    ///     Defines an element of a document section.
    /// </summary>
    internal abstract class Element
    {
        public abstract void Generate(StreamWriter writer);
    }
}
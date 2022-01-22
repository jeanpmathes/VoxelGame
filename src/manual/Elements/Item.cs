// <copyright file="Item.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.IO;

namespace VoxelGame.Manual.Elements
{
    /// <summary>
    ///     An item for lists.
    /// </summary>
    internal class Item : IElement
    {
        void IElement.Generate(StreamWriter writer)
        {
            writer.Write(@"\item ");
        }
    }
}

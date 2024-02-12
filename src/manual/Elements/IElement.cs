// <copyright file="IElement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     Defines an element of a document section.
/// </summary>
internal interface IElement
{
    internal void Generate(StreamWriter writer);
}

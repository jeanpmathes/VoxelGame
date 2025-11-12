// <copyright file="NewLine.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     Creates a new line.
/// </summary>
internal sealed class NewLine : IElement
{
    void IElement.Generate(StreamWriter writer)
    {
        writer.WriteLine(@"\newline");
    }
}

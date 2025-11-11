// <copyright file="Boolean.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;

namespace VoxelGame.Manual.Elements;

/// <summary>
///     A piece of text that represents a boolean value.
/// </summary>
internal sealed class Boolean : IElement
{
    internal Boolean(System.Boolean value)
    {
        Value = value;
    }

    private System.Boolean Value { get; }

    void IElement.Generate(StreamWriter writer)
    {
        writer.Write(Value ? @" \Checkmark " : @" \XSolidBrush ");
    }
}

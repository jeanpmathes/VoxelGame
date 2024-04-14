// <copyright file="Message.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A property that represents a message.
/// </summary>
public class Message : Property
{
    /// <summary>
    ///     Create a <see cref="Message" /> with a name and text.
    /// </summary>
    public Message(String name, String text) : base(name)
    {
        Text = text;
    }

    /// <summary>
    ///     The text of the message.
    /// </summary>
    public String Text { get; }

    /// <exclude />
    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

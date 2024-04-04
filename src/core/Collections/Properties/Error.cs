// <copyright file="Error.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Collections.Properties;

/// <summary>
///     A property that represents a warning.
/// </summary>
public sealed class Error : Property
{
    /// <summary>
    ///     Creates a new error with the given name and message.
    /// </summary>
    public Error(String name, String message, Boolean isCritical) : base(name)
    {
        Message = message;
        IsCritical = isCritical;
    }

    /// <summary>
    ///     Gets the message of the error.
    /// </summary>
    public String Message { get; }

    /// <summary>
    ///     Whether the error is critical or just a warning.
    /// </summary>
    public Boolean IsCritical { get; }

    internal override void Accept(Visitor visitor)
    {
        visitor.Visit(this);
    }
}

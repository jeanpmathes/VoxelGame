// <copyright file="Exceptions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     Utility for creating exceptions.
/// </summary>
public static class Exceptions
{
    /// <summary>
    ///     Check whether a message is well-formed to be used as an exception message.
    ///     To be well-formed, a message must not be null or empty and must end with a punctuation mark.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns><c>true</c> if the message is well-formed, <c>false</c> otherwise.</returns>
    public static Boolean IsMessageWellFormed(String message)
    {
        if (String.IsNullOrWhiteSpace(message))
            return false;

        return message[^1] is '.' or '!' or '?';
    }

    /// <summary>
    ///     Create an aggregate exception with a message.
    /// </summary>
    /// <param name="message">The message to use.</param>
    /// <param name="inner">The inner exception.</param>
    /// <returns>The aggregate exception.</returns>
    public static Exception Annotated(String message, Exception inner)
    {
        Debug.Assert(IsMessageWellFormed(message));

        return new AggregateException(message, inner);
    }

    /// <summary>
    ///     Create an exception for an argument that is not of expected type.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="expected">The expected type.</param>
    /// <param name="actual">The actual object.</param>
    /// <returns>The exception.</returns>
    public static Exception ArgumentOfWrongType(String paramName, Type expected, Object actual)
    {
        return new ArgumentException($"Expected {Reflections.GetLongName(expected)} for {paramName}, got {Reflections.GetLongName(actual.GetType())}.", paramName);
    }

    /// <summary>
    ///     Create an exception for an argument that is not in a collection.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="actual">The actual object.</param>
    /// <returns>The exception.</returns>
    public static Exception ArgumentNotInCollection(String paramName, String collectionName, Object actual)
    {
        return new ArgumentException($"Expected argument {paramName} of value '{actual}' to be in {collectionName}.");
    }

    /// <summary>
    ///     Create an exception for an argument that violates uniqueness.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="actual">The actual object.</param>
    /// <returns>The exception.</returns>
    public static Exception ArgumentViolatesUniqueness(String paramName, Object actual)
    {
        return new ArgumentException($"Expected argument {paramName} of value '{actual}' to be unique.");
    }

    /// <summary>
    ///     Create an exception for an unsupported enum value.
    ///     This can be used both when an enum value is intentionally not supported or to make a switch exhaustive.
    /// </summary>
    /// <param name="value">The value that is not supported.</param>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <returns>The exception.</returns>
    public static InvalidEnumArgumentException UnsupportedEnumValue<T>(T value)
        where T : Enum
    {
        return new InvalidEnumArgumentException($"The enum value {value} of the enum {typeof(T).Name} is not supported here.");
    }

    /// <summary>
    ///     Create an exception for an unsupported value in a switch statement or similar.
    /// </summary>
    /// <param name="value">The value that is not supported.</param>
    /// <returns>The exception.</returns>
    public static InvalidOperationException UnsupportedValue(Object value)
    {
        return new InvalidOperationException($"The value {value} is not supported here.");
    }

    /// <summary>
    ///     Create an exception for an invalid operation.
    /// </summary>
    /// <param name="message">A message describing the invalid operation.</param>
    /// <returns>The exception.</returns>
    public static Exception InvalidOperation(String message)
    {
        Debug.Assert(IsMessageWellFormed(message));

        return new InvalidOperationException(message);
    }
}

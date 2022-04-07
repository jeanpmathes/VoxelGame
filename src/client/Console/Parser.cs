// <copyright file="Parser.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Client.Console;

/// <summary>
///     Base class for argument parsers, which parse a given string to provide a value of specific type.
/// </summary>
public abstract class Parser
{
    /// <summary>
    ///     Get the type parsed by this parser.
    /// </summary>
    public abstract Type ParsedType { get; }

    /// <summary>
    ///     Check if the given string can be parsed by this parser.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if this parser can parse the provided input.</returns>
    public abstract bool CanParse(string input);

    /// <summary>
    ///     Parse the given string to the type this parser is for.
    /// </summary>
    /// <param name="input">The input to parse. Must be checked with <see cref="CanParse" /></param>
    /// before.
    /// <returns>A value of the type this parsers targets.</returns>
    public abstract object Parse(string input);

    /// <summary>
    ///     Create a parser for a specific type.
    /// </summary>
    /// <param name="check">A function checking if a string can be parsed.</param>
    /// <param name="parse">A function parsing a string.</param>
    /// <typeparam name="T">The type the new parsers should parse.</typeparam>
    /// <returns>A parser for the specific type.</returns>
    public static Parser BuildParser<T>(Func<string, bool> check, Func<string, T> parse)
    {
        return new SimpleParser<T>(check, parse);
    }

    private sealed class SimpleParser<T> : Parser
    {
        private readonly Func<string, bool> check;
        private readonly Func<string, T> parse;

        public SimpleParser(Func<string, bool> check, Func<string, T> parse)
        {
            this.check = check;
            this.parse = parse;
        }

        public override Type ParsedType => typeof(T);

        public override bool CanParse(string input)
        {
            return check(input);
        }

        public override object Parse(string input)
        {
            return parse(input)!;
        }
    }
}

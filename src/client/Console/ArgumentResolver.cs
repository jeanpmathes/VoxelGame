// <copyright file="ArgumentResolver.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Reflection;
using VoxelGame.Client.Console.Parsers;

namespace VoxelGame.Client.Console;

/// <summary>
///     Resolves the correct overload for provided arguments and parses them.
/// </summary>
public class ArgumentResolver : ITypeRepresentationProvider
{
    private readonly Dictionary<Type, Parser> parsers = new();

    /// <inheritdoc />
    public String GetTypeRepresentation(Type type)
    {
        return parsers.TryGetValue(type, out Parser? parser)
            ? parser.TypeRepresentation
            : type.Name;
    }

    /// <summary>
    ///     Add an argument parser to the resolver.
    ///     Will replace any existing parser for the same type.
    /// </summary>
    /// <param name="parser">The parser to add.</param>
    public void AddParser(Parser parser)
    {
        parsers[parser.ParsedType] = parser;
    }

    /// <summary>
    ///     Resolve the correct overload for the provided arguments.
    /// </summary>
    /// <param name="overloads">The possible overloads to choose from.</param>
    /// <param name="args">The arguments to resolve the overload for.</param>
    /// <returns>The resolution result.</returns>
    public OverloadResolutionResult ResolveOverload(IEnumerable<CommandOverload> overloads, IReadOnlyList<String> args)
    {
        List<String> diagnostics = [];

        Int32 overloadCount = 0;

        foreach (CommandOverload overload in overloads)
        {
            overloadCount += 1;

            IReadOnlyList<ParameterInfo> parameters = overload.CallableParameters;

            Int32 offset = 0;
            List<Argument> arguments = [];
            Boolean matched = true;

            for (Int32 index = 0; index < parameters.Count; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (!parsers.TryGetValue(parameter.ParameterType, out Parser? parser))
                {
                    matched = false;

                    diagnostics.Add(
                        $"- Overload #{overloadCount}: Parameter #{index + 1} '{parameter.Name}' of type {parameter.ParameterType.Name} has no registered parser.");

                    break;
                }

                if (offset >= args.Count)
                {
                    matched = false;

                    diagnostics.Add(
                        $"- Overload #{overloadCount}: Missing argument(s) for parameter #{index + 1} '{parameter.Name}' ({parser.TypeRepresentation}).");

                    break;
                }

                Argument? argument = parser.Parse(args, offset, out Int32 consumed);

                if (argument == null || consumed <= 0)
                {
                    matched = false;

                    diagnostics.Add(
                        $"- Overload #{overloadCount}: Parameter #{index + 1} '{parameter.Name}' expects {parser.TypeRepresentation}, got '{args[offset]}'.");

                    break;
                }

                arguments.Add(argument);
                offset += consumed;
            }

            if (!matched) continue;

            if (offset == args.Count)
                return new OverloadResolutionResult(overload, arguments, []);

            diagnostics.Add($"- Overload #{overloadCount}: Expected {offset} argument(s), got {args.Count}.");
        }

        return new OverloadResolutionResult(Overload: null, [], diagnostics);
    }

    /// <summary>
    ///     The result of trying to resolve a command overload.
    /// </summary>
    /// <param name="Overload">The resolved overload, if successful.</param>
    /// <param name="Arguments">The parsed arguments corresponding to the callable parameters.</param>
    /// <param name="Diagnostics">Details about why no overload could be selected.</param>
    public sealed record OverloadResolutionResult(
        CommandOverload? Overload,
        IReadOnlyList<Argument> Arguments,
        IReadOnlyList<String> Diagnostics)
    {
        /// <summary>
        ///     Whether the resolution was successful.
        /// </summary>
        public Boolean IsSuccess => Overload != null;
    }
}

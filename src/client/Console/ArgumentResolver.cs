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

namespace VoxelGame.Client.Console;

/// <summary>
///     Resolves the correct overload for provided arguments and parses them.
/// </summary>
public class ArgumentResolver
{
    private readonly Dictionary<Type, Parser> parsers = new();

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

            if (parameters.Count != args.Count)
            {
                diagnostics.Add($"- Overload #{overloadCount} expects {parameters.Count} argument(s), got {args.Count}.");

                continue;
            }

            Boolean isValid = true;

            for (Int32 i = 0; i < parameters.Count; i++)
            {
                if (!parsers.TryGetValue(parameters[i].ParameterType, out Parser? parser))
                {
                    isValid = false;

                    diagnostics.Add(
                        $"- Parameter #{i + 1} '{parameters[i].Name}' of type {parameters[i].ParameterType.Name} has no registered parser.");

                    break;
                }

                if (!parser.CanParse(args[i]))
                {
                    isValid = false;

                    diagnostics.Add(
                        $"- Parameter #{i + 1} '{parameters[i].Name}' expects {parameters[i].ParameterType.Name}, got '{args[i]}'.");

                    break;
                }
            }

            if (isValid) return new OverloadResolutionResult(overload, []);
        }

        return new OverloadResolutionResult(Overload: null, diagnostics);
    }

    /// <summary>
    ///     Parse the arguments for a method.
    /// </summary>
    /// <param name="overload">The overload to parse the arguments for.</param>
    /// <param name="args">The arguments to parse.</param>
    /// <returns>The parsed arguments.</returns>
    public Object[] ParseArguments(CommandOverload overload, IReadOnlyList<String> args)
    {
        IReadOnlyList<ParameterInfo> parameters = overload.CallableParameters;

        Object[] parsedArgs = new Object[args.Count];

        for (Int32 i = 0; i < args.Count; i++)
            parsedArgs[i] = parsers[parameters[i].ParameterType].Parse(args[i]);

        return parsedArgs;
    }

    /// <summary>
    ///     The result of trying to resolve a command overload.
    /// </summary>
    /// <param name="Overload">The resolved overload, if successful.</param>
    /// <param name="Diagnostics">Details about why no overload could be selected.</param>
    public sealed record OverloadResolutionResult(CommandOverload? Overload, IReadOnlyList<String> Diagnostics)
    {
        /// <summary>
        ///     Whether the resolution was successful.
        /// </summary>
        public Boolean IsSuccess => Overload != null;
    }
}

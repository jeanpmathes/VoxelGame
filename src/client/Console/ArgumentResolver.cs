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
    /// <returns>The resolved overload or <c>null</c> if none could be resolved.</returns>
    public MethodInfo? ResolveOverload(IEnumerable<MethodInfo> overloads, IReadOnlyList<String> args)
    {
        foreach (MethodInfo method in overloads)
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != args.Count) continue;

            var isValid = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parsers.TryGetValue(parameters[i].ParameterType, out Parser? parser))
                {
                    isValid = false;

                    break;
                }

                if (!parser.CanParse(args[i]))
                {
                    isValid = false;

                    break;
                }
            }

            if (isValid) return method;
        }

        return null;
    }

    /// <summary>
    ///     Parse the arguments for a method.
    /// </summary>
    /// <param name="method">The method to parse the arguments for.</param>
    /// <param name="args">The arguments to parse.</param>
    /// <returns>The parsed arguments.</returns>
    public Object[] ParseArguments(MethodBase method, IReadOnlyList<String> args)
    {
        ParameterInfo[] parameters = method.GetParameters();

        var parsedArgs = new Object[args.Count];

        for (var i = 0; i < args.Count; i++)
            parsedArgs[i] = parsers[parameters[i].ParameterType].Parse(args[i]);

        return parsedArgs;
    }
}

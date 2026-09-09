// <copyright file="CommandOverload.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace VoxelGame.Client.Console;

/// <summary>
///     One overload of a command's invocation method.
/// </summary>
public sealed class CommandOverload
{
    private readonly MethodInfo method;
    private readonly Boolean requiresContext;

    private CommandOverload(MethodInfo method, ParameterInfo[] callableParameters, Boolean requiresContext)
    {
        this.method = method;
        this.requiresContext = requiresContext;

        CallableParameters = callableParameters;
    }

    /// <summary>
    ///     Get the parameters that need to be provided as explicit arguments during a call to this overload.
    /// </summary>
    public IReadOnlyList<ParameterInfo> CallableParameters { get; }

    /// <summary>
    ///     Try to create a command overload from a command's <c>Invoke</c> method.
    /// </summary>
    /// <param name="method">The <c>Invoke</c> method.</param>
    /// <param name="overload">The created command overload, if the passed method is a valid command overload.</param>
    /// <returns>Whether the invocation method represents a valid command overload.</returns>
    public static Boolean TryCreate(MethodInfo method, [NotNullWhen(returnValue: true)] out CommandOverload? overload)
    {
        ParameterInfo[] parameters = method.GetParameters();

        for (Int32 index = 0; index < parameters.Length - 1; index++)
        {
            if (parameters[index].ParameterType != typeof(Context)) continue;

            overload = null;

            return false;
        }

        Boolean requiresContext = parameters.Length > 0 && parameters[^1].ParameterType == typeof(Context);
        ParameterInfo[] callableParameters = requiresContext ? parameters[..^1] : parameters;

        overload = new CommandOverload(method, callableParameters, requiresContext);

        return true;
    }

    /// <summary>
    ///     Check whether another overload has the same callable signature.
    ///     The callable signature is the signature only considering the callable parameters.
    ///     This only compares the types, not the names of the parameters.
    /// </summary>
    /// <param name="other">The other overload.</param>
    /// <returns>Whether both overloads have the same callable signature.</returns>
    public Boolean HasSameCallableSignature(CommandOverload other)
    {
        return CallableParameters.Select(parameter => parameter.ParameterType)
            .SequenceEqual(other.CallableParameters.Select(parameter => parameter.ParameterType));
    }

    /// <summary>
    ///     Invoke this overload.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <param name="arguments">The parsed callable arguments.</param>
    /// <param name="context">The current command context.</param>
    public void Invoke(ICommand command, Object[] arguments, Context context)
    {
        Object[] invocationArguments = requiresContext ? [.. arguments, context] : arguments;

        method.Invoke(command, invocationArguments);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        String parameters = String.Join(", ",
            method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"));

        return $"{method.Name}({parameters})";
    }
}

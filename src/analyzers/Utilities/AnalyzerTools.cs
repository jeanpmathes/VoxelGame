// <copyright file="AnalyzerTools.cs" company="VoxelGame">
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
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VoxelGame.Analyzers.Utilities;

/// <summary>
///     Tools for writing analyzers.
/// </summary>
public static class AnalyzerTools
{
    /// <summary>
    ///     Check whether a type symbol is of a certain interface type or a type that implements that interface type.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="interfaceDisplayName">The name of the interface to check against.</param>
    /// <returns><c>true</c> if it matches, <c>false</c> if not</returns>
    public static Boolean IsOrImplementsInterface(ITypeSymbol typeSymbol, String interfaceDisplayName)
    {
        return typeSymbol.OriginalDefinition.ToDisplayString() == interfaceDisplayName
               || typeSymbol.AllInterfaces.Any(i => i.OriginalDefinition.ToDisplayString() == interfaceDisplayName);
    }
}

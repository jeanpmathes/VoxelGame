// <copyright file="TestTool.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace VoxelGame.Analyzers.Tests.Utilities;

/// <summary>
///     Helps with setup related things for analyzer tests.
/// </summary>
public static class TestTool
{
    private const String Version = "9.0";

    /// <summary>
    ///     The default assembly to use for tests.
    /// </summary>
    public static ReferenceAssemblies DefaultAssembly { get; } = new(
        $"net{Version}",
        new PackageIdentity("Microsoft.NETCore.App.Ref", $"{Version}.0"),
        Path.Combine("ref", $"net{Version}"));

    /// <summary>
    ///     The default solution transform to use for tests.
    /// </summary>
    public static Func<Solution, ProjectId, Solution> DefaultSolutionTransform { get; } = (solution, projectId) => solution.WithProjectParseOptions(projectId,
        new CSharpParseOptions(
            LanguageVersion.CSharp13
        ));
}

// <copyright file="TestTool.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
            languageVersion: LanguageVersion.CSharp13
        ));
}

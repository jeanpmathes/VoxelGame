// <copyright file="TestTools.cs" company="VoxelGame">
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
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace VoxelGame.SourceGenerators.Tests.Utilities;

/// <summary>
///     Tools to help with testing source generators.
/// </summary>
public static class TestTools
{
    public static String RunGenerator<T>(String source, String fileSuffix, params Type[] dependencies) where T : IIncrementalGenerator, new()
    {
        HashSet<Assembly> assemblies = [];

        foreach (Type dependency in dependencies) assemblies.Add(dependency.Assembly);

        T generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        CSharpCompilation compilation = CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp13))],
            [
                .. ((String) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Select(path => MetadataReference.CreateFromFile(path)),
                .. assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            ]);

        GeneratorDriverRunResult runResult = driver.RunGenerators(compilation).GetRunResult();
        SyntaxTree generated = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase));

        return generated.GetText().ToString();
    }
}

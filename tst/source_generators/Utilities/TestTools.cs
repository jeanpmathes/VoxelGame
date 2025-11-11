// <copyright file = "TestTools.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
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
/// Tools to help with testing source generators.
/// </summary>
public static class TestTools
{
    public static String RunGenerator<T>(String source, String fileSuffix, params Type[] dependencies) where T : IIncrementalGenerator, new()
    {
        HashSet<Assembly> assemblies = [];
        
        foreach (Type dependency in dependencies)
        {
            assemblies.Add(dependency.Assembly);
        }
        
        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp13))],
            [
                .. ((String)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Select(path => MetadataReference.CreateFromFile(path)),
                .. assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            ]);

        GeneratorDriverRunResult runResult = driver.RunGenerators(compilation).GetRunResult();
                SyntaxTree generated = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase));
        
        return generated.GetText().ToString();
    }
}

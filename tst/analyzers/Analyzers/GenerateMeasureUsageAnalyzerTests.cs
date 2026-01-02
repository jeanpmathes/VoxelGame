// <copyright file="GenerateMeasureUsageAnalyzerTests.cs" company="VoxelGame">
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

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoxelGame.Analyzers.Analyzers;
using VoxelGame.Analyzers.Tests.Utilities;
using VoxelGame.Annotations.Attributes;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.GenerateMeasureUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class GenerateMeasureUsageAnalyzerTests
{
    [Fact]
    public async Task GenerateMeasureUsageAnalyzer_ShouldDetectNonStaticProperty()
    {
        await new CSharpAnalyzerTest<GenerateMeasureUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;
                       using VoxelGame.Annotations.Definitions;

                       namespace VoxelGame.Core.Utilities.Units
                       {
                           public record Unit(String Symbol);
                       }

                       public class UnitProvider
                       {
                           [GenerateMeasure("Length", "Meters", AllowedPrefixes.Unprefixed)]
                           public VoxelGame.Core.Utilities.Units.Unit Meter { get; } = new("m");
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateMeasureAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GenerateMeasureUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 13, column: 48)
                    .WithArguments("Meter", GenerateMeasureUsageAnalyzer.ReasonNotStatic)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateMeasureUsageAnalyzer_ShouldDetectWrongType()
    {
        await new CSharpAnalyzerTest<GenerateMeasureUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;
                       using VoxelGame.Annotations.Definitions;

                       public class UnitProvider
                       {
                           [GenerateMeasure("Length", "Meters", AllowedPrefixes.Unprefixed)]
                           public static Int32 Meter { get; } = 0;
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateMeasureAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GenerateMeasureUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 8, column: 25)
                    .WithArguments("Meter", GenerateMeasureUsageAnalyzer.ReasonWrongType)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateMeasureUsageAnalyzer_ShouldNotReportForValidProperty()
    {
        await new CSharpAnalyzerTest<GenerateMeasureUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;
                       using VoxelGame.Annotations.Definitions;

                       namespace VoxelGame.Core.Utilities.Units
                       {
                           public record Unit(String Symbol);
                       }

                       public class UnitProvider
                       {
                           [GenerateMeasure("Length", "Meters", AllowedPrefixes.Unprefixed)]
                           public static VoxelGame.Core.Utilities.Units.Unit Meter { get; } = new("m");
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateMeasureAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }
}

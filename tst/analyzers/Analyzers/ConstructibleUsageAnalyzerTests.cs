// <copyright file="ConstructibleUsageAnalyzerTests.cs" company="VoxelGame">
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

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoxelGame.Analyzers.Analyzers;
using VoxelGame.Analyzers.Tests.Utilities;
using VoxelGame.Annotations.Attributes;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.ConstructibleUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class ConstructibleUsageAnalyzerTests
{
    [Fact]
    public async Task ConstructibleUsageAnalyzer_ShouldDetectMissingParameters()
    {
        await new CSharpAnalyzerTest<ConstructibleUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       namespace TestNamespace;

                       public partial class Sample
                       {
                           [Constructible]
                           public Sample()
                           {
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(ConstructibleAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ConstructibleUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 9, column: 12)
                    .WithArguments("TestNamespace.Sample.Sample()", ConstructibleUsageAnalyzer.ReasonNoParameters)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ConstructibleUsageAnalyzer_ShouldDetectUnsupportedParameter()
    {
        await new CSharpAnalyzerTest<ConstructibleUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       namespace TestNamespace;

                       public partial class Sample
                       {
                           [Constructible]
                           public Sample(ref Int32 value)
                           {
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(ConstructibleAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ConstructibleUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 9, column: 12)
                    .WithArguments("TestNamespace.Sample.Sample(ref int)", ConstructibleUsageAnalyzer.ReasonRefInOutParameters)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ConstructibleUsageAnalyzer_ShouldDetectDefaultParameterValue()
    {
        await new CSharpAnalyzerTest<ConstructibleUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       namespace TestNamespace;

                       public partial class Sample
                       {
                           [Constructible]
                           public Sample(Int32 value = 42)
                           {
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(ConstructibleAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ConstructibleUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 9, column: 12)
                    .WithArguments("TestNamespace.Sample.Sample(int)", ConstructibleUsageAnalyzer.ReasonDefaultParameterValues)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ConstructibleUsageAnalyzer_ShouldDetectParamsParameter()
    {
        await new CSharpAnalyzerTest<ConstructibleUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       namespace TestNamespace;

                       public partial class Sample
                       {
                           [Constructible]
                           public Sample(params Int32[] values)
                           {
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(ConstructibleAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ConstructibleUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 9, column: 12)
                    .WithArguments("TestNamespace.Sample.Sample(params int[])", ConstructibleUsageAnalyzer.ReasonParamsParameter)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ConstructibleUsageAnalyzer_ShouldNotReportWhenCorrect()
    {
        await new CSharpAnalyzerTest<ConstructibleUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       namespace TestNamespace;

                       public partial class Sample
                       {
                           [Constructible]
                           public Sample(Int32 value)
                           {
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(ConstructibleAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }
}

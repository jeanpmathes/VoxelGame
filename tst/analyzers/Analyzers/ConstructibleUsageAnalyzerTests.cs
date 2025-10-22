// <copyright file="ConstructibleUsageAnalyzerTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
                       using VoxelGame.Annotations;

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
                       using VoxelGame.Annotations;

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
                       using VoxelGame.Annotations;

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
                       using VoxelGame.Annotations;

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
                       using VoxelGame.Annotations;

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

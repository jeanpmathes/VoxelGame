// <copyright file="LateInitializationUsageAnalyzerTests.cs" company="VoxelGame">
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
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.LateInitializationUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class LateInitializationUsageAnalyzerTests
{
    [Fact]
    public async Task LateInitializationUsageAnalyzer_ShouldDetectNotPartial()
    {
        await new CSharpAnalyzerTest<LateInitializationUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations;

                       public partial class TestClass
                       {
                           [LateInitialization]
                           public Int32 Property { get; set; }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(LateInitializationAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 7, column: 18).WithArguments("Property")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task LateInitializationUsageAnalyzer_ShouldDetectStatic()
    {
        await new CSharpAnalyzerTest<LateInitializationUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations;

                       public partial class TestClass
                       {
                           [LateInitialization]
                           public static partial Int32 Property { get; set; }
                       }

                       public partial class TestClass
                       {
                           public static partial Int32 Property { get => 0; set {} }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(LateInitializationAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 7, column: 18).WithArguments("Property"),

                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 12, column: 25).WithArguments("Property")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task LateInitializationUsageAnalyzer_ShouldDetectNullable()
    {
        await new CSharpAnalyzerTest<LateInitializationUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations;

                       public partial class TestClass
                       {
                           [LateInitialization]
                           public partial Int32? Property { get; set; }
                       }

                       public partial class TestClass
                       {
                           public partial Int32? Property { get => 0; set {} }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(LateInitializationAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 7, column: 18).WithArguments("Property"),

                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 12, column: 25).WithArguments("Property")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task LateInitializationUsageAnalyzer_ShouldNotReportWhenCorrect()
    {
        await new CSharpAnalyzerTest<LateInitializationUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations;

                       public partial class TestClass
                       {
                           [LateInitialization]
                           public partial Int32 Property { get; set; }
                       }

                       public partial class TestClass
                       {
                           public partial Int32 Property { get => 0; set {} }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(LateInitializationAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }
}

// <copyright file="LateInitializationUsageAnalyzerTests.cs" company="VoxelGame">
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
                       using VoxelGame.Annotations.Attributes;

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
    public async Task LateInitializationUsageAnalyzer_ShouldDetectNullable()
    {
        await new CSharpAnalyzerTest<LateInitializationUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

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
                    .WithLocation(line: 7, column: 27).WithArguments("Property"),

                Verifier.Diagnostic(LateInitializationUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 12, column: 27).WithArguments("Property")
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
                       using VoxelGame.Annotations.Attributes;

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

// <copyright file="GenerateRecordUsageAnalyzerTests.cs" company="VoxelGame">
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
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.GenerateRecordUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class GenerateRecordUsageAnalyzerTests
{
    [Fact]
    public async Task GenerateRecordUsageAnalyzer_ShouldDetectGenericInterface()
    {
        await new CSharpAnalyzerTest<GenerateRecordUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using VoxelGame.Annotations.Attributes;

                       [GenerateRecord(typeof(Object))]
                       public interface I<T> {}
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateRecordAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GenerateRecordUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 5, column: 18).WithArguments("I")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateRecordUsageAnalyzer_ShouldDetectInvalidUnboundArgumentWithWrongArity()
    {
        await new CSharpAnalyzerTest<GenerateRecordUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using VoxelGame.Annotations.Attributes;

                       public interface A<T1, T2>;

                       [GenerateRecord(typeof(A<,>))]
                       public interface I {}
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateRecordAttribute).Assembly.Location)
                }
            },

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GenerateRecordUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 6, column: 18).WithArguments("I")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateRecordUsageAnalyzer_ShouldNotReportWhenCorrectWithNonGenericBase()
    {
        await new CSharpAnalyzerTest<GenerateRecordUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using VoxelGame.Annotations.Attributes;

                       public class A;

                       [GenerateRecord(typeof(A))]
                       public interface I {}
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateRecordAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateRecordUsageAnalyzer_ShouldNotReportWhenCorrectWithUnboundGenericWithArityOfOne()
    {
        await new CSharpAnalyzerTest<GenerateRecordUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using VoxelGame.Annotations.Attributes;

                       public class A<T>;

                       [GenerateRecord(typeof(A<>))]
                       public interface I {}
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateRecordAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GenerateRecordUsageAnalyzer_ShouldNotReportWhenCorrectWithBoundGenericBase()
    {
        await new CSharpAnalyzerTest<GenerateRecordUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;
                       using System.Collections.Generic;
                       using VoxelGame.Annotations.Attributes;

                       public class A<T1, T2>;

                       [GenerateRecord(typeof(A<Int32, Int32>))]
                       public interface I {}
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            TestState =
            {
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(GenerateRecordAttribute).Assembly.Location)
                }
            }
        }.RunAsync();
    }
}

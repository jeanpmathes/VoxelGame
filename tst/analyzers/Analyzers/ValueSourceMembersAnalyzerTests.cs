// <copyright file="GetValueUsageAnalyzerTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoxelGame.Analyzers.Analyzers;
using VoxelGame.Analyzers.Tests.Utilities;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.ValueSourceMembersAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

[TestSubject(typeof(ValueSourceMembersAnalyzer))]
public class ValueSourceMembersAnalyzerTests
{
    [Fact]
    public async Task ValueSourceMembersAnalyzer_ShouldReportPublicValueSourceField()
    {
        await new CSharpAnalyzerTest<ValueSourceMembersAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }
                       }

                       public class TestClass 
                       {
                           public VoxelGame.GUI.Bindings.IValueSource<Object> Source = null!;
                       }
                       """,

            ReferenceAssemblies = TestTools.DefaultAssembly,
            SolutionTransforms = {TestTools.DefaultSolutionTransform},

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ValueSourceMembersAnalyzer.DiagnosticID)
                    .WithLocation(line: 13, column: 56)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ValueSourceMembersAnalyzer_ShouldNotReportReadOnlyPublicValueSourceField()
    {
        await new CSharpAnalyzerTest<ValueSourceMembersAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }
                       }

                       public class TestClass 
                       {
                           public readonly VoxelGame.GUI.Bindings.IValueSource<Object> Source = null!;
                       }
                       """,

            ReferenceAssemblies = TestTools.DefaultAssembly,
            SolutionTransforms = {TestTools.DefaultSolutionTransform}
        }.RunAsync();
    }

    [Fact]
    public async Task ValueSourceMembersAnalyzer_ShouldReportPublicValueSourceProperty()
    {
        await new CSharpAnalyzerTest<ValueSourceMembersAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }
                       }

                       public class TestClass 
                       {
                           public VoxelGame.GUI.Bindings.IValueSource<Object> Source { get; set; } = null!;
                       }
                       """,

            ReferenceAssemblies = TestTools.DefaultAssembly,
            SolutionTransforms = {TestTools.DefaultSolutionTransform},

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(ValueSourceMembersAnalyzer.DiagnosticID)
                    .WithLocation(line: 13, column: 5)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task ValueSourceMembersAnalyzer_ShouldNotReportReadOnlyPublicValueSourceProperty()
    {
        await new CSharpAnalyzerTest<ValueSourceMembersAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }
                       }

                       public class TestClass 
                       {
                           public VoxelGame.GUI.Bindings.IValueSource<Object> Source { get; } = null!;
                       }
                       """,

            ReferenceAssemblies = TestTools.DefaultAssembly,
            SolutionTransforms = {TestTools.DefaultSolutionTransform}
        }.RunAsync();
    }

    [Fact]
    public async Task ValueSourceMembersAnalyzer_ShouldNotReportWriteOnlyPublicValueSourceProperty()
    {
        await new CSharpAnalyzerTest<ValueSourceMembersAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }
                       }

                       public class TestClass 
                       {
                           public VoxelGame.GUI.Bindings.IValueSource<Object> Source { set { } }
                       }
                       """,

            ReferenceAssemblies = TestTools.DefaultAssembly,
            SolutionTransforms = {TestTools.DefaultSolutionTransform}
        }.RunAsync();
    }
}

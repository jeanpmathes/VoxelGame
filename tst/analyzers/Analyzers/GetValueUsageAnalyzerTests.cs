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
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.GetValueUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

[TestSubject(typeof(GetValueUsageAnalyzer))]
public class GetValueUsageAnalyzerTests
{
    [Fact]
    public async Task GetValueUsageAnalyzer_ValueSource1_ShouldDetectGetValueUsageInBindingMethod()
    {
        await new CSharpAnalyzerTest<GetValueUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<T> 
                           {
                               public T GetValue();
                           }

                           public class Binding 
                           {
                               public static void DoStuff(Object val) 
                               {
                               
                               }   
                           }
                       }

                       public class TestClass 
                       {
                           public static void TestMethod() 
                           {
                               VoxelGame.GUI.Bindings.IValueSource<Object> source = null!;
                               VoxelGame.GUI.Bindings.Binding.DoStuff(source.GetValue());
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GetValueUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 24, column: 48)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task GetValueUsageAnalyzer_ValueSource2_ShouldDetectGetValueUsageInBindingMethod()
    {
        await new CSharpAnalyzerTest<GetValueUsageAnalyzer, DefaultVerifier>
        {
            TestCode = """
                       using System;

                       namespace VoxelGame.GUI.Bindings 
                       {
                           public interface IValueSource<TIn, TOut> 
                           {
                               public TOut GetValue(TIn input);
                           }

                           public class Binding 
                           {
                               public static void DoStuff(Object val) 
                               {
                               
                               }   
                           }
                       }

                       public class TestClass 
                       {
                           public static void TestMethod() 
                           {
                               VoxelGame.GUI.Bindings.IValueSource<Object, Object> source = null!;
                               VoxelGame.GUI.Bindings.Binding.DoStuff(source.GetValue(null!));
                           }
                       }
                       """,

            ReferenceAssemblies = TestTool.DefaultAssembly,
            SolutionTransforms = {TestTool.DefaultSolutionTransform},

            ExpectedDiagnostics =
            {
                Verifier.Diagnostic(GetValueUsageAnalyzer.DiagnosticID)
                    .WithLocation(line: 24, column: 48)
            }
        }.RunAsync();
    }
}

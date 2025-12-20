// <copyright file="HiddenFieldUsageAnalyzerTests.cs" company="VoxelGame">
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

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using VoxelGame.Analyzers.Analyzers;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.HiddenFieldUsageAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class HiddenFieldUsageAnalyzerTests
{
    [Fact]
    public async Task DoubleUnderscoreFieldUsageAnalyzer_ShouldDetectUseOfInstanceField()
    {
        const String text = """
                            public class C
                            {
                                private int __f;
                                public void M()
                                {
                                    var x = __f;
                                }
                            }
                            """;

        DiagnosticResult expected = Verifier.Diagnostic(HiddenFieldUsageAnalyzer.DiagnosticID)
            .WithLocation(line: 6, column: 17).WithArguments("__f");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task DoubleUnderscoreFieldUsageAnalyzer_ShouldDetectUseViaMemberAccess()
    {
        const String text = """
                            public class C
                            {
                                public int __f;
                            }

                            public class D
                            {
                                public void M()
                                {
                                    var c = new C();
                                    var x = c.__f;
                                }
                            }
                            """;

        DiagnosticResult expected = Verifier.Diagnostic(HiddenFieldUsageAnalyzer.DiagnosticID)
            .WithLocation(line: 11, column: 19).WithArguments("__f");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task DoubleUnderscoreFieldUsageAnalyzer_ShouldDetectUseOfInNameof()
    {
        const String text = """
                            public class C
                            {
                                private int __f;
                                public void M()
                                {
                                    var x = nameof(__f);
                                }
                            }
                            """;

        DiagnosticResult expected = Verifier.Diagnostic(HiddenFieldUsageAnalyzer.DiagnosticID)
            .WithLocation(line: 6, column: 24).WithArguments("__f");

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
}

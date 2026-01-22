// <copyright file="EnumToStringAnalyzerTests.cs" company="VoxelGame">
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

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using VoxelGame.Analyzers.Analyzers;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<VoxelGame.Analyzers.Analyzers.EnumToStringAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace VoxelGame.Analyzers.Tests.Analyzers;

public class EnumToStringAnalyzerTests
{
    [Fact]
    public async Task EnumToStringAnalyzer_ShouldDetectUsageOfEnumToString()
    {
        const String text = """
                            public enum TestEnum
                            {
                                Value1,
                                Value2
                            }

                            public class TestClass
                            {
                                public void TestMethod()
                                {
                                    TestEnum e = TestEnum.Value1;
                                    string s = e.ToString();
                                }
                            }
                            """;

        DiagnosticResult expected = Verifier.Diagnostic(EnumToStringAnalyzer.ToStringDiagnosticID)
            .WithLocation(line: 12, column: 20);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task EnumToStringAnalyzer_ShouldDetectUsageOfEnumInStringInterpolation()
    {
        const String text = """
                            public enum TestEnum
                            {
                                Value1,
                                Value2
                            }

                            public class TestClass
                            {
                                public void TestMethod()
                                {
                                    TestEnum e = TestEnum.Value1;
                                    string s = $"Enum value: {e}";
                                }
                            }
                            """;

        DiagnosticResult expected = Verifier.Diagnostic(EnumToStringAnalyzer.InterpolationDiagnosticID)
            .WithLocation(line: 12, column: 34);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
}

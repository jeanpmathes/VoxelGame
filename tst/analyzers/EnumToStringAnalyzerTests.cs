// <copyright file="EnumToStringAnalyzerTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<VoxelGame.Analyzers.EnumToStringAnalyzer>;

namespace VoxelGame.Analyzers.Tests;

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

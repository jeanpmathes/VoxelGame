using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<VoxelGame.Analyzers.EnumToStringAnalyzer>;

namespace VoxelGame.Analyzers.Tests;

public class SampleSemanticAnalyzerTests
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

        DiagnosticResult expected = Verifier.Diagnostic()
            .WithLocation(line: 12, column: 20);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
}

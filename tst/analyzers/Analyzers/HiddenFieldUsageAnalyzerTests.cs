// <copyright file="HiddenFieldUsageAnalyzerTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

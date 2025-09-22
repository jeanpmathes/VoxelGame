// <copyright file="EnumToStringCodeFixProviderTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<VoxelGame.Analyzers.EnumToStringAnalyzer, VoxelGame.Analyzers.EnumToStringCodeFixProvider>;

namespace VoxelGame.Analyzers.Tests;

public class EnumToStringCodeFixProviderTests
{
    [Fact]
    public async Task EnumToStringCodeFixProvider_ShouldReplaceToStringWithToStringFast()
    {
        const String oldText = """
                               public enum TestEnum
                               {
                                   Value1,
                                   Value2
                               }
                               
                               public static class EnumExtensions
                               {
                                   public static string ToStringFast(this TestEnum value)
                                   {
                                       return "";
                                   }
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

        const String newText = """
                               public enum TestEnum
                               {
                                   Value1,
                                   Value2
                               }
                               
                               public static class EnumExtensions
                               {
                                   public static string ToStringFast(this TestEnum value)
                                   {
                                       return "";
                                   }
                               }
                               
                               public class TestClass
                               {
                                   public void TestMethod()
                                   {
                                       TestEnum e = TestEnum.Value1;
                                       string s = e.ToStringFast();
                                   }
                               }
                               """;

        DiagnosticResult expected = Verifier.Diagnostic().WithLocation(line: 20, column: 20);

        await Verifier.VerifyCodeFixAsync(oldText, expected, newText).ConfigureAwait(false);
    }
}

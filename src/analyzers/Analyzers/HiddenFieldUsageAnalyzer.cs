// <copyright file="HiddenFieldUsageAnalyzer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VoxelGame.Analyzers.Analyzers;

/// <summary>
///     Warns if a field prefixed with two underscores is used as these are hidden fields created by generators.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HiddenFieldUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0004";

    private const String Category = "Usage";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "Use of generated double-underscore field",
        "Field '{0}' is generated (double-underscore prefix) and must not be used directly",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Fields starting with two underscores are generated implementation details and must not be referenced directly.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
    }

    private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IdentifierNameSyntax identifierName)
            return;

        if (context.SemanticModel.GetSymbolInfo(identifierName).Symbol is not IFieldSymbol fieldSymbol)
            return;

        if (!fieldSymbol.Name.StartsWith("__", StringComparison.Ordinal))
            return;

        if (identifierName.Parent is VariableDeclaratorSyntax declarator &&
            declarator.Identifier.ValueText == fieldSymbol.Name)
            return;

        var diagnostic = Diagnostic.Create(rule, identifierName.Identifier.GetLocation(), fieldSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}

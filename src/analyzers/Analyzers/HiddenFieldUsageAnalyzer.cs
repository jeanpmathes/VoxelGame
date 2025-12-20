// <copyright file="HiddenFieldUsageAnalyzer.cs" company="VoxelGame">
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

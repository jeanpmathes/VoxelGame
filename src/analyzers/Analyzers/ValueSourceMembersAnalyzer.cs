// <copyright file="ValueSourceMembersAnalyzer.cs" company="VoxelGame">
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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VoxelGame.Analyzers.Utilities;

namespace VoxelGame.Analyzers.Analyzers;

/// <summary>
///     Analyzes the declarations of properties that expose value sources.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueSourceMembersAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0010";

    private static readonly DiagnosticDescriptor rule = new(
        "VG0010",
        "Value sources must not be exposed through non-readonly members",
        "Exposing a value source through a non-readonly member means the source can be changed which does not update bindings to it",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Bindings to a value source always bind to the instance, not the member providing it. In the case of members that provide a value source, one might expect bindings to it to update when the value source changes, but they will not.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclarationSyntax)
            return;

        foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) is not IFieldSymbol fieldSymbol)
                continue;

            AnalyzeDeclaration(context, fieldSymbol, isWriteOnly: false, fieldSymbol.IsReadOnly, fieldSymbol.Type, variableDeclaratorSyntax.GetLocation());
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclarationSyntax)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not {} memberDeclarationSymbol) return;
        if (memberDeclarationSymbol is not {} propertySymbol) return;

        AnalyzeDeclaration(context, propertySymbol, propertySymbol.IsWriteOnly, propertySymbol.IsReadOnly, propertySymbol.Type, propertyDeclarationSyntax.GetLocation());
    }

    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context, ISymbol declarationSymbol, Boolean isWriteOnly, Boolean isReadOnly, ITypeSymbol typeSymbol, Location location)
    {
        if (declarationSymbol.DeclaredAccessibility <= Accessibility.Private)
            return;

        // If it is read-only, the binding never changes, so binding to it is not critical.
        // If it is write-only, one can never bind to it.
        // In both cases, we don't need to report anything.

        if (isReadOnly || isWriteOnly)
            return;

        if (AnalyzerTools.IsOrImplementsInterface(typeSymbol, Constants.ValueSource) || AnalyzerTools.IsOrImplementsInterface(typeSymbol, Constants.ValueSource2))
        {
            Diagnostic diagnostic = Diagnostic.Create(rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

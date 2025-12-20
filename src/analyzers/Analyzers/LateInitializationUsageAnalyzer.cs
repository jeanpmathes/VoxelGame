// <copyright file="LateInitializationUsageAnalyzer.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Analyzers.Analyzers;

/// <summary>
///     Warns if the <see cref="LateInitializationAttribute" /> is used incorrectly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LateInitializationUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0003";

    private const String Category = "Usage";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "Property with LateInitialization must be partial",
        "Property '{0}' is marked with LateInitialization but is not partial or non-nullable",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Properties marked with LateInitialization must be partial or non-nullable.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclarationSyntax)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not {} propertySymbol)
            return;

        foreach (AttributeData? attribute in propertySymbol.GetAttributes())
        {
            if (attribute.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name != nameof(LateInitializationAttribute) && attributeClass.ToDisplayString() != typeof(LateInitializationAttribute).FullName) continue;

            if (!propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
                || propertyDeclarationSyntax.Type is NullableTypeSyntax
                || propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                var diagnostic = Diagnostic.Create(rule, propertyDeclarationSyntax.Identifier.GetLocation(), propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            break;
        }
    }
}

// <copyright file="LateInitializationUsageAnalyzer.cs" company="VoxelGame">
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
        "Property '{0}' is marked with LateInitialization but is not partial, non-nullable or non-static",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Properties marked with LateInitialization must be partial, non-nullable and non-static.");

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

            var isPartial = false;

            foreach (SyntaxToken modifier in propertyDeclarationSyntax.Modifiers)
            {
                if (!modifier.IsKind(SyntaxKind.PartialKeyword))
                    continue;

                isPartial = true;

                break;
            }

            if (!isPartial || propertySymbol.IsStatic || propertyDeclarationSyntax.Type is NullableTypeSyntax || propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                var diagnostic = Diagnostic.Create(rule, propertyDeclarationSyntax.Identifier.GetLocation(), propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            break;
        }
    }
}

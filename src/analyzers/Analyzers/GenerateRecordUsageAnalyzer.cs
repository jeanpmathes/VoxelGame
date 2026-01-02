// <copyright file="GenerateRecordUsageAnalyzer.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;

namespace VoxelGame.Analyzers.Analyzers;

/// <summary>
///     Analyzes the usage of the <see cref="GenerateRecordAttribute" />.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenerateRecordUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0005";

    private const String Category = "Usage";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "GenerateRecord usage prevents generation",
        "Type '{0}' is marked with GenerateRecord but is either a generic interface or the passed attribute is unsupported",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "The GenerateRecord attribute can only be applied to non-generic interfaces. If a base type is passed, it must be a non-generic type or a generic type with one type parameter.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
    }

    private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl) return;
        if (context.SemanticModel.GetDeclaredSymbol(typeDecl) is not {} typeSymbol) return;

        foreach (AttributeData attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass is null) continue;

            if (attribute.AttributeClass.Name != nameof(GenerateRecordAttribute) &&
                attribute.AttributeClass.ToDisplayString() != typeof(GenerateRecordAttribute).FullName)
                continue;

            Boolean ok = !typeSymbol.IsGenericType && AttributeArgumentsFit(attribute);

            if (ok) continue;

            Location location = typeDecl.Identifier.GetLocation();
            var diagnostic = Diagnostic.Create(rule, location, typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);

            break;
        }
    }

    private static Boolean AttributeArgumentsFit(AttributeData attribute)
    {
        switch (attribute.ConstructorArguments.Length)
        {
            case 0:
                return true;

            case 1:
            {
                TypedConstant argument = attribute.ConstructorArguments[index: 0];

                if (argument.Kind != TypedConstantKind.Type || argument.Value is not INamedTypeSymbol baseTypeSymbol)
                    return false;

                if (baseTypeSymbol.IsUnboundGenericType)
                    return baseTypeSymbol.Arity == 1;

                return true;
            }

            default:
                return false;
        }

    }
}

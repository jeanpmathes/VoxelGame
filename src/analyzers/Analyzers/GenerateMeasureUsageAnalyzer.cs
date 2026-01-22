// <copyright file="GenerateMeasureUsageAnalyzer.cs" company="VoxelGame">
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
///     Warns when <see cref="GenerateMeasureAttribute" /> is applied to an invalid property.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GenerateMeasureUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic identifier for invalid attribute usage.
    /// </summary>
    public const String DiagnosticID = "VG0007";

    /// <summary>
    ///     Reason message for the property not being static.
    /// </summary>
    public const String ReasonNotStatic = "it is not static";

    /// <summary>
    ///     Reason message for the property having an incorrect type.
    /// </summary>
    public const String ReasonWrongType = "its type is not VoxelGame.Core.Utilities.Units.Unit";

    /// <summary>
    ///     Diagnostic message for attribute misuse.
    /// </summary>
    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "GenerateMeasure can only be applied to unit properties",
        "Property '{0}' is marked with GenerateMeasure but {1}",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Properties marked with GenerateMeasure must be static properties of type Unit.");

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
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) is not {} propertySymbol)
            return;

        AttributeData? attribute = null;

        foreach (AttributeData? attributeData in propertySymbol.GetAttributes())
        {
            if (attributeData?.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name != nameof(GenerateMeasureAttribute)
                && attributeClass.ToDisplayString() != typeof(GenerateMeasureAttribute).FullName)
                continue;

            attribute = attributeData;

            break;
        }

        if (attribute is null)
            return;

        Location location = propertyDeclaration.Identifier.GetLocation();

        if (!propertySymbol.IsStatic) context.ReportDiagnostic(Diagnostic.Create(rule, location, propertySymbol.Name, ReasonNotStatic));

        if (!IsUnitType(propertySymbol.Type)) context.ReportDiagnostic(Diagnostic.Create(rule, location, propertySymbol.Name, ReasonWrongType));
    }

    private static Boolean IsUnitType(ISymbol symbol)
    {
        return symbol.Name == "Unit"
               && symbol.ContainingNamespace.ToDisplayString() == "VoxelGame.Core.Utilities.Units";
    }
}

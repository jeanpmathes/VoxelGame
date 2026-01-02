// <copyright file="ConstructibleUsageAnalyzer.cs" company="VoxelGame">
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
///     Warns if the <see cref="ConstructibleAttribute" /> is used incorrectly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConstructibleUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0006";

    private const String Category = "Usage";

    /// <summary>
    ///     The reason why the constructor is invalid - it has no parameters.
    /// </summary>
    public const String ReasonNoParameters = "it does not declare any parameters";

    /// <summary>
    ///     The reason why the constructor is invalid - it uses ref, in or out parameters.
    /// </summary>
    public const String ReasonRefInOutParameters = "it uses ref, in or out parameters";

    /// <summary>
    ///     The reason why the constructor is invalid - it specifies default parameter values.
    /// </summary>
    public const String ReasonDefaultParameterValues = "it specifies default parameter values";

    /// <summary>
    ///     The reason why the constructor is invalid - it uses a params parameter.
    /// </summary>
    public const String ReasonParamsParameter = "it uses a params parameter";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "Constructible constructors must be valid",
        "Constructor '{0}' is marked with Constructible but {1}",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        "Constructible constructors must use supported kinds of parameters.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConstructorDeclarationSyntax constructorDeclaration)
            return;

        if (context.SemanticModel.GetDeclaredSymbol(constructorDeclaration) is not {} constructorSymbol)
            return;

        if (!HasConstructibleAttribute(constructorSymbol))
            return;

        if (constructorDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            return;

        Location location = constructorDeclaration.Identifier.GetLocation();

        if (!IsPartial(typeDeclaration))
            return;

        ImmutableArray<IParameterSymbol> parameters = constructorSymbol.Parameters;

        if (parameters.Length == 0)
        {
            Report(context, location, constructorSymbol, ReasonNoParameters);

            return;
        }

        foreach (IParameterSymbol parameter in parameters)
        {
            if (parameter.RefKind != RefKind.None)
            {
                Report(context, location, constructorSymbol, ReasonRefInOutParameters);

                return;
            }

            if (parameter.HasExplicitDefaultValue)
            {
                Report(context, location, constructorSymbol, ReasonDefaultParameterValues);

                return;
            }

            if (parameter.IsParams)
            {
                Report(context, location, constructorSymbol, ReasonParamsParameter);

                return;
            }
        }

    }

    private static void Report(SyntaxNodeAnalysisContext context, Location location, ISymbol constructorSymbol, String message)
    {
        var diagnostic = Diagnostic.Create(rule, location, constructorSymbol.ToDisplayString(), message);
        context.ReportDiagnostic(diagnostic);
    }

    private static Boolean HasConstructibleAttribute(ISymbol constructorSymbol)
    {
        foreach (AttributeData attribute in constructorSymbol.GetAttributes())
        {
            if (attribute.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name == nameof(ConstructibleAttribute))
                return true;

            if (attributeClass.ToDisplayString() == typeof(ConstructibleAttribute).FullName)
                return true;
        }

        return false;
    }

    private static Boolean IsPartial(MemberDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    }
}

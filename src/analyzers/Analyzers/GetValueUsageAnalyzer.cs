// <copyright file="GetValueUsageAnalyzer.cs" company="VoxelGame">
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
using VoxelGame.Analyzers.Walkers;

namespace VoxelGame.Analyzers.Analyzers;

/// <summary>
///     Analyzes the usage of the <c>GetValue</c> method of value sources within bindings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GetValueUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The ID of diagnostics produced by this analyzer.
    /// </summary>
    public const String DiagnosticID = "VG0009";

    private static readonly DiagnosticDescriptor rule = new(
        DiagnosticID,
        "All value sources in bindings must be correctly referenced",
        "All value sources in bindings must be correctly referenced, GetValue() in a binding indicates an error",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Creating a binding from value sources will not subscribe to changes of the sources except when the sources are explicitly passed to binding creation.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationSyntax)
            return;

        if (context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol is not IMethodSymbol methodSymbol)
            return;

        if (!IsContainingTypeBindingType(methodSymbol) && !IsReceiverTypeBindingType(invocationSyntax, context.SemanticModel))
            return;

        foreach (ArgumentSyntax argument in invocationSyntax.ArgumentList.Arguments)
        {
            if (GetValueFindingWalker.ContainsGetValueInvocation(argument.Expression, context.SemanticModel) is not {} location)
                continue;

            context.ReportDiagnostic(Diagnostic.Create(rule, location));
        }
    }

    private static Boolean IsContainingTypeBindingType(IMethodSymbol methodSymbol)
    {
        return IsBindingType(methodSymbol.ContainingType);
    }

    private static Boolean IsReceiverTypeBindingType(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        ITypeSymbol? receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;

        return IsBindingType(receiverType);
    }

    private static Boolean IsBindingType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null) return false;

        ITypeSymbol original = typeSymbol.OriginalDefinition;

        return original.Name == "Binding"
               && original.ContainingNamespace?.ToDisplayString() == "VoxelGame.GUI.Bindings";
    }
}

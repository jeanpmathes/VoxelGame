// <copyright file="EnumToStringAnalyzer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace VoxelGame.Analyzers;

/// <summary>
/// Enforces the usage of a custom enum to string conversion method instead of the default <see cref="Enum.ToString()"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnumToStringAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The ID of diagnostics produced for <see cref="Enum.ToString()"/> by this analyzer.
    /// </summary>
    public const String ToStringDiagnosticID = "VG0001";
    
    /// <summary>
    /// The ID of diagnostics produced for string interpolation by this analyzer.
    /// </summary>
    public const String InterpolationDiagnosticID = "VG0002";
    
    private const String Category = "Usage";
    
    private static readonly DiagnosticDescriptor toStringRule = new(ToStringDiagnosticID,
        "Use Enum.ToStringFast()",
        "Enum.ToString() used, prefer Enum.ToStringFast() for better performance",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Enum.ToStringFast() is significantly faster than Enum.ToString() and should be preferred in performance critical code.");

    private static readonly DiagnosticDescriptor interpolationRule = new(InterpolationDiagnosticID,
        "Use Enum.ToStringFast() in string interpolation",
        "Enum used in string interpolation, consider using Enum.ToStringFast() for better performance",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Enum.ToStringFast() is significantly faster than Enum.ToString() and should be preferred in performance critical code.");
    
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(toStringRule, interpolationRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeInterpolation, OperationKind.Interpolation);
    }
    
    private static void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocationOperation ||
            context.Operation.Syntax is not InvocationExpressionSyntax invocationSyntax)
            return;

        IMethodSymbol methodSymbol = invocationOperation.TargetMethod;
        
        if (!IsTargetedMethod(methodSymbol))
            return;
        
        if (invocationSyntax.ArgumentList.Arguments.Count != 0)
            return;
        
        ITypeSymbol? receiverType = invocationOperation.Instance?.Type;
        if (receiverType is ITypeParameterSymbol)
            return;

        var diagnostic = Diagnostic.Create(toStringRule, invocationSyntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
    
    private static void AnalyzeInterpolation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInterpolationOperation interpolationOperation ||
            context.Operation.Syntax is not InterpolationSyntax interpolationSyntax)
            return;

        ITypeSymbol? typeSymbol = interpolationOperation.Expression.Type;
        if (typeSymbol is null || typeSymbol.TypeKind != TypeKind.Enum)
            return;

        var diagnostic = Diagnostic.Create(interpolationRule, interpolationSyntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
    
    private static Boolean IsTargetedMethod(IMethodSymbol methodSymbol)
        => methodSymbol is {MethodKind: MethodKind.Ordinary, ReceiverType.Name: nameof(Enum), Name: nameof(Enum.ToString), Parameters.Length: 0};
}

// <copyright file="EnumToStringCodeFixProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace VoxelGame.Analyzers;

/// <summary>
/// Fixes issues found by <see cref="EnumToStringAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnumToStringCodeFixProvider)), Shared]
public class EnumToStringCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public sealed override ImmutableArray<String> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(EnumToStringAnalyzer.DiagnosticID);
    
    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.Single();
        
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
        
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SyntaxNode? diagnosticNode = root?.FindNode(diagnosticSpan);
        
        if (diagnosticNode is not InvocationExpressionSyntax invocationExpressionSyntax)
            return;
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace Enum.ToString with Enum.ToStringFast",
                createChangedSolution: c => ReplaceToStringMethod(context.Document, invocationExpressionSyntax, c),
                equivalenceKey: "EnumToStringCodeFixProvider"),
            diagnostic);
    }
    
    private static async Task<Solution> ReplaceToStringMethod(Document document,
        InvocationExpressionSyntax invocationExpressionSyntax, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        
        if (root == null)
            return document.Project.Solution;
        
        InvocationExpressionSyntax newInvocation = invocationExpressionSyntax
            .WithExpression(SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccess
                    ? memberAccess.Expression
                    : invocationExpressionSyntax.Expression,
                SyntaxFactory.IdentifierName("ToStringFast")))
            .WithTriviaFrom(invocationExpressionSyntax);
        
        SyntaxNode newRoot = root.ReplaceNode(invocationExpressionSyntax, newInvocation);
        Document newDocument = document.WithSyntaxRoot(newRoot);
        
        return newDocument.Project.Solution;
    }
}

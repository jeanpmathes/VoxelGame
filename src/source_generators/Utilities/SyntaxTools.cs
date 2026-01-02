// <copyright file="SyntaxTools.cs" company="VoxelGame">
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
///     Helps with working with syntax nodes.
/// </summary>
public static class SyntaxTools
{
    /// <summary>
    ///     Determine the namespace a node is contained in.
    /// </summary>
    /// <param name="node">The syntax node.</param>
    /// <returns>The namespace the type is contained in, or an empty string if it is in the global namespace.</returns>
    public static String GetNamespace(SyntaxNode node)
    {
        var @namespace = "";

        SyntaxNode? potentialNamespaceParent = node.Parent;

        while (potentialNamespaceParent != null
               && potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax) potentialNamespaceParent = potentialNamespaceParent.Parent;

        if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceParent)
            return @namespace;

        @namespace = namespaceParent.Name.ToString();

        while (true)
        {
            if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                break;

            @namespace = $"{namespaceParent.Name}.{@namespace}";
            namespaceParent = parent;
        }

        return @namespace;
    }

    /// <summary>
    ///     Get the containing type chain of a type declaration.
    ///     This does not include the passed type itself.
    /// </summary>
    /// <param name="node">The type declaration syntax node.</param>
    /// <param name="semanticModel">The semantic model to use for symbol information.</param>
    /// <returns>The chain of containing type.</returns>
    public static ContainingType? GetContainingType(TypeDeclarationSyntax node, SemanticModel semanticModel)
    {
        return GetContainingType(node, first: null, semanticModel);
    }

    /// <summary>
    ///     Get the containing type chain of a member declaration.
    /// </summary>
    /// <param name="node">The member declaration syntax node.</param>
    /// <param name="semanticModel">The semantic model to use for symbol information.</param>
    /// <returns>The chain of containing type.</returns>
    public static ContainingType? GetContainingType(MemberDeclarationSyntax node, SemanticModel semanticModel)
    {
        return node.Parent is TypeDeclarationSyntax first ? GetContainingType(first, first, semanticModel) : null;
    }

    private static ContainingType? GetContainingType(TypeDeclarationSyntax node, TypeDeclarationSyntax? first, SemanticModel semanticModel)
    {
        var parentSyntax = node.Parent as TypeDeclarationSyntax;
        ContainingType? containingInfo = first != null ? CreateContainingType(first, child: null, semanticModel) : null;

        while (parentSyntax?.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordDeclaration)
        {
            containingInfo = CreateContainingType(parentSyntax, containingInfo, semanticModel);
            parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
        }

        return containingInfo;
    }

    private static ContainingType CreateContainingType(TypeDeclarationSyntax node, ContainingType? child, SemanticModel semanticModel)
    {
        return new ContainingType(
            SyntaxFacts.GetText(semanticModel.GetDeclaredSymbol(node)?.DeclaredAccessibility ?? Accessibility.NotApplicable),
            node.Keyword.ValueText,
            node.Identifier.ToString(),
            node.TypeParameterList?.ToString(),
            node.ConstraintClauses.ToString(),
            child);
    }
}

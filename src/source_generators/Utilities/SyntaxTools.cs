// <copyright file="SyntaxTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
/// Helps with working with syntax nodes.
/// </summary>
public static class SyntaxTools
{
    /// <summary>
    /// Determine the namespace a type declaration is contained in.
    /// </summary>
    /// <param name="node">The type declaration syntax node.</param>
    /// <returns>The namespace the type is contained in, or an empty string if it is in the global namespace.</returns>
    public static String GetNamespace(BaseTypeDeclarationSyntax node)
    {
        var nameSpace = "";
        
        SyntaxNode? potentialNamespaceParent = node.Parent;
        
        while (potentialNamespaceParent != null 
               && potentialNamespaceParent is not NamespaceDeclarationSyntax 
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceParent) 
            return nameSpace;
        
        nameSpace = namespaceParent.Name.ToString();
        
        while (true)
        {
            if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                break;
            
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            namespaceParent = parent;
        }
        
        return nameSpace;
    }
    
    /// <summary>
    /// Get the containing type chain of a type declaration.
    /// </summary>
    /// <param name="node">The type declaration syntax node.</param>
    /// <returns>The chain of containing type.</returns>
    public static ContainingType? GetContainingType(BaseTypeDeclarationSyntax node)
    {
        var parentSyntax = node.Parent as TypeDeclarationSyntax;
        ContainingType? containingInfo = null;
        
        while (parentSyntax?.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordDeclaration)
        {
            containingInfo = new ContainingType(
                keyword: parentSyntax.Keyword.ValueText,
                parentSyntax.Identifier.ToString(),
                parentSyntax.TypeParameterList?.ToString(),
                parentSyntax.ConstraintClauses.ToString(),
                child: containingInfo);
            
            parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
        }
        
        return containingInfo;
    }
}

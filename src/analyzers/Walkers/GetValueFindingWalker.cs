// <copyright file="GetValueFindingWalker.cs" company="VoxelGame">
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VoxelGame.Analyzers.Walkers;

internal class GetValueFindingWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel semanticModel;

    private Location? found;

    private GetValueFindingWalker(SemanticModel semanticModel)
    {
        this.semanticModel = semanticModel;
    }

    public static Location? ContainsGetValueInvocation(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        GetValueFindingWalker visitor = new(semanticModel);

        visitor.Visit(expression);

        return visitor.found;
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        base.VisitInvocationExpression(invocationExpressionSyntax);

        if (found != null)
            return;

        if (IsGetValueInvocation(invocationExpressionSyntax, semanticModel) || IsGetValue2Invocation(invocationExpressionSyntax, semanticModel))
            found = invocationExpressionSyntax.GetLocation();
    }

    private static Boolean IsGetValueInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
    {
        if (IsGetValueInvocationBase(invocationExpressionSyntax, semanticModel) is not {} methodSymbol)
            return false;

        if (methodSymbol is not {Name: "GetValue", Parameters.Length: 0})
            return false;

        return methodSymbol.ContainingType is {} containingType
               && IsOrImplementsInterface(containingType, "VoxelGame.GUI.Bindings.IValueSource<T>");
    }

    private static Boolean IsGetValue2Invocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
    {
        if (IsGetValueInvocationBase(invocationExpressionSyntax, semanticModel) is not {} methodSymbol)
            return false;

        if (methodSymbol is not {Name: "GetValue", Parameters.Length: 1})
            return false;

        return methodSymbol.ContainingType is {} containingType
               && IsOrImplementsInterface(containingType, "VoxelGame.GUI.Bindings.IValueSource<TIn, TOut>");
    }

    private static Boolean IsOrImplementsInterface(ITypeSymbol typeSymbol, String interfaceDisplayName)
    {
        return typeSymbol.OriginalDefinition.ToDisplayString() == interfaceDisplayName
               || typeSymbol.AllInterfaces.Any(i => i.OriginalDefinition.ToDisplayString() == interfaceDisplayName);
    }

    private static IMethodSymbol? IsGetValueInvocationBase(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
    {
        String? methodName = invocationExpressionSyntax.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => null
        };

        if (methodName != "GetValue")
            return null;

        return semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
    }
}

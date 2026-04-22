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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VoxelGame.Analyzers.Utilities;

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

        if (IsGetValueInvocation(invocationExpressionSyntax, semanticModel, parametersLength: 0, Constants.ValueSource) || IsGetValueInvocation(invocationExpressionSyntax, semanticModel, parametersLength: 1, Constants.ValueSource2))
            found = invocationExpressionSyntax.GetLocation();
    }

    private static Boolean IsGetValueInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Int32 parametersLength, String interfaceDisplayName)
    {
        String? methodName = invocationExpressionSyntax.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => null
        };

        if (methodName != "GetValue")
            return false;

        if (semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is not IMethodSymbol methodSymbol)
            return false;

        if (methodSymbol is not {Name: "GetValue"})
            return false;

        if (methodSymbol.Parameters.Length != parametersLength)
            return false;

        return methodSymbol.ContainingType is {} containingType
               && AnalyzerTools.IsOrImplementsInterface(containingType, interfaceDisplayName);
    }
}

// <copyright file="LateInitializationGenerator.cs" company="VoxelGame">
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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.Annotations.Attributes;
using VoxelGame.SourceGenerators.Utilities;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
///     Generates the implementation for partial properties marked with <see cref="LateInitializationAttribute" />.
/// </summary>
[Generator]
public sealed class LateInitializationGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(LateInitializationAttribute).FullName ?? nameof(LateInitializationAttribute);

        IncrementalValuesProvider<PropertyModel?> models = context.SyntaxProvider.ForAttributeWithMetadataName(attributeName,
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(models,
            static (spc, source) => Execute(source, spc));
    }

    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is PropertyDeclarationSyntax;
    }

    private static PropertyModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        return GetPropertyModel(context.SemanticModel, (PropertyDeclarationSyntax) context.TargetNode);
    }

    private static PropertyModel? GetPropertyModel(SemanticModel semanticModel, MemberDeclarationSyntax declarationSyntax)
    {
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, declarationSyntax) is not IPropertySymbol propertySymbol)
            return null;

        if (!declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            return null;

        ContainingType? containingType = SyntaxTools.GetContainingType(declarationSyntax, semanticModel);
        String @namespace = SyntaxTools.GetNamespace(declarationSyntax);
        String declaringType = propertySymbol.ContainingType.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String type = propertySymbol.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String accessibility = SyntaxFacts.GetText(propertySymbol.DeclaredAccessibility);
        String name = propertySymbol.Name;

        Boolean isStatic = propertySymbol.IsStatic;

        String getAccessibility = propertySymbol.GetMethod != null ? SyntaxFacts.GetText(propertySymbol.GetMethod.DeclaredAccessibility) : accessibility;
        String setAccessibility = propertySymbol.SetMethod != null ? SyntaxFacts.GetText(propertySymbol.SetMethod.DeclaredAccessibility) : accessibility;

        return new PropertyModel(containingType, declaringType, @namespace, accessibility, type, name, isStatic, getAccessibility, setAccessibility);
    }

    private static void Execute(PropertyModel? model, SourceProductionContext context)
    {
        if (model is not {} value) return;

        String result = GenerateSource(value);

        context.AddSource($"{NameTools.SanitizeForIO(value.DeclaringType)}_{NameTools.SanitizeForIO(value.Name)}_LateInitialization.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static String GenerateSource(PropertyModel model)
    {
        StringBuilder sb = new();

        sb.AppendPreamble<LateInitializationGenerator>().AppendNamespace(model.Namespace);

        var backingFieldName = $"@__{NameTools.ConvertPascalCaseToCamelCase(model.Name)}";

        String staticModifier = model.IsStatic ? "static " : "";

        String getAccessibility = model.GetAccessibility != model.Accessibility ? $"{model.GetAccessibility} " : "";
        String setAccessibility = model.SetAccessibility != model.Accessibility ? $"{model.SetAccessibility} " : "";

        sb.AppendNestedClass(model.ContainingType,
            (c, i) =>
            {
                c.Append($$"""
                           {{i}}private {{staticModifier}}{{model.Type}}? {{backingFieldName}};

                           {{i}}{{model.Accessibility}} {{staticModifier}}partial {{model.Type}} {{model.Name}}
                           {{i}}{
                           {{i}}    {{getAccessibility}}get => {{backingFieldName}} 
                           {{i}}        ?? throw new global::System.InvalidOperationException($"Property '{nameof({{model.Name}})}' is used before being initialized.");

                           {{i}}    {{setAccessibility}}set
                           {{i}}    {
                           {{i}}        if ({{backingFieldName}} is not null)
                           {{i}}            throw new global::System.InvalidOperationException($"Property '{nameof({{model.Name}})}' is already initialized.");

                           {{i}}        {{backingFieldName}} = value;
                           {{i}}     }
                           {{i}}} 
                           """);
            });

        return sb.ToString();
    }

    private record struct PropertyModel(
        ContainingType? ContainingType,
        String DeclaringType,
        String Namespace,
        String Accessibility,
        String Type,
        String Name,
        Boolean IsStatic,
        String GetAccessibility,
        String SetAccessibility);
}

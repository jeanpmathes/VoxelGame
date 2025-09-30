// <copyright file="LateInitializationGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.Annotations;
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

    private static PropertyModel? GetPropertyModel(SemanticModel semanticModel, PropertyDeclarationSyntax propertyDeclarationSyntax)
    {
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, propertyDeclarationSyntax) is not IPropertySymbol propertySymbol)
            return null;

        var isPartial = false;

        foreach (SyntaxToken modifier in propertyDeclarationSyntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.StaticKeyword))
                return null;

            if (modifier.IsKind(SyntaxKind.PartialKeyword))
                isPartial = true;
        }

        if (!isPartial)
            return null;

        ContainingType? containingType = SyntaxTools.GetContainingType(propertyDeclarationSyntax, semanticModel);
        String @namespace = SyntaxTools.GetNamespace(propertyDeclarationSyntax);
        String declaringType = propertySymbol.ContainingType.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String type = propertySymbol.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String accessibility = SyntaxFacts.GetText(propertySymbol.DeclaredAccessibility);
        String name = propertySymbol.Name;

        return new PropertyModel(containingType, declaringType, @namespace, accessibility, type, name);
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

        sb.AppendNestedClass(model.ContainingType,
            (c, i) =>
            {
                c.Append($$"""
                           {{i}}private {{model.Type}}? {{backingFieldName}};

                           {{i}}{{model.Accessibility}} partial {{model.Type}} {{model.Name}}
                           {{i}}{
                           {{i}}    get => {{backingFieldName}} ?? throw new global::System.InvalidOperationException($"Property '{nameof({{model.Name}})}' is used before being initialized.");
                           {{i}}    set
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

    private record struct PropertyModel(ContainingType? ContainingType, String DeclaringType, String Namespace, String Accessibility, String Type, String Name);
}

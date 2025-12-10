// <copyright file="ConstructibleGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.Annotations.Attributes;
using VoxelGame.SourceGenerators.Utilities;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
///     Generates constructible implementations for constructors marked with <see cref="ConstructibleAttribute" />.
/// </summary>
[Generator]
public sealed class ConstructibleGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(ConstructibleAttribute).FullName ?? nameof(ConstructibleAttribute);

        IncrementalValuesProvider<ConstructorModel?> models = context.SyntaxProvider.ForAttributeWithMetadataName(
                attributeName,
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static model => model is not null);

        context.RegisterSourceOutput(models, 
            static (spc, source) => Execute(source, spc));
    }
    
    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ConstructorDeclarationSyntax;
    }

    private static ConstructorModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ConstructorDeclarationSyntax constructorDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(constructorDeclaration) is not {} constructorSymbol)
            return null;

        if (constructorSymbol.MethodKind != MethodKind.Constructor || constructorSymbol.IsStatic || constructorSymbol.Parameters.Length == 0)
            return null;

        if (HasUnsupportedParameterKind(constructorSymbol)) 
            return null;

        if (constructorDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            return null;
        
        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not {} typeSymbol)
            return null;

        ContainingType? containingType = SyntaxTools.GetContainingType(typeDeclaration, context.SemanticModel);
        String @namespace = SyntaxTools.GetNamespace(typeDeclaration);
        
        ImmutableArray<ParameterModel> parameters = CreateParameterModels(constructorSymbol);

        if (parameters.IsDefaultOrEmpty)
            return null;
        
        return new ConstructorModel(
            containingType,
            @namespace,
            SyntaxFacts.GetText(typeSymbol.DeclaredAccessibility),
            typeDeclaration.Keyword.ValueText,
            typeSymbol.Name,
            typeSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat),
            typeDeclaration.TypeParameterList?.ToString() ?? String.Empty,
            typeDeclaration.ConstraintClauses.ToString(),
            parameters);
    }
    
    private static Boolean HasUnsupportedParameterKind(IMethodSymbol methodSymbol)
    {
        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            if (parameter.RefKind != RefKind.None || parameter.HasExplicitDefaultValue || parameter.IsParams) 
                return true;
        }

        return false;
    }

    private static ImmutableArray<ParameterModel> CreateParameterModels(IMethodSymbol methodSymbol)
    {
        ImmutableArray<ParameterModel>.Builder parameterBuilder = ImmutableArray.CreateBuilder<ParameterModel>();
        
        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            parameterBuilder.Add(new ParameterModel(parameter.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat)));
        }
        
        return parameterBuilder.ToImmutable();
    }

    private static void Execute(ConstructorModel? model, SourceProductionContext context)
    {
        if (model is not { } constructorModel) return;
        
        var constructorMethod = ConstructorMethod.Create(constructorModel.Parameters);
        
        String result = GenerateSource(constructorModel, constructorMethod);
        
        context.AddSource($"{NameTools.SanitizeForIO(constructorModel.TypeFullName)}_{constructorMethod.SignatureHint}_Constructible.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static String GenerateSource(ConstructorModel constructorModel, ConstructorMethod constructorMethod)
    {
        StringBuilder sb = new();

        sb.AppendPreamble<ConstructibleGenerator>()
          .AppendNamespace(constructorModel.Namespace);

        sb.AppendNestedClass(constructorModel.ContainingType, (builder, i) =>
        {
            builder
                .Append($"{i}{constructorModel.TypeAccessibility} partial {constructorModel.TypeKeyword} {constructorModel.TypeName}{constructorModel.TypeParameters}")
                .Append($" : {constructorMethod.GetConstructibleInterface(constructorModel.TypeFullName)}");

            if (!String.IsNullOrWhiteSpace(constructorModel.TypeConstraints))
                builder.Append($"{i}{constructorModel.TypeConstraints}");

            builder.Append($$"""
                             
                             {{i}}{
                             {{i}}    /// <inheritdoc />
                             {{i}}    public static {{constructorModel.TypeFullName}} Construct({{constructorMethod.ParameterList}})
                             {{i}}    {
                             {{i}}        return new {{constructorModel.TypeFullName}}({{constructorMethod.ArgumentList}});
                             {{i}}    }
                             {{i}}}
                             """);
        });

        return sb.ToString();
    }

    private record struct ConstructorModel(
        ContainingType? ContainingType,
        String Namespace,
        String TypeAccessibility,
        String TypeKeyword,
        String TypeName,
        String TypeFullName,
        String TypeParameters,
        String TypeConstraints,
        ImmutableArray<ParameterModel> Parameters);

    private record struct ParameterModel(String TypeName);

    private record struct ConstructorMethod(
        ImmutableArray<String> ParameterTypes,
        String ParameterList,
        String ArgumentList,
        String SignatureHint)
    {
        public static ConstructorMethod Create(ImmutableArray<ParameterModel> parameters)
        {
            ImmutableArray<String> parameterTypes = [..parameters.Select(static model => model.TypeName)];

            String parameterList;
            String argumentList;

            switch (parameterTypes.Length)
            {
                case 1:
                    parameterList = $"{parameterTypes[index: 0]} input";
                    argumentList = "input";

                    break;

                case 2:
                    parameterList = $"{parameterTypes[index: 0]} input1, {parameterTypes[index: 1]} input2";
                    argumentList = "input1, input2";

                    break;

                default:
                {
                    var tupleType = $"({String.Join(", ", parameterTypes.Skip(count: 1))})";

                    parameterList = $"{parameterTypes[index: 0]} input1, {tupleType} input2";

                    ImmutableArray<String>.Builder builder = ImmutableArray.CreateBuilder<String>(parameterTypes.Length);
                    builder.Add("input1");

                    for (var i = 1; i < parameterTypes.Length; i++)
                        builder.Add($"input2.Item{i}");

                    argumentList = String.Join(", ", builder.ToImmutable());

                    break;
                }
            }

            String signatureHint = String.Join("_", parameterTypes.Select(NameTools.SanitizeForIO));

            return new ConstructorMethod(parameterTypes, parameterList, argumentList, signatureHint);
        }

        public String GetConstructibleInterface(String typeDisplay)
        {
            const String interfacePrefix = "global::VoxelGame.Toolkit.Utilities.IConstructible";

            return ParameterTypes.Length switch
            {
                1 => $"{interfacePrefix}<{ParameterTypes[index: 0]}, {typeDisplay}>",
                2 => $"{interfacePrefix}<{ParameterTypes[index: 0]}, {ParameterTypes[index: 1]}, {typeDisplay}>",
                _ => $"{interfacePrefix}<{ParameterTypes[index: 0]}, ({String.Join(", ", ParameterTypes.Skip(count: 1))}), {typeDisplay}>"
            };
        }
    }
}

// <copyright file="GenerateRecordGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.Annotations.Attributes;
using VoxelGame.SourceGenerators.Utilities;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
///     Generates a record that implements an interface marked with <see cref="GenerateRecordAttribute" />.
/// </summary>
[Generator]
public sealed class GenerateRecordGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(GenerateRecordAttribute).FullName ?? nameof(GenerateRecordAttribute);

        IncrementalValuesProvider<InterfaceModel?> models = context.SyntaxProvider.ForAttributeWithMetadataName(
                attributeName,
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(models, static (spc, model) =>
        {
            if (model is null) return;
            Execute(model.Value, spc);
        });
    }
    
    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax;
    }

    private static InterfaceModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        return GetInterfaceModel(context);
    }

    private static InterfaceModel? GetInterfaceModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetNode is not InterfaceDeclarationSyntax ids) return null;
        if (ctx.SemanticModel.GetDeclaredSymbol(ids) is not {} namedTypeSymbol) return null;

        if (namedTypeSymbol.IsGenericType) return null;
        
        String @namespace = SyntaxTools.GetNamespace(ids);
        ContainingType? containingType = SyntaxTools.GetContainingType(ids, ctx.SemanticModel);

        ImmutableArray<PropertyModel>.Builder propertyArrayBuilder = ImmutableArray.CreateBuilder<PropertyModel>();

        foreach (IPropertySymbol? member in namedTypeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            String typeDisplay = member.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
            
            Boolean isReference = member.Type.IsReferenceType;
            Boolean isNullableAnnotated = member.NullableAnnotation == NullableAnnotation.Annotated;

            propertyArrayBuilder.Add(new PropertyModel(
                member.Name,
                typeDisplay,
                isReference,
                isNullableAnnotated
            ));
        }
        
        String interfaceDisplay = namedTypeSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String interfaceName = namedTypeSymbol.Name;
        
        String? baseType = null;
        var isBaseTypeGeneric = false;
        
        if (ctx.Attributes.Length != 1 || ctx.Attributes[0].AttributeClass == null)
            return null;

        AttributeData attribute = ctx.Attributes[0];
        
        // ReSharper disable once MergeIntoPattern
        if (attribute.ConstructorArguments is {Length: 1} array && array[0] is {Kind: TypedConstantKind.Type, Value: INamedTypeSymbol baseTypeSymbol})
        {
            if (!baseTypeSymbol.IsGenericType || !baseTypeSymbol.IsUnboundGenericType)
            {
                baseType = baseTypeSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
                isBaseTypeGeneric = false;
            }
            else if (baseTypeSymbol is {IsUnboundGenericType: true, Arity: 1})
            {
                baseType = baseTypeSymbol.ConstructedFrom.ToDisplayString(SourceCodeTools.SymbolDisplayFormatWithoutGenericTypeArguments);
                isBaseTypeGeneric = true;
            }
            else return null;
        }
        else if (attribute.ConstructorArguments.Length != 0) return null;
        
        return new InterfaceModel(
            ContainingType: containingType,
            Namespace: @namespace,
            InterfaceDisplay: interfaceDisplay,
            InterfaceName: interfaceName,
            propertyArrayBuilder.ToImmutable(),
            BaseType: baseType,
            IsBaseTypeGeneric: isBaseTypeGeneric
        );
    }

    private static void Execute(InterfaceModel model, SourceProductionContext context)
    {
        String source = GenerateSource(model);
        
        context.AddSource($"{NameTools.SanitizeForIO(model.InterfaceDisplay)}_Record.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static String GenerateSource(InterfaceModel model)
    {
        var sb = new StringBuilder();

        sb.AppendPreamble<GenerateRecordGenerator>()
          .AppendNamespace(model.Namespace);

        sb.AppendNestedClass(model.ContainingType, (c, i) =>
        {
            String recordName = DeriveRecordName(model.InterfaceName);
            
            StringBuilder implements = new(" : ");
            
            if (model.BaseType != null)
            {
                String baseType = model.IsBaseTypeGeneric 
                    ? $"{model.BaseType}<{recordName}>"
                    : model.BaseType;
                implements.Append(baseType).Append(", ");
            }
            
            implements.Append(model.InterfaceDisplay);
            
            Boolean isNested = model.ContainingType != null;

            c.AppendLine($$"""
                          {{i}}/// <summary>
                          {{i}}///     Implementation of <see cref="{{model.InterfaceDisplay}}" />.
                          {{i}}/// </summary>
                          {{i}}{{(isNested ? "private" : "public")}} partial record {{recordName}}{{implements}}
                          {{i}}{
                          """);

            foreach (PropertyModel propertyModel in model.Properties)
            {
                Boolean needsNullInitializer = propertyModel is {IsReferenceType: true, IsNullableAnnotated: false};

                c.Append($"{i}    public {propertyModel.TypeDisplay} {propertyModel.Name} {{ get; set; }}");
                
                if (needsNullInitializer)
                    c.Append(" = null!;");

                c.AppendLine();
            }

            c.Append($"{i}}}");
        });

        return sb.ToString();
    }

    private static String DeriveRecordName(String interfaceName)
    {
        // ReSharper disable once MergeIntoPattern
        if (interfaceName.Length >= 2 && interfaceName[0] == 'I' && Char.IsUpper(interfaceName[1]))
            return interfaceName.Substring(1);
        
        return interfaceName + "Record";
    }

    private record struct InterfaceModel(ContainingType? ContainingType, String Namespace, String InterfaceDisplay, String InterfaceName, ImmutableArray<PropertyModel> Properties, String? BaseType, Boolean IsBaseTypeGeneric);

    private record struct PropertyModel(String Name, String TypeDisplay, Boolean IsReferenceType, Boolean IsNullableAnnotated);
}

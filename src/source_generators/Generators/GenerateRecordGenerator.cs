// <copyright file="GenerateRecordGenerator.cs" company="VoxelGame">
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

        context.RegisterSourceOutput(models,
            static (spc, model) =>
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
        if (ctx.TargetNode is not InterfaceDeclarationSyntax interfaceDeclarationSyntax) return null;
        if (ctx.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not {} namedTypeSymbol) return null;

        if (namedTypeSymbol.IsGenericType) return null;

        String @namespace = SyntaxTools.GetNamespace(interfaceDeclarationSyntax);
        ContainingType? containingType = SyntaxTools.GetContainingType(interfaceDeclarationSyntax, ctx.SemanticModel);

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

        if (ctx.Attributes.Length != 1 || ctx.Attributes[index: 0].AttributeClass == null)
            return null;

        AttributeData attribute = ctx.Attributes[index: 0];

        // ReSharper disable once MergeIntoPattern
        if (attribute.ConstructorArguments is {Length: 1} array && array[index: 0] is {Kind: TypedConstantKind.Type, Value: INamedTypeSymbol baseTypeSymbol})
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
            else
            {
                return null;
            }
        }
        else if (attribute.ConstructorArguments.Length != 0)
        {
            return null;
        }

        return new InterfaceModel(
            containingType,
            @namespace,
            interfaceDisplay,
            interfaceName,
            propertyArrayBuilder.ToImmutable(),
            baseType,
            isBaseTypeGeneric
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

        sb.AppendNestedClass(model.ContainingType,
            (c, i) =>
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
                               {{i}}{{(isNested ? "private" : "public")}} sealed partial record {{recordName}}{{implements}}
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
        if (interfaceName.Length >= 2 && interfaceName[index: 0] == 'I' && Char.IsUpper(interfaceName[index: 1]))
            return interfaceName.Substring(startIndex: 1);

        return interfaceName + "Record";
    }

    private record struct InterfaceModel(
        ContainingType? ContainingType,
        String Namespace,
        String InterfaceDisplay,
        String InterfaceName,
        ImmutableArray<PropertyModel> Properties,
        String? BaseType,
        Boolean IsBaseTypeGeneric);

    private record struct PropertyModel(
        String Name,
        String TypeDisplay,
        Boolean IsReferenceType,
        Boolean IsNullableAnnotated);
}

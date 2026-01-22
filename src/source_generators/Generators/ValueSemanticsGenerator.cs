// <copyright file="ValueSemanticsGenerator.cs" company="VoxelGame">
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
///     Generates interface implementations for structs which have value semantics.
///     This includes implementations for equality and default values.
///     While using records is generally preferred, this generator also targets custom interfaces and can be used when
///     records are not an option.
/// </summary>
[Generator]
public sealed class ValueSemanticsGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(ValueSemanticsAttribute).FullName ?? nameof(ValueSemanticsAttribute);

        IncrementalValuesProvider<StructModel?> models = context.SyntaxProvider.ForAttributeWithMetadataName(
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
        return node is StructDeclarationSyntax;
    }

    private static StructModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        return GetStructModel(context);
    }

    private static StructModel? GetStructModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetNode is not StructDeclarationSyntax structDeclarationSyntax) return null;
        if (ctx.SemanticModel.GetDeclaredSymbol(structDeclarationSyntax) is not {} namedTypeSymbol) return null;

        if (namedTypeSymbol.IsGenericType) return null;

        String @namespace = SyntaxTools.GetNamespace(structDeclarationSyntax);
        ContainingType? containingType = SyntaxTools.GetContainingType(structDeclarationSyntax, ctx.SemanticModel);

        ImmutableArray<FieldModel>.Builder fieldModelArrayBuilder = ImmutableArray.CreateBuilder<FieldModel>();

        foreach (IFieldSymbol? member in namedTypeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            String typeDisplay = member.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);

            fieldModelArrayBuilder.Add(new FieldModel(member.Name, typeDisplay));
        }

        String display = namedTypeSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
        String name = namedTypeSymbol.Name;
        String accessibility = SyntaxFacts.GetText(namedTypeSymbol.DeclaredAccessibility);

        if (ctx.Attributes.Length != 1 || ctx.Attributes[index: 0].AttributeClass == null)
            return null;

        return new StructModel(
            containingType,
            @namespace,
            display,
            name,
            accessibility,
            fieldModelArrayBuilder.ToImmutable()
        );
    }

    private static void Execute(StructModel model, SourceProductionContext context)
    {
        String source = GenerateSource(model);

        context.AddSource($"{NameTools.SanitizeForIO(model.Display)}_ValueSemantics.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static String GenerateSource(StructModel model)
    {
        var sb = new StringBuilder();

        sb.AppendPreamble<ValueSemanticsGenerator>()
            .AppendNamespace(model.Namespace);

        sb.AppendNestedClass(model.ContainingType,
            (c, i) =>
            {
                (String packTupleType, String packTupleValues) = GetPackStrings(model);

                c.Append($$"""
                           {{i}}{{model.Accessibility}} partial struct {{model.Name}} : global::System.IEquatable<{{model.Name}}>, global::VoxelGame.Toolkit.Utilities.IDefault<{{model.Name}}>
                           {{i}}{
                           {{i}}    /// <inheritdoc />
                           {{i}}    public static {{model.Name}} Default => new();

                           {{i}}    private {{packTupleType}} Pack => {{packTupleValues}};

                           {{i}}    /// <inheritdoc />
                           {{i}}    public override global::System.Boolean Equals(global::System.Object? obj)
                           {{i}}    {
                           {{i}}        return obj is {{model.Name}} other && Equals(other);
                           {{i}}    }

                           {{i}}    /// <inheritdoc />
                           {{i}}    public global::System.Boolean Equals({{model.Name}} other)
                           {{i}}    {
                           {{i}}        return Pack.Equals(other.Pack);
                           {{i}}    }

                           {{i}}    /// <inheritdoc />
                           {{i}}    public override global::System.Int32 GetHashCode()
                           {{i}}    {
                           {{i}}        return Pack.GetHashCode();
                           {{i}}    }

                           {{i}}    /// <summary>
                           {{i}}    ///     Equality operator.
                           {{i}}    /// </summary>
                           {{i}}    public static global::System.Boolean operator ==({{model.Name}} left, {{model.Name}} right) => left.Equals(right);

                           {{i}}    /// <summary>
                           {{i}}    ///     Inequality operator.
                           {{i}}    /// </summary>
                           {{i}}    public static global::System.Boolean operator !=({{model.Name}} left, {{model.Name}} right) => !(left == right);
                           {{i}}}
                           """);
            });

        return sb.ToString();
    }

    private static (String, String) GetPackStrings(StructModel model)
    {
        if (model.Fields.Length == 0)
            return ("global::System.ValueTuple", "global::System.ValueTuple.Create()");

        if (model.Fields.Length == 1)
            return ($"global::System.ValueTuple<{model.Fields[0].TypeDisplay}>", $"global::System.ValueTuple.Create({model.Fields[0].Name})");

        StringBuilder packTupleType = new("(");
        StringBuilder packTupleValues = new("(");

        var first = true;

        foreach (FieldModel field in model.Fields)
        {
            if (!first)
            {
                packTupleType.Append(", ");
                packTupleValues.Append(", ");
            }

            packTupleType.Append(field.TypeDisplay);
            packTupleValues.Append(field.Name);

            first = false;
        }

        packTupleType.Append(')');
        packTupleValues.Append(')');

        return (packTupleType.ToString(), packTupleValues.ToString());
    }

    private record struct StructModel(
        ContainingType? ContainingType,
        String Namespace,
        String Display,
        String Name,
        String Accessibility,
        ImmutableArray<FieldModel> Fields);

    private record struct FieldModel(String Name, String TypeDisplay);
}

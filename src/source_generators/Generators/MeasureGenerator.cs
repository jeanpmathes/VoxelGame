// <copyright file="MeasureGenerator.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.Annotations.Attributes;
using VoxelGame.SourceGenerators.Utilities;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
///     Generates measure structs for unit definitions annotated with <see cref="GenerateMeasureAttribute" />.
/// </summary>
[Generator]
public sealed class MeasureGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(GenerateMeasureAttribute).FullName ?? nameof(GenerateMeasureAttribute);

        IncrementalValuesProvider<MeasureModel?> models = context.SyntaxProvider
            .ForAttributeWithMetadataName(attributeName,
                static (node, _) => IsSyntaxTargetForGeneration(node),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static model => model is not null);

        context.RegisterSourceOutput(models, static (spc, model) => Execute(model, spc));
    }

    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is PropertyDeclarationSyntax;
    }

    private static MeasureModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not IPropertySymbol propertySymbol)
            return null;

        if (!propertySymbol.IsStatic)
            return null;

        if (!IsUnitType(propertySymbol.Type))
            return null;

        AttributeData? attributeData = GetAttribute(context);

        if (attributeData == null)
            return null;

        if (attributeData.ConstructorArguments.Length != 3)
            return null;

        if (attributeData.ConstructorArguments[index: 0].Value is not String measureName || String.IsNullOrWhiteSpace(measureName))
            return null;

        if (attributeData.ConstructorArguments[index: 1].Value is not String valuePropertyName || String.IsNullOrWhiteSpace(valuePropertyName))
            return null;

        if (attributeData.ConstructorArguments[index: 2].Value is not UInt32 allowedPrefixes)
            return null;

        GetNamedArguments(attributeData, out String? measureSummary, out String? valueSummary);

        String @namespace = propertySymbol.ContainingType.ContainingNamespace.ToDisplayString();
        String unitContainingType = propertySymbol.ContainingType.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);

        return new MeasureModel(
            String.IsNullOrWhiteSpace(@namespace) ? null : @namespace,
            measureName,
            valuePropertyName,
            allowedPrefixes,
            measureSummary,
            valueSummary,
            $"{unitContainingType}.{propertySymbol.Name}");
    }

    private static void GetNamedArguments(AttributeData attributeData, out String? measureSummary, out String? valueSummary)
    {
        measureSummary = null;
        valueSummary = null;

        foreach (KeyValuePair<String, TypedConstant> namedArgument in attributeData.NamedArguments)
            switch (namedArgument.Key)
            {
                case nameof(GenerateMeasureAttribute.MeasureSummary):
                    measureSummary = namedArgument.Value.Value as String;

                    break;
                case nameof(GenerateMeasureAttribute.ValueSummary):
                    valueSummary = namedArgument.Value.Value as String;

                    break;
            }
    }

    private static AttributeData? GetAttribute(GeneratorAttributeSyntaxContext context)
    {
        foreach (AttributeData attribute in context.Attributes)
        {
            if (attribute.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name != nameof(GenerateMeasureAttribute)
                && attributeClass.ToDisplayString() != typeof(GenerateMeasureAttribute).FullName)
                continue;

            return attribute;
        }

        return null;
    }

    private static Boolean IsUnitType(ISymbol symbol)
    {
        return symbol.Name == "Unit" && symbol.ContainingNamespace.ToDisplayString() == "VoxelGame.Core.Utilities.Units";
    }

    private static void Execute(MeasureModel? model, SourceProductionContext context)
    {
        if (model is not {} value)
            return;

        String source = GenerateSource(value);

        context.AddSource($"{NameTools.SanitizeForIO(value.MeasureName)}_Measure.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static String GenerateSource(MeasureModel model)
    {
        StringBuilder sb = new();

        sb.AppendPreamble<MeasureGenerator>().AppendNamespace(model.Namespace);

        sb.Append($$"""
                    /// <summary>
                    ///     {{model.MeasureSummary ?? "A measure."}}
                    /// </summary>
                    public readonly partial struct {{model.MeasureName}} 
                        : global::VoxelGame.Core.Utilities.Units.IMeasure
                        , global::System.IEquatable<{{model.MeasureName}}>
                        , global::System.IComparable<{{model.MeasureName}}>
                        , global::System.Numerics.IComparisonOperators<{{model.MeasureName}}, {{model.MeasureName}}, global::System.Boolean>
                    {
                        /// <summary>
                        ///     {{model.ValueSummary ?? "The value of the measure."}}
                        /// </summary>
                        public global::System.Double {{model.ValuePropertyName}} { get; init; }

                        /// <inheritdoc />
                        public static global::VoxelGame.Core.Utilities.Units.Unit Unit 
                            => {{model.UnitPropertyAccess}};

                        /// <inheritdoc />
                        public static global::VoxelGame.Annotations.Definitions.AllowedPrefixes Prefixes 
                            => (global::VoxelGame.Annotations.Definitions.AllowedPrefixes) {{model.AllowedPrefixes}};

                        /// <inheritdoc />
                        global::System.Double global::VoxelGame.Core.Utilities.Units.IMeasure.Value 
                            => {{model.ValuePropertyName}};

                        /// <inheritdoc />
                        public global::System.Boolean Equals({{model.MeasureName}} other)
                        {
                            return {{model.ValuePropertyName}}.Equals(other.{{model.ValuePropertyName}});
                        }

                        /// <inheritdoc />
                        public override global::System.Boolean Equals(global::System.Object? obj)
                        {
                            return obj is {{model.MeasureName}} other && Equals(other);
                        }

                        /// <inheritdoc />
                        public global::System.Int32 CompareTo({{model.MeasureName}} other)
                        {
                            return {{model.ValuePropertyName}}.CompareTo(other.{{model.ValuePropertyName}});
                        }

                        /// <inheritdoc />
                        public override global::System.Int32 GetHashCode()
                        {
                            return {{model.ValuePropertyName}}.GetHashCode();
                        }

                        /// <summary>
                        ///     Equality operator.
                        /// </summary>
                        public static global::System.Boolean operator ==({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return left.Equals(right);
                        }

                        /// <summary>
                        ///     Inequality operator.
                        /// </summary>
                        public static global::System.Boolean operator !=({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return !left.Equals(right);
                        }

                        /// <summary>
                        ///     Greater-than operator.
                        /// </summary>
                        public static global::System.Boolean operator >({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return left.{{model.ValuePropertyName}} > right.{{model.ValuePropertyName}};
                        }

                        /// <summary>
                        ///     Less-than operator.
                        /// </summary>
                        public static global::System.Boolean operator <({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return left.{{model.ValuePropertyName}} < right.{{model.ValuePropertyName}};
                        }

                        /// <summary>
                        ///     Greater-than-or-equal operator.
                        /// </summary>
                        public static global::System.Boolean operator >=({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return left.{{model.ValuePropertyName}} >= right.{{model.ValuePropertyName}};
                        }

                        /// <summary>
                        ///     Less-than-or-equal operator.
                        /// </summary>
                        public static global::System.Boolean operator <=({{model.MeasureName}} left, {{model.MeasureName}} right)
                        {
                            return left.{{model.ValuePropertyName}} <= right.{{model.ValuePropertyName}};
                        }

                        /// <inheritdoc />
                        public override global::System.String ToString()
                        {
                            return ToString(format: null);
                        }

                        /// <summary>
                        ///     Convert the measure to a string.
                        /// </summary>
                        /// <param name="format">The format provider.</param>
                        /// <returns>The string representation of the measure.</returns>
                        public global::System.String ToString(global::System.IFormatProvider? format)
                        {
                            return global::VoxelGame.Core.Utilities.Units.IMeasure.ToString(this, format);
                        }
                    }

                    """);

        return sb.ToString();
    }

    private record struct MeasureModel(
        String? Namespace,
        String MeasureName,
        String ValuePropertyName,
        UInt32 AllowedPrefixes,
        String? MeasureSummary,
        String? ValueSummary,
        String UnitPropertyAccess);
}

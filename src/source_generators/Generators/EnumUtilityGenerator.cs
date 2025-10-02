// <copyright file="EnumUtilityGenerator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VoxelGame.SourceGenerators.Utilities;

namespace VoxelGame.SourceGenerators.Generators;

/// <summary>
///     Generates helpful utilities for enums.
/// </summary>
[Generator]
public class EnumUtilityGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumModel?> models = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(models,
            static (spc, source) => Execute(source, spc));
    }

    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is EnumDeclarationSyntax;
    }

    private static EnumModel? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        return GetEnumModel(context.SemanticModel, (EnumDeclarationSyntax) context.Node);
    }

    private static EnumModel? GetEnumModel(SemanticModel semanticModel, EnumDeclarationSyntax enumDeclarationSyntax)
    {
        if (ModelExtensions.GetDeclaredSymbol(semanticModel, enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            return null;

        if (enumSymbol.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
            return null;

        String accessibility = SyntaxFacts.GetText(enumSymbol.DeclaredAccessibility);

        ContainingType? containingType = SyntaxTools.GetContainingType(enumDeclarationSyntax, semanticModel);
        String @namespace = SyntaxTools.GetNamespace(enumDeclarationSyntax);
        String name = enumSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);

        if (GetNumberOfGenericNestingLevels(containingType) > 1)
            return null;

        ImmutableArray<ISymbol> members = enumSymbol.GetMembers();
        List<EnumMember> memberModel = new(members.Length);

        foreach (ISymbol member in members)
        {
            if (member is IFieldSymbol {ConstantValue: {} value})
            {
                memberModel.Add(new EnumMember(member.Name, IntegerConstant.From(enumSymbol.EnumUnderlyingType, value)));
            }
        }

        var isFlag = false;

        foreach (AttributeData attribute in enumSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != typeof(FlagsAttribute).FullName) continue;

            isFlag = true;

            break;
        }

        return new EnumModel(containingType, @namespace, accessibility, name, isFlag, memberModel);
    }

    private static Int32 GetNumberOfGenericNestingLevels(ContainingType? containingType)
    {
        var levels = 0;

        while (containingType != null)
        {
            if (containingType.TypeParameters != null)
                levels++;

            containingType = containingType.Child;
        }

        return levels;
    }

    private static void Execute(EnumModel? model, SourceProductionContext context)
    {
        if (model is not {} value) return;

        String result = GenerateSourceCode(value);

        context.AddSource($"{NameTools.SanitizeForIO(value.Name)}_EnumUtility.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static String GenerateSourceCode(EnumModel model)
    {
        StringBuilder sb = new();

        sb.AppendPreamble<EnumUtilityGenerator>().AppendNamespace(model.Namespace);

        (String? typeParameters, String? constraints) = GetTypeParameterAndConstraints(model.ContainingType);

        sb.Append($$"""
                    /// <ignore />
                    public static partial class EnumExtensions
                    {
                        /// <summary>
                        /// A faster alternative to <see cref="global::System.Object.ToString"/> for the enum <see cref="{{NameTools.SanitizeForDocumentationReference(model.Name)}}"/>.
                        /// </summary>
                        {{model.Accessibility}} static global::System.String ToStringFast{{typeParameters}}(this {{model.Name}} value) {{constraints}}
                            => value switch
                            {

                    """);

        HashSet<IntegerConstant> coveredForSwitch = [];

        foreach (EnumMember member in model.Members)
        {
            if (!coveredForSwitch.Add(member.Value)) continue;

            sb.Append($"""
                                   {model.Name}.{member.Name} => nameof({model.Name}.{member.Name}),

                       """);
        }

        sb.Append($$"""

                                _ => {{(model.IsFlag ? $"FormatFlags{typeParameters}(value)" : "value.ToString()")}}
                            };
                    """);

        if (model.IsFlag)
        {
            sb.Append($$"""

                            private static global::System.String FormatFlags{{typeParameters}}({{model.Name}} value) {{constraints}}
                            {
                                global::System.UInt64 remaining = unchecked((global::System.UInt64)value);
                                global::System.UInt64 current = 0UL;
                                
                                if (remaining == 0UL)
                                    return "0";
                                    
                                global::System.Text.StringBuilder sb = new();
                                global::System.Boolean first = true;
                        """);

            HashSet<IntegerConstant> coveredForFlag = [];

            foreach (EnumMember member in model.Members)
            {
                if (!member.Value.IsFlag) continue;
                if (!coveredForFlag.Add(member.Value)) continue;

                sb.Append($$"""
                                    
                                    
                                    current = unchecked((global::System.UInt64){{model.Name}}.{{member.Name}});
                                    if ((remaining & current) == current)
                                    {    
                                        remaining &= ~current;
                                        
                                        if (!first) sb.Append(", ");
                                        first = false;
                                        
                                        sb.Append(nameof({{model.Name}}.{{member.Name}}));
                                    }
                            """);
            }

            sb.Append("""
                                    
                                    
                              return remaining == 0UL ? sb.ToString() : value.ToString();
                          }
                      """);
        }

        sb.Append("""

                  }

                  """);

        return sb.ToString();
    }

    private static (String? typeParameters, String? constraints) GetTypeParameterAndConstraints(ContainingType? containingType)
    {
        while (containingType != null)
        {
            if (containingType.TypeParameters != null)
                return (containingType.TypeParameters, containingType.Constraints);

            containingType = containingType.Child;
        }

        return (null, null);
    }

    private record struct EnumMember(String Name, IntegerConstant Value);

    private record struct EnumModel(ContainingType? ContainingType, String Namespace, String Accessibility, String Name, Boolean IsFlag, ImmutableArray<EnumMember> Members)
    {
        public EnumModel(ContainingType? containingType, String @namespace, String accessibility, String name, Boolean isFlag, List<EnumMember> members)
            : this(containingType, @namespace, accessibility, name, isFlag, members.ToImmutableArray()) {}
    }
}

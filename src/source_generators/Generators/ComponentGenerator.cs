// <copyright file="ComponentGenerator.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
///     Generates component implementations for classes marked with <see cref="ComponentSubjectAttribute" />.
/// </summary>
[Generator]
public sealed class ComponentGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        String attributeName = typeof(ComponentSubjectAttribute).FullName ?? nameof(ConstructibleAttribute);

        IncrementalValuesProvider<ComponentSubjectModel?> models = context.SyntaxProvider.ForAttributeWithMetadataName(
                attributeName,
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static model => model is not null);

        context.RegisterSourceOutput(models, static (spc, data) => Execute(spc, data));
    }

    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax {AttributeLists.Count: > 0};
    }

    private static ComponentSubjectModel? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not {} subjectSymbol)
            return null;

        AttributeData? attributeData = GetSubjectAttributeData(context);

        if (attributeData?.ConstructorArguments.Length != 1)
            return null;

        if (attributeData.ConstructorArguments[index: 0].Value is not INamedTypeSymbol componentSymbol)
            return null;

        String @namespace = SyntaxTools.GetNamespace(classDeclaration);
        ContainingType? containingType = SyntaxTools.GetContainingType(classDeclaration, context.SemanticModel);

        ImmutableArray<ComponentEventModel>.Builder events = ImmutableArray.CreateBuilder<ComponentEventModel>();

        foreach (IMethodSymbol method in subjectSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            AttributeData? eventAttribute = GetEventAttributeData(method);

            if (eventAttribute == null)
                continue;

            ComponentEventModel? eventModel = CreateEventModel(method, eventAttribute);

            if (eventModel != null)
                events.Add(eventModel.Value);
        }

        ComponentModel? componentModel = CreateComponentModel(componentSymbol, context.SemanticModel);

        if (componentModel == null)
            return null;

        return new ComponentSubjectModel(
            @namespace,
            containingType,
            classDeclaration.Identifier.Text,
            subjectSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat),
            classDeclaration.TypeParameterList?.ToString(),
            classDeclaration.ConstraintClauses.ToString(),
            SyntaxFacts.GetText(subjectSymbol.DeclaredAccessibility),
            events.ToImmutable(),
            componentModel.Value);
    }

    private static AttributeData? GetSubjectAttributeData(GeneratorAttributeSyntaxContext context)
    {
        foreach (AttributeData attribute in context.Attributes)
        {
            if (attribute.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name != nameof(ComponentSubjectAttribute)
                && attributeClass.ToDisplayString() != typeof(ComponentSubjectAttribute).FullName)
                continue;

            return attribute;
        }

        return null;
    }

    private static AttributeData? GetEventAttributeData(ISymbol symbol)
    {
        foreach (AttributeData attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass is not {} attributeClass)
                continue;

            if (attributeClass.Name != nameof(ComponentEventAttribute)
                && attributeClass.ToDisplayString() != typeof(ComponentEventAttribute).FullName)
                continue;

            return attribute;
        }

        return null;
    }

    private static ComponentEventModel? CreateEventModel(IMethodSymbol subjectMethod, AttributeData attribute)
    {
        String subjectMethodName = subjectMethod.Name;
        String subjectMethodAccessibility = SyntaxFacts.GetText(subjectMethod.DeclaredAccessibility);

        if (attribute.ConstructorArguments.Length != 1)
            return null;

        var componentMethodName = attribute.ConstructorArguments[index: 0].Value as String;

        if (String.IsNullOrWhiteSpace(componentMethodName))
            return null;

        ImmutableArray<ComponentEventParameterModel>.Builder parameters = ImmutableArray.CreateBuilder<ComponentEventParameterModel>();

        foreach (IParameterSymbol parameter in subjectMethod.Parameters)
        {
            String type = parameter.Type.ToDisplayString(SourceCodeTools.SymbolDisplayFormat);
            String name = parameter.Name;

            String declarationPrefix = parameter.IsParams ? "params " : String.Empty;

            String usagePrefix = parameter.RefKind switch
            {
                RefKind.Out => "out ",
                RefKind.Ref => "ref ",
                RefKind.In => "in ",
                _ => String.Empty
            };

            if (parameter.RefKind is not RefKind.None)
                declarationPrefix = usagePrefix + declarationPrefix;

            parameters.Add(new ComponentEventParameterModel(type, name, declarationPrefix, usagePrefix));
        }

        return new ComponentEventModel(
            subjectMethodName,
            componentMethodName!,
            subjectMethodAccessibility,
            parameters.ToImmutable());
    }

    private static ComponentModel? CreateComponentModel(ISymbol componentSymbol, SemanticModel semanticModel)
    {
        if (componentSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not TypeDeclarationSyntax componentTypeDeclarationSyntax)
            return null;

        String @namespace = SyntaxTools.GetNamespace(componentTypeDeclarationSyntax);
        ContainingType? containingType = SyntaxTools.GetContainingType(componentTypeDeclarationSyntax, semanticModel);

        String name = componentTypeDeclarationSyntax.Identifier.Text;
        var typeParameters = componentTypeDeclarationSyntax.TypeParameterList?.ToString();
        var constraints = componentTypeDeclarationSyntax.ConstraintClauses.ToString();

        String accessibility = SyntaxFacts.GetText(componentSymbol.DeclaredAccessibility);

        return new ComponentModel(
            @namespace,
            containingType,
            name,
            componentSymbol.ToDisplayString(SourceCodeTools.SymbolDisplayFormat),
            typeParameters,
            constraints,
            accessibility);
    }

    private static void Execute(SourceProductionContext context, ComponentSubjectModel? model)
    {
        if (model is not {} subjectModel)
            return;

        String subjectResult = GenerateSubjectSource(subjectModel);
        context.AddSource($"{NameTools.SanitizeForIO(subjectModel.FullName)}_ComponentSubject.g.cs", SourceText.From(subjectResult, Encoding.UTF8));

        String componentResult = GenerateComponentSource(subjectModel);
        context.AddSource($"{NameTools.SanitizeForIO(subjectModel.Component.FullName)}_Component.g.cs", SourceText.From(componentResult, Encoding.UTF8));
    }

    private static String GenerateSubjectSource(ComponentSubjectModel subjectModel)
    {
        StringBuilder sb = new();

        sb.AppendPreamble<ComponentGenerator>().AppendNamespace(subjectModel.Namespace);

        sb.AppendNestedClass(subjectModel.ContainingType,
            (builder, i) =>
            {
                String typeParameters = subjectModel.TypeParameters ?? String.Empty;
                String constraints = String.IsNullOrWhiteSpace(subjectModel.TypeConstraints) ? String.Empty : $" {subjectModel.TypeConstraints}";

                builder.Append($$"""
                                 {{i}}{{subjectModel.Accessibility}} partial class {{subjectModel.Name}}{{typeParameters}}{{constraints}}
                                 {{i}}{
                                 {{i}}    /// <inheritdoc />
                                 {{i}}    protected override {{subjectModel.Name}} Self => this;
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($"""


                                    {i}    private readonly global::System.Collections.Generic.HashSet<{subjectModel.Component.FullName}> {componentEvent.FieldName} = new();
                                    {i}    private readonly global::System.Collections.Generic.HashSet<{subjectModel.Component.FullName}> {componentEvent.PendingRemovalFieldName} = new();
                                    """);

                builder.Append($$"""


                                 {{i}}    private global::System.Int32 {{subjectModel.IterationDepthFieldName}};

                                 {{i}}    /// <inheritdoc />
                                 {{i}}    protected override void OnComponentAdded({{subjectModel.Component.FullName}} component)
                                 {{i}}    {
                                 {{i}}        RegisterComponentEvents(component);
                                 {{i}}    }

                                 {{i}}    /// <inheritdoc />
                                 {{i}}    protected override void OnComponentRemoved({{subjectModel.Component.FullName}} component)
                                 {{i}}    {
                                 {{i}}        UnregisterComponentEvents(component);
                                 {{i}}    }

                                 {{i}}    private void RegisterComponentEvents({{subjectModel.Component.FullName}} component)
                                 {{i}}    {
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($"""

                                    {i}        {componentEvent.FieldName}.Add(component);
                                    {i}        {componentEvent.PendingRemovalFieldName}.Remove(component);
                                    """);

                builder.Append($$"""

                                 {{i}}    }

                                 {{i}}    private void UnregisterComponentEvents({{subjectModel.Component.FullName}} component)
                                 {{i}}    {
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($"""

                                    {i}        {componentEvent.DisableMethodName}(component);
                                    """);

                builder.Append($$"""

                                 {{i}}    }
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($$"""


                                     {{i}}    internal void {{componentEvent.DisableMethodName}}({{subjectModel.Component.FullName}} component)
                                     {{i}}    {
                                     {{i}}        if ({{subjectModel.IterationDepthFieldName}} > 0)
                                     {{i}}        {
                                     {{i}}            {{componentEvent.PendingRemovalFieldName}}.Add(component);
                                     {{i}}        }
                                     {{i}}        else
                                     {{i}}        {
                                     {{i}}            {{componentEvent.PendingRemovalFieldName}}.Remove(component);
                                     {{i}}            {{componentEvent.FieldName}}.Remove(component);
                                     {{i}}        }
                                     {{i}}    }
                                     """);

                builder.Append($$"""


                                 {{i}}    private void {{subjectModel.FlushMethodName}}()
                                 {{i}}    {
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($$"""

                                     {{i}}        foreach (var component in {{componentEvent.PendingRemovalFieldName}})
                                     {{i}}        {
                                     {{i}}            {{componentEvent.FieldName}}.Remove(component);
                                     {{i}}        }
                                     {{i}}        {{componentEvent.PendingRemovalFieldName}}.Clear();
                                     """);

                builder.Append($$"""

                                 {{i}}    }
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($$"""


                                     {{i}}    {{componentEvent.SubjectMethodAccessibility}} partial void {{componentEvent.SubjectMethodName}}({{componentEvent.Signature}})
                                     {{i}}    {
                                     {{i}}        {{subjectModel.IterationDepthFieldName}} += 1;
                                     {{i}}        try
                                     {{i}}        {
                                     {{i}}            foreach (var component in {{componentEvent.FieldName}})
                                     {{i}}            {
                                     {{i}}                component.{{componentEvent.ComponentMethodName}}({{componentEvent.Invocation}});
                                     {{i}}            }
                                     {{i}}        }
                                     {{i}}        finally
                                     {{i}}        {
                                     {{i}}            {{subjectModel.IterationDepthFieldName}} -= 1;
                                     {{i}}            if ({{subjectModel.IterationDepthFieldName}} == 0)
                                     {{i}}                {{subjectModel.FlushMethodName}}();
                                     {{i}}        }
                                     {{i}}    }
                                     """);

                builder.Append($$"""

                                 {{i}}}
                                 """);
            });

        return sb.ToString();
    }

    private static String GenerateComponentSource(ComponentSubjectModel subjectModel)
    {
        ComponentModel componentModel = subjectModel.Component;

        StringBuilder sb = new();

        sb.AppendPreamble<ComponentGenerator>().AppendNamespace(componentModel.Namespace);

        sb.AppendNestedClass(componentModel.ContainingType,
            (builder, i) =>
            {
                String typeParameters = componentModel.TypeParameters ?? String.Empty;
                String constraints = String.IsNullOrWhiteSpace(componentModel.TypeConstraints) ? String.Empty : $" {componentModel.TypeConstraints}";

                builder.Append($$"""
                                 {{i}}{{componentModel.Accessibility}} partial class {{componentModel.Name}}{{typeParameters}}{{constraints}}
                                 {{i}}{
                                 """);

                foreach (ComponentEventModel componentEvent in subjectModel.Events)
                    builder.Append($$"""

                                     {{i}}    /// <inheritdoc cref="{{NameTools.SanitizeForDocumentationReference(subjectModel.FullName)}}.{{componentEvent.SubjectMethodName}}" />
                                     {{i}}    public virtual void {{componentEvent.ComponentMethodName}}({{componentEvent.Signature}})
                                     {{i}}    {
                                     {{i}}        Subject.{{componentEvent.DisableMethodName}}(this);
                                     {{i}}    }
                                     """);

                builder.Append($$"""

                                 {{i}}}
                                 """);
            });

        return sb.ToString();
    }

    // ReSharper disable once StructCanBeMadeReadOnly
    private record struct ComponentSubjectModel(
        String Namespace,
        ContainingType? ContainingType,
        String Name,
        String FullName,
        String? TypeParameters,
        String? TypeConstraints,
        String Accessibility,
        ImmutableArray<ComponentEventModel> Events,
        ComponentModel Component)
    {
        public String IterationDepthFieldName => $"@__{NameTools.ConvertPascalCaseToCamelCase(Name)}ComponentEventIterationDepth";
        public String FlushMethodName => $"FlushPending{Name}EventRemovals";
    }

    // ReSharper disable once StructCanBeMadeReadOnly
    private record struct ComponentEventModel(String SubjectMethodName, String ComponentMethodName, String SubjectMethodAccessibility, ImmutableArray<ComponentEventParameterModel> Parameters)
    {
        public String FieldName => $"@__{NameTools.ConvertPascalCaseToCamelCase(ComponentMethodName)}Components";
        public String PendingRemovalFieldName => $"@__{NameTools.ConvertPascalCaseToCamelCase(ComponentMethodName)}PendingRemoval";
        public String DisableMethodName => $"Disable_{ComponentMethodName}";

        public String Signature => String.Join(", ", Parameters.Select(p => $"{p.DeclarationPrefix}{p.Type} {p.Name}"));
        public String Invocation => String.Join(", ", Parameters.Select(p => $"{p.UsagePrefix}{p.Name}"));
    }

    private record struct ComponentEventParameterModel(
        String Type,
        String Name,
        String DeclarationPrefix,
        String UsagePrefix);

    private record struct ComponentModel(
        String Namespace,
        ContainingType? ContainingType,
        String Name,
        String FullName,
        String? TypeParameters,
        String? TypeConstraints,
        String Accessibility);
}

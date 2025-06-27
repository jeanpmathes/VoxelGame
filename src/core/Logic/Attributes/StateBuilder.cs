// <copyright file="StateBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using VoxelGame.Core.Logic.Attributes.Implementations;
using VoxelGame.Core.Logic.Elements.New;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
/// Used to define the <see cref="StateSet"/> of a block by defining the attributes of a block.
/// </summary>
/// <param name="context">The context in which the state set is defined.</param>
public partial class StateBuilder(IResourceContext context)
{
    private const String Root = "Root";
    private const String Separator = "/";

    private readonly HashSet<String> names = [];
    private String path = Root;

    private UInt64 stateCount = 1;
    private List<IScoped> entries = [];

    private UInt64 generationDefaultState;

    private String CheckName(String name)
    {
        if (!NameRegex().IsMatch(name))
        {
            context.ReportWarning(this, $"Attribute and scope names must be alphanumeric, but '{name}' is not");
            name = "!unnamed";
        }
        else if (names.Contains(GetPath(name)))
        {
            context.ReportWarning(this, $"Attribute or scope name '{name}' is not unique in the current scope");
        }
        else
        {
            return name;
        }

        while (names.Contains(GetPath(name))) name += "'";

        return name;
    }

    private String GetPath(String entryName)
    {
        return $"{path}{Separator}{entryName}";
    }

    /// <summary>
    /// Enclose a set of attributes in a named scope.
    /// </summary>
    /// <param name="name">The name of the scope, which must be unique within the current scope and alphanumeric.</param>
    /// <param name="scoped">A builder function, all attributes defined within this function will be part of the scope.</param>
    /// <returns>The builder itself, allowing for chaining.</returns>
    public StateBuilder Enclose(String name, Action<StateBuilder> scoped)
    {
        name = CheckName(name);

        List<IScoped> outerEntries = entries;
        String outerPath = path;

        entries = [];
        path = GetPath(name);

        scoped(this);

        AddEntry(new Scope(name, entries));

        entries = outerEntries;
        path = outerPath;

        return this;
    }

    private void AddEntry(IScoped entry)
    {
        entries.Add(entry);
        names.Add($"{path}{Separator}{entry.Name}");
    }

    /// <summary>
    /// Begin defining a new attribute with the given name.
    /// </summary>
    /// <param name="name">The name of the attribute, which must be unique within the current scope and alphanumeric.</param>
    /// <returns>The next step of the builder.</returns>
    public AttributeDefinition Define(String name)
    {
        name = CheckName(name);

        return new AttributeDefinition(name, this);
    }

    private void AddAttribute<TValue>(Attribute<TValue> attribute, String name, String? description, TValue generationDefault)
    {
        if (stateCount * attribute.Multiplicity > Int32.MaxValue)
        {
            context.ReportWarning(this, $"Attribute '{name}' would cause {stateCount * attribute.Multiplicity} states which is more than allowed");

            return;
        }

        if (description == null) context.ReportWarning(this, $"Attribute '{name}' has no description");

        attribute.Initialize(name, description, stateCount);

        stateCount *= attribute.Multiplicity;

        AddEntry(attribute);

        UpdateGenerationDefaultState(attribute, generationDefault);
    }

    private void UpdateGenerationDefaultState<TValue>(IAttribute<TValue> attribute, TValue generationDefault)
    {
        Int32 index = attribute.Provide(generationDefault);
        generationDefaultState += attribute.GetStateIndex(index);
    }

    /// <summary>
    /// Builds the <see cref="StateSet"/> with the defined attributes.
    /// </summary>
    /// <param name="block">The block that this state set belongs to.</param>
    /// <param name="setOffset">The offset of the state set within the global state space.</param>
    /// <returns>The state set.</returns>
    public StateSet Build(Block block, UInt64 setOffset)
    {
        return new StateSet(block, setOffset, stateCount, generationDefaultState, entries);
    }

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex NameRegex();

    /// <summary>
    ///     Intermediate definition of an attribute.
    /// </summary>
    public sealed class AttributeDefinition(String name, StateBuilder builder)
    {
        private String? desc;

        /// <summary>
        ///     Set the description of the attribute.
        ///     Calling this method multiple times will append the descriptions.
        /// </summary>
        /// <param name="description">The description of the attribute.</param>
        public AttributeDefinition Described(String description)
        {
            if (desc == null) desc = description;
            else desc += Environment.NewLine + description;

            return this;
        }

        /// <summary>
        ///     Define the attribute as a boolean attribute.
        /// </summary>
        public AttributeDefinition<Boolean> Boolean()
        {
            return new AttributeDefinition<Boolean>(new BooleanAttribute(), name, desc, builder);
        }

        /// <summary>
        ///     Define the attribute as an integer attribute with the given minimum and maximum values.
        ///     The lower bound is inclusive, the upper bound is exclusive.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value, which must be greater than the minimum value.</param>
        /// <returns></returns>
        public AttributeDefinition<Int32> Int32(Int32 min, Int32 max)
        {
            Debug.Assert(min < max);

            return new AttributeDefinition<Int32>(new Int32Attribute(min, max), name, desc, builder);
        }

        /// <summary>
        ///     Define the attribute as an element from a list of values.
        /// </summary>
        /// <param name="elements">The valid elements for this attribute.</param>
        /// <typeparam name="TElement">The element type of the list.</typeparam>
        public AttributeDefinition<TElement> List<TElement>(IEnumerable<TElement> elements) where TElement : struct
        {
            List<TElement> list = elements.ToList();

            Debug.Assert(list.Count > 0);

            return new AttributeDefinition<TElement>(new ListAttribute<TElement>(list), name, desc, builder);
        }

        /// <summary>
        ///     Define the attribute as an enum attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        public AttributeDefinition<TEnum> Enum<TEnum>()
            where TEnum : struct, Enum
        {
            Debug.Assert(!EnumTools.IsFlagsEnum<TEnum>());

            return new AttributeDefinition<TEnum>(new EnumAttribute<TEnum>(), name, desc, builder);
        }

        /// <summary>
        ///     Define the attribute as a flags enum attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the flags enum.</typeparam>
        public AttributeDefinition<TEnum> Flags<TEnum>()
            where TEnum : struct, Enum
        {
            Debug.Assert(EnumTools.IsFlagsEnum<TEnum>());

            return new AttributeDefinition<TEnum>(new FlagsAttribute<TEnum>(), name, desc, builder);
        }
    }

    /// <summary>
    ///     Last step of the builder to define an attribute.
    /// </summary>
    public sealed class AttributeDefinition<TValue>(Attribute<TValue> attribute, String name, String? description, StateBuilder builder) where TValue : struct
    {
        /// <summary>
        ///     Complete the definition of the attribute.
        /// </summary>
        /// <param name="generationDefault">The value this attribute should have by default for generated blocks.</param>
        /// <returns>The attribute that was defined.</returns>
        public IAttribute<TValue> Attribute(TValue? generationDefault = null)
        {
            builder.AddAttribute(attribute, name, description, generationDefault ?? attribute.Retrieve(index: 0));

            return attribute;
        }

        /// <summary>
        ///     Complete the definition of the attribute as a nullable attribute.
        /// </summary>
        /// <param name="generationDefault">The value this attribute should have by default for generated blocks.</param>
        /// <returns>The nullable attribute that was defined.</returns>
        public IAttribute<TValue?> NullableAttribute(TValue? generationDefault = null)
        {
            Attribute<TValue?> nullableAttribute = new NullableAttribute<TValue>(attribute);

            builder.AddAttribute(nullableAttribute, name, description, generationDefault);

            return nullableAttribute;
        }
    }
}

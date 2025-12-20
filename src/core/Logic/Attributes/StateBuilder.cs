// <copyright file="StateBuilder.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes.Implementations;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Attributes;

/// <summary>
///     Used to define the <see cref="StateSet" /> of a block by defining the attributes of a block.
/// </summary>
/// <param name="validator">The validator to report warnings and errors to.</param>
public partial class StateBuilder(IValidator validator) : IStateBuilder
{
    private const String Root = "Root";
    private const String Separator = "/";

    private readonly HashSet<String> names = [];
    private List<IScoped> entries = [];
    private UInt64 generationDefaultState;
    private String path = Root;

    private UInt64 placementDefaultState;

    private UInt64 stateCount = 1;

    /// <inheritdoc />
    public AttributeDefinition Define(String name)
    {
        name = CheckName(name, isAttribute: true);

        return new AttributeDefinition(name, this);
    }

    private String CheckName(String name, Boolean isAttribute)
    {
        if (isAttribute && !AttributeNameRegex().IsMatch(name))
        {
            validator.ReportWarning($"Attribute names must be alphanumeric and start with an uppercase letter, '{name}' is not valid");
            name = "!unnamed";
        }
        else if (!isAttribute && !ScopeNameRegex().IsMatch(name))
        {
            validator.ReportWarning($"Scope names must be alphanumeric (with dots), '{name}' is not valid");
            name = "!unnamed";
        }
        else if (names.Contains(GetPath(name)))
        {
            validator.ReportWarning($"Attribute or scope name '{name}' is not unique in the current scope");
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
    ///     Enclose a set of attributes in a named scope.
    /// </summary>
    /// <param name="name">The name of the scope, which must be unique within the current scope and alphanumeric.</param>
    /// <param name="scoped">A builder function, all attributes defined within this function will be part of the scope.</param>
    public void Enclose(String name, Action<StateBuilder> scoped)
    {
        name = CheckName(name, isAttribute: false);

        List<IScoped> outerEntries = entries;
        String outerPath = path;

        entries = [];
        path = GetPath(name);

        scoped(this);

        List<IScoped> innerEntries = entries;

        entries = outerEntries;
        path = outerPath;

        AddEntry(new Scope(name, innerEntries));
    }

    private void AddEntry(IScoped entry)
    {
        entries.Add(entry);
        names.Add($"{path}{Separator}{entry.Name}");
    }

    private void AddAttribute<TValue>(AttributeDataImplementation<TValue> attributeData, String name, TValue placementDefault, TValue generationDefault)
    {
        if (stateCount * (UInt64) attributeData.Multiplicity > Int32.MaxValue)
        {
            validator.ReportWarning($"Attribute '{name}' would cause {stateCount * (UInt64) attributeData.Multiplicity} states which is more than allowed");

            return;
        }

        attributeData.Initialize(name, (Int32) stateCount);

        stateCount *= (UInt64) attributeData.Multiplicity;

        AddEntry(attributeData);

        UpdatePlacementDefaultState(attributeData, placementDefault);
        UpdateGenerationDefaultState(attributeData, generationDefault);
    }

    private void UpdatePlacementDefaultState<TValue>(AttributeDataImplementation<TValue> attributeData, TValue placementDefault)
    {
        Int32 index = attributeData.Provide(placementDefault);
        placementDefaultState += IAttributeData.GetStateIndex(attributeData, index);
    }

    private void UpdateGenerationDefaultState<TValue>(AttributeDataImplementation<TValue> attributeData, TValue generationDefault)
    {
        Int32 index = attributeData.Provide(generationDefault);
        generationDefaultState += IAttributeData.GetStateIndex(attributeData, index);
    }

    /// <summary>
    ///     Builds the <see cref="StateSet" /> with the defined attributes.
    /// </summary>
    /// <param name="block">The block that this state set belongs to.</param>
    /// <param name="setOffset">The offset of the state set within the global state space.</param>
    /// <returns>The state set.</returns>
    public StateSet Build(Block block, UInt32 setOffset)
    {
        Debug.Assert(stateCount <= UInt32.MaxValue);
        Debug.Assert(placementDefaultState <= Int32.MaxValue);
        Debug.Assert(generationDefaultState <= Int32.MaxValue);

        return new StateSet(block, setOffset, (UInt32) stateCount, (Int32) placementDefaultState, (Int32) generationDefaultState, entries);
    }

    [GeneratedRegex("^[A-Z][a-zA-Z0-9]*$")]
    private static partial Regex AttributeNameRegex();

    [GeneratedRegex("^[a-zA-Z0-9.]+$")]
    private static partial Regex ScopeNameRegex();

    /// <summary>
    ///     Intermediate definition of an attribute.
    /// </summary>
    public sealed class AttributeDefinition(String name, StateBuilder builder)
    {
        /// <summary>
        ///     Define the attribute as a boolean attribute.
        /// </summary>
        public AttributeDefinition<Boolean> Boolean()
        {
            return new AttributeDefinition<Boolean>(new BooleanAttributeData(), name, builder);
        }

        /// <summary>
        ///     Define the attribute as an integer attribute with the given minimum and maximum values.
        ///     The lower bound is inclusive, the upper bound is exclusive.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value, which must be greater than the minimum value.</param>
        public AttributeDefinition<Int32> Int32(Int32 min, Int32 max)
        {
            Debug.Assert(min < max);

            return new AttributeDefinition<Int32>(new Int32AttributeData(min, max), name, builder);
        }

        /// <summary>
        ///     Defines the attribute as an <see cref="Vector3i" /> attribute with the given maximum values.
        ///     The lower bound is always (0, 0, 0) and the upper bound is exclusive.
        /// </summary>
        /// <param name="max">The maximum value for each component of the vector, which must be greater than 0.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming must match the type name.")]
        public AttributeDefinition<Vector3i> Vector3i(Vector3i max)
        {
            Debug.Assert(max is {X: > 0, Y: > 0, Z: > 0});

            return new AttributeDefinition<Vector3i>(new Vector3IAttributeData(max), name, builder);
        }

        /// <summary>
        ///     Define the attribute as an element from a list of values.
        /// </summary>
        /// <param name="elements">The valid elements for this attribute.</param>
        /// <param name="representation">
        ///     An optional function to provide a custom string representation for each element, given
        ///     its index in the list.
        /// </param>
        /// <typeparam name="TElement">The element type of the list.</typeparam>
        public AttributeDefinition<TElement> List<TElement>(IEnumerable<TElement> elements, Func<Int32, String>? representation = null) where TElement : struct
        {
            List<TElement> list = elements.ToList();

            Debug.Assert(list.Count > 0);

            return new AttributeDefinition<TElement>(new ListAttributeData<TElement>(list, representation), name, builder);
        }

        /// <summary>
        ///     Define the attribute as an enum attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        public AttributeDefinition<TEnum> Enum<TEnum>()
            where TEnum : struct, Enum
        {
            Debug.Assert(!EnumTools.IsFlagsEnum<TEnum>());

            return new AttributeDefinition<TEnum>(new EnumAttributeData<TEnum>(), name, builder);
        }

        /// <summary>
        ///     Define the attribute as a flags enum attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the flags enum.</typeparam>
        public AttributeDefinition<TEnum> Flags<TEnum>()
            where TEnum : struct, Enum
        {
            Debug.Assert(EnumTools.IsFlagsEnum<TEnum>());

            return new AttributeDefinition<TEnum>(new FlagsAttributeData<TEnum>(), name, builder);
        }
    }

    /// <summary>
    ///     Last step of the builder to define an attribute.
    /// </summary>
    public sealed class AttributeDefinition<TValue>(AttributeDataImplementation<TValue> attributeData, String name, StateBuilder builder) where TValue : struct
    {
        /// <summary>
        ///     Complete the definition of the attribute.
        /// </summary>
        /// <param name="placementDefault">The value this attribute should have by default for placed blocks.</param>
        /// <param name="generationDefault">The value this attribute should have by default for generated blocks.</param>
        /// <returns>The attribute that was defined.</returns>
        [MustUseReturnValue]
        public IAttributeData<TValue> Attribute(TValue? placementDefault = null, TValue? generationDefault = null)
        {
            builder.AddAttribute(attributeData,
                name,
                placementDefault ?? attributeData.Retrieve(index: 0),
                generationDefault ?? attributeData.Retrieve(index: 0));

            return attributeData;
        }

        /// <summary>
        ///     Complete the definition of the attribute as a nullable attribute.
        /// </summary>
        /// <param name="placementDefault">The value this attribute should have by default for placed blocks.</param>
        /// <param name="generationDefault">The value this attribute should have by default for generated blocks.</param>
        /// <returns>The nullable attribute that was defined.</returns>
        [MustUseReturnValue]
        public IAttributeData<TValue?> NullableAttribute(TValue? placementDefault = null, TValue? generationDefault = null)
        {
            AttributeDataImplementation<TValue?> nullableAttributeData = new NullableAttributeData<TValue>(attributeData);

            builder.AddAttribute(nullableAttributeData, name, placementDefault, generationDefault);

            return nullableAttributeData;
        }
    }
}

/// <summary>
///     Limited interface for the <see cref="StateBuilder" /> to allow defining attributes.
/// </summary>
public interface IStateBuilder
{
    /// <summary>
    ///     Begin defining a new attribute with the given name.
    /// </summary>
    /// <param name="name">The name of the attribute, which must be unique within the current scope and alphanumeric.</param>
    /// <returns>The next step of the builder.</returns>
    StateBuilder.AttributeDefinition Define(String name);
}

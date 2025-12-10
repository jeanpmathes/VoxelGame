// <copyright file="FlagsAttributeData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal sealed class FlagsAttributeData<TFlags> : AttributeDataImplementation<TFlags> where TFlags : struct, Enum
{
    public override Int32 Multiplicity { get; } = 1 << EnumTools.CountFlags<TFlags>();

    public override TFlags Retrieve(Int32 index)
    {
        return EnumTools.GetEnumValue<TFlags>((UInt64) index);
    }

    public override Int32 Provide(TFlags value)
    {
        return (Int32) EnumTools.GetUnsignedValue(value);
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        TFlags value = Retrieve(index);

        List<Property> flags = new(EnumTools.CountFlags<TFlags>());

        foreach ((String name, TFlags flag) in EnumTools.GetPositions<TFlags>())
        {
            flags.Add(new Truth(name, value.HasFlag(flag)));
        }

        return new Group(Name, flags);
    }

    public override JsonNode GetValues(State state)
    {
        return state.Get(this).ToString();
    }

    public override State SetValues(State state, JsonNode values)
    {
        return Enum.TryParse(values.GetValue<String>(), out TFlags result) ? state.With(this, result) : state;
    }
}

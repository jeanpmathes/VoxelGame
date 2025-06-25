// <copyright file="NullableAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class NullableAttribute<TValue>(IAttribute<TValue> valueAttribute) : Attribute<TValue?> where TValue : notnull
{
    public override UInt64 Multiplicity { get; } = valueAttribute.Multiplicity + 1;

    public override TValue? Retrieve(Int32 index)
    {
        return index == 0 ? default : valueAttribute.Retrieve(index - 1);
    }

    public override Int32 Provide(TValue? value)
    {
        return value is null ? 0 : valueAttribute.Provide(value) + 1;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return index == 0
            ? new Message(Name, "null")
            : new Group(Name, [valueAttribute.RetrieveRepresentation(index - 1)]);
    }
}

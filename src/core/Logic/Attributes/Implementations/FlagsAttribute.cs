// <copyright file="FlagsAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class FlagsAttribute<TFlags> : Attribute<TFlags> where TFlags : struct, Enum
{
    public override UInt64 Multiplicity { get; } = 1u << EnumTools.CountFlags<TFlags>();

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
        return new Message(Name, $"{Retrieve(index)}"); // todo: think about a flags property
    }
}

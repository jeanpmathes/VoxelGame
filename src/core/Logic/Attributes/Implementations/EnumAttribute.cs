// <copyright file="EnumAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class EnumAttribute<TEnum>() : ListAttribute<TEnum>(Enum.GetValues<TEnum>()) where TEnum : struct, Enum
{
    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Message(Name, $"{Retrieve(index)}");
    }
}

// <copyright file="Int32Attribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class Int32Attribute(Int32 min, Int32 max) : Attribute<Int32>
{
    public override UInt64 Multiplicity { get; } = (UInt64) (max - min);

    public override Int32 Retrieve(Int32 index)
    {
        return min + index;
    }

    public override Int32 Provide(Int32 value)
    {
        return value - min;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Integer(Name, Retrieve(index));
    }
}

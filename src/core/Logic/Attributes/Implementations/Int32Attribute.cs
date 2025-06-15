// <copyright file="Int32Attribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class Int32Attribute(String name, Int32 min, Int32 max) : IAttribute<Int32>
{
    public String Name { get; } = name;
    
    public Int32 Index { get; init; }

    public UInt64 Multiplicity { get; } = (UInt64) (max - min);

    public Int32 Retrieve(Int32 index)
    {
        return min + index;
    }
}
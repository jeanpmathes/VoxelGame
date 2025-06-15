// <copyright file="BooleanAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class BooleanAttribute(String name) : IAttribute<Boolean>
{
    public String Name { get; } = name;
    
    public Int32 Index { get; init; }
    
    public UInt64 Multiplicity => 2;

    public Boolean Retrieve(Int32 index)
    {
        return index == 1;
    }
}
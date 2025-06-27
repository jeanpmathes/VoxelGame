// <copyright file="ListAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class ListAttribute<TElement>(IEnumerable<TElement> elements) : Attribute<TElement> where TElement : struct
{
    private readonly TElement[] elements = [..elements];

    public override UInt64 Multiplicity => (UInt64) elements.Length;

    public override TElement Retrieve(Int32 index)
    {
        return elements[index];
    }

    public override Int32 Provide(TElement value)
    {
        for (var index = 0; index < elements.Length; index++)
            if (EqualityComparer<TElement>.Default.Equals(elements[index], value))
                return index;

        return 0;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Message(Name, $"[{index}] = {elements[index]}");
    }
}

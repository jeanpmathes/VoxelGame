﻿// <copyright file="BooleanAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

internal class BooleanAttribute : Attribute<Boolean>
{
    public override UInt64 Multiplicity => 2;

    public override Boolean Retrieve(Int32 index)
    {
        return index == 1;
    }

    public override Int32 Provide(Boolean value)
    {
        return value ? 1 : 0;
    }

    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Message(Name, index == 0 ? "false" : "true"); // todo: boolean property
    }
}

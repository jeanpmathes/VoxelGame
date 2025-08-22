// <copyright file="Vector3iAttribute.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Logic.Attributes.Implementations;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming must match the type name.")]
internal class Vector3iAttribute(Vector3i max) : AttributeImplementation<Vector3i>
{
    private readonly Int32 maxXY = max.X * max.Y;
    
    public override Int32 Multiplicity { get; } = max.X * max.Y * max.Z;
    
    public override Vector3i Retrieve(Int32 index)
    {
        Int32 z = index / maxXY;
        Int32 y = index / max.X % max.Y;
        Int32 x = index % max.X;

        return new Vector3i(x, y, z);
    }
    public override Int32 Provide(Vector3i value)
    {
        Debug.Assert(value.X >= 0 && value.X < max.X);
        Debug.Assert(value.Y >= 0 && value.Y < max.Y);
        Debug.Assert(value.Z >= 0 && value.Z < max.Z);

        return value.Z * maxXY + value.Y * max.X + value.X;
    }
    public override Property RetrieveRepresentation(Int32 index)
    {
        return new Group(Name, [
            new Integer("x", Retrieve(index).X),
            new Integer("y", Retrieve(index).Y),
            new Integer("z", Retrieve(index).Z)
        ]);
    }
}

﻿// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Structures;

/// <summary>
///     A dynamically created tree structure.
/// </summary>
public class Tree : DynamicStructure
{
    /// <summary>
    ///     The kind of tree.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        ///     A 'normal' tree, growing in temperate climates.
        /// </summary>
        Normal,

        /// <summary>
        ///     A tropical tree, growing in warm climates.
        /// </summary>
        Tropical
    }

    private readonly Kind kind;

    private readonly Vector3i normalExtents = new(x: 5, y: 9, z: 5);
    private readonly Vector3i tropicalExtents = new(x: 9, y: 16, z: 9);

    /// <summary>
    ///     Creates a new tree structure.
    /// </summary>
    /// <param name="kind">The kind of tree.</param>
    public Tree(Kind kind)
    {
        this.kind = kind;
    }

    /// <inheritdoc />
    public override Vector3i Extents => kind switch
    {
        Kind.Normal => normalExtents,
        Kind.Tropical => tropicalExtents,
        _ => throw new InvalidOperationException()
    };

    /// <inheritdoc />
    protected override Content? GetContent(Vector3i offset)
    {
        return kind switch
        {
            Kind.Normal => GetNormalContent(offset),
            Kind.Tropical => GetTropicalContent(offset),
            _ => throw new InvalidOperationException()
        };
    }

    private Content? GetNormalContent(Vector3i offset)
    {
        const int center = 2;

        if (offset is {X: center, Z: center} and {Y: 0})
            return new Content(Block.Roots);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 7})
            return new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default);

        // The crown of this tree is a sphere.

        Vector3i crownCenter = new(center, y: 6, center);

        const float radiusSquared = 2.5f * 2.5f;
        float distanceSquared = Vector3.DistanceSquared(offset, crownCenter);

        if (distanceSquared > radiusSquared) return null;

        float closeness = 1 - distanceSquared / radiusSquared;

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return new Content(Block.Leaves);
    }

    private Content? GetTropicalContent(Vector3i offset)
    {
        const int center = 4;

        if (offset is {X: center, Z: center} and {Y: 0})
            return new Content(Block.Roots);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 14})
            return new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default);

        // The crown of this tree is an spheroid.

        Vector3i crownCenter = new(center, y: 14, center);

        const float crownHeight = 1.5f;
        const float crownRadius = 4.5f;

        Vector3 point = offset - crownCenter;

        float a = point.X * point.X / (crownRadius * crownRadius);
        float b = point.Y * point.Y / (crownHeight * crownHeight);
        float c = point.Z * point.Z / (crownRadius * crownRadius);

        float closeness = 1 - (a + b + c);

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return new Content(Block.Leaves);
    }
}

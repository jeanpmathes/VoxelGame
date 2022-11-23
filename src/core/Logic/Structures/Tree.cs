// <copyright file="Tree.cs" company="VoxelGame">
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
        ///     A second 'normal' tree, growing in temperate climates.
        /// </summary>
        Normal2,

        /// <summary>
        ///     A tropical tree, growing in warm climates.
        /// </summary>
        Tropical,

        /// <summary>
        ///     A tree with needles, growing in cold climates.
        /// </summary>
        Needle
    }

    private readonly Kind kind;
    private readonly Vector3i needleExtents = new(x: 5, y: 11, z: 5);
    private readonly Vector3i normal2Extents = new(x: 5, y: 9, z: 5);

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
        Kind.Normal2 => normal2Extents,
        Kind.Tropical => tropicalExtents,
        Kind.Needle => needleExtents,
        _ => throw new InvalidOperationException()
    };

    /// <inheritdoc />
    protected override (Content, bool)? GetContent(Vector3i offset)
    {
        return kind switch
        {
            Kind.Normal => GetNormalContent(offset),
            Kind.Normal2 => GetNormal2Content(offset),
            Kind.Tropical => GetTropicalContent(offset),
            Kind.Needle => GetNeedleContent(offset),
            _ => throw new InvalidOperationException()
        };
    }

    private (Content, bool overwrite)? GetNormalContent(Vector3i offset)
    {
        const int center = 2;

        if (offset is {X: center, Z: center} and {Y: 0})
            return (new Content(Block.Roots), overwrite: true);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 7})
            return (new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default), overwrite: true);

        // The crown of this tree is a sphere.

        Vector3i crownCenter = new(center, y: 6, center);

        const float radiusSquared = 2.5f * 2.5f;
        float distanceSquared = Vector3.DistanceSquared(offset, crownCenter);

        if (distanceSquared > radiusSquared) return null;

        float closeness = 1 - distanceSquared / radiusSquared;

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return (new Content(Block.Leaves), overwrite: false);
    }

    private (Content, bool overwrite)? GetNormal2Content(Vector3i offset)
    {
        const int center = 2;

        if (offset is {X: center, Z: center} and {Y: 0})
            return (new Content(Block.Roots), overwrite: true);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 7})
            return (new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default), overwrite: true);

        // The crown of this tree is a spheroid.

        Vector3 crownCenter = new(center, y: 5.5f, center);

        const float crownHeight = 4.0f;
        const float crownRadius = 2.5f;

        Vector3 point = offset - crownCenter;

        float a = point.X * point.X / (crownRadius * crownRadius);
        float b = point.Y * point.Y / (crownHeight * crownHeight);
        float c = point.Z * point.Z / (crownRadius * crownRadius);

        float closeness = 1 - (a + b + c);

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return (new Content(Block.Leaves), overwrite: false);
    }

    private (Content, bool overwrite)? GetTropicalContent(Vector3i offset)
    {
        const int center = 4;

        if (offset is {X: center, Z: center} and {Y: 0})
            return (new Content(Block.Roots), overwrite: true);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 14})
            return (new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default), overwrite: true);

        // The crown of this tree is a spheroid.

        Vector3i crownCenter = new(center, y: 14, center);

        const float crownHeight = 1.5f;
        const float crownRadius = 4.5f;

        Vector3 point = offset - crownCenter;

        float a = point.X * point.X / (crownRadius * crownRadius);
        float b = point.Y * point.Y / (crownHeight * crownHeight);
        float c = point.Z * point.Z / (crownRadius * crownRadius);

        float closeness = 1 - (a + b + c);

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return (new Content(Block.Leaves), overwrite: false);
    }

    private (Content, bool overwrite)? GetNeedleContent(Vector3i offset)
    {
        const int center = 2;

        if (offset is {X: center, Z: center} and {Y: 0})
            return (new Content(Block.Roots), overwrite: true);

        if (offset is {X: center, Z: center} and {Y: > 0 and < 8})
            return (new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default), overwrite: true);

        // The crown of this tree is a cone.

        Vector3 crownStart = new(center, y: 3, center);

        const float crownHeight = 9.0f;
        const float baseRadius = 2.5f;

        Vector3 point = offset - crownStart;

        float height = point.Y / crownHeight;
        float radius = baseRadius * (1 - height);

        if (height is < 0 or > 1) return null;

        float radiusSquared = radius * radius;
        float distanceSquared = point.X * point.X + point.Z * point.Z;

        float closeness = 1 - distanceSquared / radiusSquared;

        if (closeness < 0.35f * Random.NextSingle()) return null;

        return (new Content(Block.Leaves), overwrite: false);
    }
}

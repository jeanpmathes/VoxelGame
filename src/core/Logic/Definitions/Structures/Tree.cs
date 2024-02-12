// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Structures;

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
        Needle,

        /// <summary>
        ///     A palm tree, growing on beaches.
        /// </summary>
        Palm,

        /// <summary>
        ///     A savanna tree, growing in savannas.
        /// </summary>
        Savanna,

        /// <summary>
        ///     A small shrub, growing in a dry climate.
        /// </summary>
        Shrub
    }

    private readonly Kind kind;

    private readonly (int height, float factor) needleConfig = (height: 8, factor: 0.35f);

    private readonly Shape3D needleCrown = new Cone
    {
        Position = new Vector3(x: 2, y: 3, z: 2),
        BottomRadius = 2.5f,
        TopRadius = 0.0f,
        Height = 9.0f
    };

    private readonly Vector3i needleExtents = new(x: 5, y: 11, z: 5);

    private readonly (int height, float factor) normal2Config = (height: 7, factor: 0.25f);

    private readonly Shape3D normal2Crown = new Spheroid
    {
        Position = new Vector3(x: 2, y: 5.5f, z: 2),
        Radius = new Vector3(x: 2.5f, y: 4.0f, z: 2.5f)
    };

    private readonly Vector3i normal2Extents = new(x: 5, y: 9, z: 5);

    private readonly (int height, float factor) normalConfig = (height: 7, factor: 0.25f);

    private readonly Shape3D normalCrown = new Sphere
    {
        Position = new Vector3(x: 2, y: 6, z: 2),
        Radius = 2.5f
    };

    private readonly Vector3i normalExtents = new(x: 5, y: 9, z: 5);

    private readonly (int height, float factor) palmConfig = (height: 9, factor: 0.25f);

    private readonly Shape3D palmCrown = new Sphere
    {
        Position = new Vector3(x: 2, y: 9, z: 2),
        Radius = 1.5f
    };

    private readonly Vector3i palmExtents = new(x: 5, y: 11, z: 5);

    private readonly (int height, float factor) savannaConfig = (height: 7, factor: 0.25f);

    private readonly Shape3D savannaCrown = new Cone
    {
        Position = new Vector3(x: 2, y: 7, z: 2),
        BottomRadius = 2.5f,
        TopRadius = 2.5f,
        Height = 1.0f
    };

    private readonly Vector3i savannaExtents = new(x: 5, y: 8, z: 5);

    private readonly (int height, float factor) shrubConfig = (height: 3, factor: 0.25f);

    private readonly Shape3D shrubCrown = new Sphere
    {
        Position = new Vector3(x: 2, y: 2, z: 2),
        Radius = 1.5f
    };

    private readonly Vector3i shrubExtents = new(x: 5, y: 4, z: 5);

    private readonly (int height, float factor) tropicalConfig = (height: 14, factor: 0.25f);

    private readonly Shape3D tropicalCrown = new Spheroid
    {
        Position = new Vector3(x: 4, y: 14, z: 4),
        Radius = new Vector3(x: 4.5f, y: 1.5f, z: 4.5f)
    };

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
        Kind.Palm => palmExtents,
        Kind.Savanna => savannaExtents,
        Kind.Shrub => shrubExtents,
        _ => throw new InvalidOperationException()
    };

    private Shape3D GetCrownShape()
    {
        return kind switch
        {
            Kind.Normal => normalCrown,
            Kind.Normal2 => normal2Crown,
            Kind.Tropical => tropicalCrown,
            Kind.Needle => needleCrown,
            Kind.Palm => palmCrown,
            Kind.Savanna => savannaCrown,
            Kind.Shrub => shrubCrown,
            _ => throw new InvalidOperationException()
        };
    }

    private (int height, float factor) GetConfig()
    {
        return kind switch
        {
            Kind.Normal => normalConfig,
            Kind.Normal2 => normal2Config,
            Kind.Tropical => tropicalConfig,
            Kind.Needle => needleConfig,
            Kind.Palm => palmConfig,
            Kind.Savanna => savannaConfig,
            Kind.Shrub => shrubConfig,
            _ => throw new InvalidOperationException()
        };
    }

    /// <inheritdoc />
    protected override (Content content, bool overwrite)? GetContent(Vector3i offset)
    {
        int center = Extents.X / 2;
        Debug.Assert(Extents.X == Extents.Z);

        (int height, float factor) = GetConfig();

        if (offset.X == center && offset.Y == 0 && offset.Z == center)
            return (new Content(Logic.Blocks.Instance.Roots), overwrite: true);

        if (offset.X == center && offset.Z == center && offset.Y < height)
            return (new Content(Logic.Blocks.Instance.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default), overwrite: true);

        if (!GetCrownShape().Contains(offset, out float closeness)) return null;
        if (closeness < factor * Random.NextSingle()) return null;

        return (new Content(Logic.Blocks.Instance.Leaves), overwrite: false);
    }
}

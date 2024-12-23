﻿// <copyright file="Structures.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>


using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     All structures that are used during structure placement generation.
///     Structure placement is used for large per-section structures.
///     For smaller structures, the decoration system is used.
/// </summary>
public sealed class Structures
{
    private readonly Dictionary<String, GeneratedStructure> structuresByName = new();

    private Structures() {}

    /// <summary>
    ///     A small pyramid.
    /// </summary>
    public GeneratedStructure SmallPyramid { get; } = new(nameof(SmallPyramid), GeneratedStructure.Kind.Surface, StaticStructure.Load("small_pyramid"), rarity: 5.0f, (0, -6, 0));

    /// <summary>
    ///     A large tropical tree.
    /// </summary>
    public GeneratedStructure LargeTropicalTree { get; } = new(nameof(LargeTropicalTree), GeneratedStructure.Kind.Surface, StaticStructure.Load("large_tropical_tree"), rarity: 0.0f, (0, -5, 0));

    /// <summary>
    ///     An old tower.
    /// </summary>
    public GeneratedStructure OldTower { get; } = new(nameof(OldTower), GeneratedStructure.Kind.Surface, StaticStructure.Load("old_tower"), rarity: 10.0f, (0, -2, 0));

    /// <summary>
    ///     A variant of the old tower that is buried in the ground.
    /// </summary>
    public GeneratedStructure BuriedTower { get; } = new(nameof(BuriedTower), GeneratedStructure.Kind.Underground, StaticStructure.Load("buried_tower"), rarity: 10.0f, (0, -2, 0));

    /// <summary>
    ///     Get the structures instance. May only be called after the initialization method has been called.
    /// </summary>
    public static Structures Instance { get; private set; } = null!;

    /// <summary>
    ///     Get all structures.
    /// </summary>
    public IEnumerable<GeneratedStructure> All { get; private set; } = new List<GeneratedStructure>();

    /// <summary>
    ///     Initialize and load all structures.
    /// </summary>
    public static void Initialize(ILoadingContext loadingContext)
    {
        using (loadingContext.BeginStep("Structures"))
        {
            Instance = new Structures();

            List<GeneratedStructure> structures =
            [
                Instance.SmallPyramid,
                Instance.LargeTropicalTree,
                Instance.OldTower,
                Instance.BuriedTower
            ];

            Instance.All = structures;

            foreach (GeneratedStructure structure in structures)
            {
                // ReSharper disable once RedundantAssignment
                Boolean success = Instance.structuresByName.TryAdd(structure.Name, structure);

                Debug.Assert(success);
            }
        }
    }

    /// <summary>
    ///     Set up the structures.
    ///     Do not forget to call tear down when done.
    /// </summary>
    /// <param name="factory">The factory to use for noise generation.</param>
    public void SetUpNoise(NoiseFactory factory)
    {
        foreach (GeneratedStructure structure in All) structure.SetUpNoise(factory);
    }

    /// <summary>
    ///     Tear down the structures.
    ///     Requires that set up has been called before.
    /// </summary>
    public void TearDownNoise()
    {
        foreach (GeneratedStructure structure in All) structure.TearDownNoise();
    }

    /// <summary>
    ///     Search for a given structure.
    /// </summary>
    public IEnumerable<Vector3i>? Search(Vector3i start, String name, UInt32 maxDistance, Generator generator)
    {
        GeneratedStructure? structure = structuresByName.GetValueOrDefault(name);

        return structure?.Search(start, maxDistance, generator);
    }
}

// <copyright file="Structures.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Structures;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     All structures that are used during structure placement generation.
///     Structure placement is used for large per-section structures.
///     For smaller structures, the decoration system is used.
/// </summary>
public class Structures
{
    private readonly Dictionary<string, GeneratedStructure> structuresByName = new();

    private Structures() {}

    /// <summary>
    ///     A small pyramid.
    /// </summary>
    public GeneratedStructure SmallPyramid { get; } = new(nameof(SmallPyramid), StaticStructure.Load("small_pyramid"), rarity: 5.0f, (0, -6, 0));

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
    public static void Initialize()
    {
        Instance = new Structures();

        List<GeneratedStructure> structures = new()
        {
            Instance.SmallPyramid
        };

        Instance.All = structures;

        foreach (GeneratedStructure structure in structures)
        {
            bool success = Instance.structuresByName.TryAdd(structure.Name, structure);
            Debug.Assert(success);
        }
    }

    /// <summary>
    ///     Setup the structures.
    /// </summary>
    /// <param name="factory">The factory to use for noise generation.</param>
    public void Setup(NoiseFactory factory)
    {
        foreach (GeneratedStructure structure in All) structure.Setup(factory);
    }

    /// <summary>
    ///     Search for a given structure.
    /// </summary>
    public IEnumerable<Vector3i>? Search(Vector3i start, string name, uint maxDistance, Generator generator)
    {
        GeneratedStructure? structure = structuresByName.GetValueOrDefault(name);

        return structure?.Search(start, maxDistance, generator);
    }
}

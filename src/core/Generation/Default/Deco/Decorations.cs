// <copyright file="Decorations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Structures;

namespace VoxelGame.Core.Generation.Default.Deco;

/// <summary>
///     Contains all decorations.
/// </summary>
public class Decorations
{
    private Decorations() {}

    /// <summary>
    ///     Get the decorations instance. May only be called after the initialization method has been called.
    /// </summary>
    public static Decorations Instance { get; private set; } = null!;

    /// <summary>
    ///     Tall grass.
    /// </summary>
    public Decoration TallGrass { get; } = new StructureDecoration(nameof(TallGrass), rarity: 1.0f, StaticStructure.Load("tall-grass"), new PlantableDecorator());

    /// <summary>
    ///     Tall flowers.
    /// </summary>
    public Decoration TallFlower { get; } = new StructureDecoration(nameof(TallFlower), rarity: 4.0f, StaticStructure.Load("tall-flower"), new PlantableDecorator());

    /// <summary>
    ///     Basic trees.
    /// </summary>
    public Decoration NormalTree { get; } = new StructureDecoration(nameof(NormalTree), rarity: 3.0f, new Tree(Tree.Kind.Normal), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Tropical trees.
    /// </summary>
    public Decoration TropicalTree { get; } = new StructureDecoration(nameof(TropicalTree), rarity: 3.0f, new Tree(Tree.Kind.Tropical), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Initialize the decorations.
    /// </summary>
    public static void Initialize()
    {
        Instance = new Decorations();
    }
}

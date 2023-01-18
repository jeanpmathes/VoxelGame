// <copyright file="Decorations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Structures;

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
    public Decoration TallGrass { get; } = new StructureDecoration(nameof(TallGrass), rarity: 1.0f, StaticStructure.Load("tall_grass"), new PlantableDecorator());

    /// <summary>
    ///     Tall flowers.
    /// </summary>
    public Decoration TallFlower { get; } = new StructureDecoration(nameof(TallFlower), rarity: 4.0f, StaticStructure.Load("tall_flower"), new PlantableDecorator());

    /// <summary>
    ///     Basic trees.
    /// </summary>
    public Decoration NormalTree { get; } = new StructureDecoration(nameof(NormalTree), rarity: 3.0f, new Tree(Tree.Kind.Normal), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Another basic tree.
    /// </summary>
    public Decoration NormalTree2 { get; } = new StructureDecoration(nameof(NormalTree2), rarity: 3.0f, new Tree(Tree.Kind.Normal2), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Tropical tree.
    /// </summary>
    public Decoration TropicalTree { get; } = new StructureDecoration(nameof(TropicalTree), rarity: 3.0f, new Tree(Tree.Kind.Tropical), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Needle tree.
    /// </summary>
    public Decoration NeedleTree { get; } = new StructureDecoration(nameof(NeedleTree), rarity: 3.0f, new Tree(Tree.Kind.Needle), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Palm tree.
    /// </summary>
    public Decoration PalmTree { get; } = new StructureDecoration(nameof(PalmTree), rarity: 25.0f, new Tree(Tree.Kind.Palm), new CoverDecorator(Blocks.Instance.Sand, Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Savanna tree.
    /// </summary>
    public Decoration SavannaTree { get; } = new StructureDecoration(nameof(SavannaTree), rarity: 30.0f, new Tree(Tree.Kind.Savanna), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     A cactus.
    /// </summary>
    public Decoration Cactus { get; } = new StructureDecoration(nameof(Cactus), rarity: 50.0f, new Cactus(), new CoverDecorator(Blocks.Instance.Sand, Vector3i.Zero, width: 3));

    /// <summary>
    ///     A boulder.
    /// </summary>
    public Decoration Boulder { get; } = new BoulderDecoration(nameof(Boulder), rarity: 2000.0f, new SurfaceDecorator(width: 5));

    /// <summary>
    ///     A shrub.
    /// </summary>
    public Decoration Shrub { get; } = new StructureDecoration(nameof(Shrub), rarity: 100.0f, new Tree(Tree.Kind.Shrub), new PlantableDecorator(Vector3i.UnitY, width: 3));

    /// <summary>
    ///     Buried roots.
    /// </summary>
    public Decoration Roots { get; } = new RootDecoration(nameof(Roots), rarity: 1000.0f, new DepthDecorator(minDepth: 5, maxDepth: 15));

    /// <summary>
    ///     Vines.
    /// </summary>
    public Decoration Vines { get; } = new FlatBlockDecoration(nameof(Vines), rarity: 1.0f, Blocks.Instance.Specials.Vines, new HashSet<Block> {Blocks.Instance.Log, Blocks.Instance.Leaves});

    /// <summary>
    ///     Initialize the decorations.
    /// </summary>
    public static void Initialize()
    {
        Instance = new Decorations();
    }
}


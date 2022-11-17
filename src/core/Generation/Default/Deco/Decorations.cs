// <copyright file="Decorations.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    public Decoration TallGrass { get; } = new StructureDecoration(nameof(TallGrass), rarity: 1f, StaticStructure.Load("tall-grass"), new PlantableDecorator());

    /// <summary>
    ///     Tall flowers.
    /// </summary>
    public Decoration TallFlower { get; } = new StructureDecoration(nameof(TallFlower), rarity: 4f, StaticStructure.Load("tall-flower"), new PlantableDecorator());

    /// <summary>
    ///     Basic trees.
    /// </summary>
    public Decoration Tree { get; } = new StructureDecoration(nameof(Tree), rarity: 3f, new Tree(), new PlantableDecorator(width: 3));

    /// <summary>
    ///     Initialize the decorations.
    /// </summary>
    public static void Initialize()
    {
        Instance = new Decorations();
    }
}

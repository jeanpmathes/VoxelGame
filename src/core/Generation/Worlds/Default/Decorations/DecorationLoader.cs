// <copyright file="DecorationLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
/// Loads all decorations.
/// </summary>
public sealed class DecorationLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IEnumerable<IResource> Load(IResourceContext context) =>
        context.Require<IStructureProvider>(structures =>
        [
            new StructureDecoration("TallGrass", rarity: 1.0f, structures.GetStructure(RID.File<StaticStructure>("tall_grass")), new PlantableDecorator()),
            new StructureDecoration("TallFlower", rarity: 4.0f, structures.GetStructure(RID.File<StaticStructure>("tall_flower")), new PlantableDecorator()),
            new StructureDecoration("NormalTree", rarity: 3.0f, new Tree(Tree.Kind.Normal), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new StructureDecoration("NormalTree2", rarity: 3.0f, new Tree(Tree.Kind.Normal2), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new StructureDecoration("TropicalTree", rarity: 3.0f, new Tree(Tree.Kind.Tropical), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new StructureDecoration("NeedleTree", rarity: 3.0f, new Tree(Tree.Kind.Needle), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new StructureDecoration("PalmTree", rarity: 25.0f, new Tree(Tree.Kind.Palm), new CoverDecorator(Blocks.Instance.Sand, Vector3i.UnitY, width: 3)),
            new StructureDecoration("SavannaTree", rarity: 30.0f, new Tree(Tree.Kind.Savanna), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new StructureDecoration("Cactus", rarity: 50.0f, new Cactus(), new CoverDecorator(Blocks.Instance.Sand, Vector3i.Zero, width: 3)),
            new BoulderDecoration("Boulder", rarity: 2000.0f, new SurfaceDecorator(width: 5)),
            new StructureDecoration("Shrub", rarity: 100.0f, new Tree(Tree.Kind.Shrub), new PlantableDecorator(Vector3i.UnitY, width: 3)),
            new RootDecoration("Roots", rarity: 1000.0f, new DepthDecorator(minDepth: 5, maxDepth: 15)),
            new FlatBlockDecoration("Vines", rarity: 1.0f, Blocks.Instance.Specials.Vines, new HashSet<Block> {Blocks.Instance.Log, Blocks.Instance.Leaves})
        ]);
}

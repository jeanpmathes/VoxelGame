// <copyright file="DecorationLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default.Decorations;

/// <summary>
///     Loads all decorations.
/// </summary>
public sealed class DecorationLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<IStructureProvider>(structures =>
        [
            .. CreatePlants(structures),
            .. CreateTrees(context.GetAll<Wood>()),
            new BoulderDecoration("Boulder", new SurfaceDecorator(width: 5)),
            new TermiteMoundDecoration("TermiteMound", new SurfaceDecorator(width: 5))
        ]);
    }

    private static IEnumerable<Decoration> CreatePlants(IStructureProvider structures)
    {
        return
        [
            new StructureDecoration("TallGrass", structures.GetStructure(RID.File<StaticStructure>("tall_grass")), new PlantableDecorator()),
            new StructureDecoration("TallRedFlower", structures.GetStructure(RID.File<StaticStructure>("tall_flower_red")), new PlantableDecorator()),
            new StructureDecoration("TallYellowFlower", structures.GetStructure(RID.File<StaticStructure>("tall_flower_yellow")), new PlantableDecorator()),
            new StructureDecoration("Cactus", new Cactus(), new CoverDecorator(Blocks.Instance.Environment.Sand, Vector3i.Zero, width: 3)),
            new RootDecoration("Roots", new DepthDecorator(minDepth: 5, maxDepth: 15)),
            new AttachedBlockDecoration("Vines",
                Blocks.Instance.Organic.Vines,
                new HashSet<Block>
                {
                    Blocks.Instance.Woods.Mahogany.Log, Blocks.Instance.Woods.Mahogany.Leaves, Blocks.Instance.Woods.Teak.Log, Blocks.Instance.Woods.Teak.Leaves
                })
        ];
    }

    private static IEnumerable<Decoration> CreateTrees(IEnumerable<Wood> woods)
    {
        foreach (Wood wood in woods)
        {
            Wood.Tree treeDefinition = wood.Trees;

            var name = $"{wood.ID}";

            Int32 height = GetTrunkHeight(treeDefinition.Height);
            Shape3D crownShape = GetCrownShape(treeDefinition.Shape, height);
            Double crownRandomization = GetCrownRandomization(treeDefinition.Density);

            Tree treeStructure = new(height, crownRandomization, crownShape, wood.Log, wood.Leaves);

            Decorator decorator = treeDefinition.Terrain switch
            {
                Wood.Tree.TerrainType.Earth => new PlantableDecorator(Vector3i.UnitY, width: 3),
                Wood.Tree.TerrainType.Sand => new CoverDecorator(Blocks.Instance.Environment.Sand, Vector3i.UnitY, width: 3),
                _ => throw Exceptions.UnsupportedEnumValue(treeDefinition.Terrain)
            };

            yield return new StructureDecoration(name, treeStructure, decorator);
        }
    }

    private static Int32 GetTrunkHeight(Wood.Tree.Growth growth)
    {
        return growth switch
        {
            Wood.Tree.Growth.Shrub => 4,
            Wood.Tree.Growth.Short => 8,
            Wood.Tree.Growth.Medium => 11,
            Wood.Tree.Growth.Tall => 14,
            _ => throw Exceptions.UnsupportedEnumValue(growth)
        };
    }

    private static Shape3D GetCrownShape(Wood.Tree.CrownShape shape, Int32 heightWithRoots)
    {
        Int32 totalHeight = heightWithRoots + 1;

        return shape switch
        {
            Wood.Tree.CrownShape.Sphere => GetSphereCrown(totalHeight),
            Wood.Tree.CrownShape.LongSpheroid => GetLongSpheroidCrown(totalHeight),
            Wood.Tree.CrownShape.FlatSpheroid => GetFlatSpheroidCrown(totalHeight),
            Wood.Tree.CrownShape.Cone => GetConeCrown(totalHeight),
            Wood.Tree.CrownShape.Palm => GetPalmCrown(totalHeight),
            _ => throw Exceptions.UnsupportedEnumValue(shape)
        };
    }

    private static Shape3D GetSphereCrown(Int32 totalHeight)
    {
        const Double heightRatio = 2.0 / 3.0;

        Double diameter = totalHeight * heightRatio;
        Double radius = diameter / 2;

        Double start = totalHeight - diameter;
        Double offset = start + radius;

        return new Sphere
        {
            Position = new Vector3d(x: 0, Math.Floor(offset), z: 0),
            Radius = radius
        };
    }

    private static Shape3D GetLongSpheroidCrown(Int32 totalHeight)
    {
        const Double heightRatio = 3.0 / 4.0;
        const Double inverseLongFactor = 3.0 / 5.0;

        Double diameter = totalHeight * heightRatio;
        Double radius = diameter / 2.0;

        Double start = totalHeight - diameter;
        Double offset = start + radius;

        return new Spheroid
        {
            Position = new Vector3d(x: 0, Math.Floor(offset), z: 0),
            Radius = new Vector3d(radius * inverseLongFactor, radius, radius * inverseLongFactor)
        };
    }

    private static Shape3D GetFlatSpheroidCrown(Int32 totalHeight)
    {
        const Double heightRatio = 3.0 / 4.0;
        const Double flatFactor = 1.0 / 5.0;

        Double diameter = totalHeight * heightRatio;
        Double radius = diameter / 2.0;

        Double start = totalHeight - diameter * flatFactor;
        Double offset = start + radius * flatFactor;

        return new Spheroid
        {
            Position = new Vector3d(x: 0, Math.Floor(offset), z: 0),
            Radius = new Vector3d(radius, radius * flatFactor, radius)
        };
    }

    private static Shape3D GetConeCrown(Int32 totalHeight)
    {
        const Double heightRatio = 3.0 / 4.0;

        const Double bottomRadiusRatio = 3.0 / 10.0;
        const Double topRadiusRatio = 1.0 / 10.0;

        Double height = totalHeight * heightRatio;
        Double bottomRadius = totalHeight * bottomRadiusRatio;
        Double topRadius = totalHeight * topRadiusRatio;

        Double offset = totalHeight - height;

        return new Cone
        {
            Position = new Vector3d(x: 0, Math.Floor(offset), z: 0),
            BottomRadius = bottomRadius,
            TopRadius = topRadius,
            Height = height
        };
    }

    private static Shape3D GetPalmCrown(Int32 totalHeight)
    {
        const Double heightRatio = 2.0 / 5.0;

        Double diameter = totalHeight * heightRatio;
        Double radius = diameter / 2.0;

        Double start = totalHeight - diameter;
        Double offset = start + radius;

        return new Sphere
        {
            Position = new Vector3d(x: 0, Math.Floor(offset), z: 0),
            Radius = radius
        };
    }

    private static Double GetCrownRandomization(Wood.Tree.CrownDensity density)
    {
        return density switch
        {
            Wood.Tree.CrownDensity.Dense => 0.3,
            Wood.Tree.CrownDensity.Normal => 0.5,
            Wood.Tree.CrownDensity.Sparse => 0.7,
            _ => throw Exceptions.UnsupportedEnumValue(density)
        };
    }
}

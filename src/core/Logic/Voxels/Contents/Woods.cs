// <copyright file="Woods.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     All sorts of wood types. Trees can be found in the world and can be used for the construction of various things.
/// </summary>
public class Woods(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Oak wood.
    /// </summary>
    public Wood Oak { get; } = builder.BuildWood(new CID(nameof(Oak)),
        (Language.OakLeaves, Language.OakLog, Language.OakWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Maple wood.
    /// </summary>
    public Wood Maple { get; } = builder.BuildWood(new CID(nameof(Maple)),
        (Language.MapleLeaves, Language.MapleLog, Language.MapleWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Birch wood.
    /// </summary>
    public Wood Birch { get; } = builder.BuildWood(new CID(nameof(Birch)),
        (Language.BirchLeaves, Language.BirchLog, Language.BirchWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Maple wood.
    /// </summary>
    public Wood Walnut { get; } = builder.BuildWood(new CID(nameof(Walnut)),
        (Language.WalnutLeaves, Language.WalnutLog, Language.WalnutWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Cherry wood.
    /// </summary>
    public Wood Cherry { get; } = builder.BuildWood(new CID(nameof(Cherry)),
        (Language.CherryLeaves, Language.CherryLog, Language.CherryWood),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Ash tree wood.
    /// </summary>
    public Wood AshTree { get; } = builder.BuildWood(new CID(nameof(AshTree)),
        (Language.AshTreeLeaves, Language.AshTreeLog, Language.AshTreeWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Rubber tree wood.
    /// </summary>
    public Wood RubberTree { get; } = builder.BuildWood(new CID(nameof(RubberTree)),
        (Language.RubberTreeLeaves, Language.RubberTreeLog, Language.RubberTreeWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Pine wood.
    /// </summary>
    public Wood Pine { get; } = builder.BuildWood(new CID(nameof(Pine)),
        (Language.PineLeaves, Language.PineLog, Language.PineWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Normal, Needles: true));

    /// <summary>
    ///     Spruce wood.
    /// </summary>
    public Wood Spruce { get; } = builder.BuildWood(new CID(nameof(Spruce)),
        (Language.SpruceLeaves, Language.SpruceLog, Language.SpruceWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Dense, Needles: true));

    /// <summary>
    ///     Fir wood.
    /// </summary>
    public Wood Fir { get; } = builder.BuildWood(new CID(nameof(Fir)),
        (Language.FirLeaves, Language.FirLog, Language.FirWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Dense, Needles: true));

    /// <summary>
    ///     Mahogany wood.
    /// </summary>
    public Wood Mahogany { get; } = builder.BuildWood(new CID(nameof(Mahogany)),
        (Language.MahoganyLeaves, Language.MahoganyLog, Language.MahoganyWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Teak wood.
    /// </summary>
    public Wood Teak { get; } = builder.BuildWood(new CID(nameof(Teak)),
        (Language.TeakLeaves, Language.TeakLog, Language.TeakWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Ebony wood.
    /// </summary>
    public Wood Ebony { get; } = builder.BuildWood(new CID(nameof(Ebony)),
        (Language.EbonyLeaves, Language.EbonyLog, Language.EbonyWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Coconut palm wood.
    /// </summary>
    public Wood CoconutPalm { get; } = builder.BuildWood(new CID(nameof(CoconutPalm)),
        (Language.CoconutPalmLeaves, Language.CoconutPalmLog, Language.CoconutPalmWood),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Palm, Wood.Tree.CrownDensity.Sparse, Terrain: Wood.Tree.TerrainType.Sand));

    /// <summary>
    ///     Date palm wood.
    /// </summary>
    public Wood DatePalm { get; } = builder.BuildWood(new CID(nameof(DatePalm)),
        (Language.DatePalmLeaves, Language.DatePalmLog, Language.DatePalmWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Palm, Wood.Tree.CrownDensity.Sparse, Terrain: Wood.Tree.TerrainType.Sand));

    /// <summary>
    ///     Acacia wood.
    /// </summary>
    public Wood Acacia { get; } = builder.BuildWood(new CID(nameof(Acacia)),
        (Language.AcaciaLeaves, Language.AcaciaLog, Language.AcaciaWood),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Baobab wood.
    /// </summary>
    public Wood Baobab { get; } = builder.BuildWood(new CID(nameof(Baobab)),
        (Language.BaobabLeaves, Language.BaobabLog, Language.BaobabWood),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Shepherd's tree wood.
    /// </summary>
    public Wood ShepherdsTree { get; } = builder.BuildWood(new CID(nameof(ShepherdsTree)),
        (Language.ShepherdsTreeLeaves, Language.ShepherdsTreeLog, Language.ShepherdsTreeWood),
        new Wood.Tree(Wood.Tree.Growth.Shrub, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Juniper wood.
    /// </summary>
    public Wood Juniper { get; } = builder.BuildWood(new CID(nameof(Juniper)),
        (Language.JuniperLeaves, Language.JuniperLog, Language.JuniperWood),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Mesquite wood.
    /// </summary>
    public Wood Mesquite { get; } = builder.BuildWood(new CID(nameof(Mesquite)),
        (Language.MesquiteLeaves, Language.MesquiteLog, Language.MesquiteWood),
        new Wood.Tree(Wood.Tree.Growth.Shrub, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Sparse));
}

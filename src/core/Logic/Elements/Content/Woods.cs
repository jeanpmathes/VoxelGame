// <copyright file="Woods.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// All sorts of wood types. Trees can be found in the world and can be used for the construction of various things.
/// </summary>
public class Woods(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Oak wood.
    /// </summary>
    public Wood Oak { get; } = builder.BuildWood(
        (Language.OakLeaves, Language.OakLog, Language.OakWood),
        nameof(Oak),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Maple wood.
    /// </summary>
    public Wood Maple { get; } = builder.BuildWood(
        (Language.MapleLeaves, Language.MapleLog, Language.MapleWood),
        nameof(Maple),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Birch wood.
    /// </summary>
    public Wood Birch { get; } = builder.BuildWood(
        (Language.BirchLeaves, Language.BirchLog, Language.BirchWood),
        nameof(Birch),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Maple wood.
    /// </summary>
    public Wood Walnut { get; } = builder.BuildWood(
        (Language.WalnutLeaves, Language.WalnutLog, Language.WalnutWood),
        nameof(Walnut),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Cherry wood.
    /// </summary>
    public Wood Cherry { get; } = builder.BuildWood(
        (Language.CherryLeaves, Language.CherryLog, Language.CherryWood),
        nameof(Cherry),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Ash tree wood.
    /// </summary>
    public Wood AshTree { get; } = builder.BuildWood(
        (Language.AshTreeLeaves, Language.AshTreeLog, Language.AshTreeWood),
        nameof(AshTree),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Normal));

    /// <summary>
    ///     Rubber tree wood.
    /// </summary>
    public Wood RubberTree { get; } = builder.BuildWood(
        (Language.RubberTreeLeaves, Language.RubberTreeLog, Language.RubberTreeWood),
        nameof(RubberTree),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Pine wood.
    /// </summary>
    public Wood Pine { get; } = builder.BuildWood(
        (Language.PineLeaves, Language.PineLog, Language.PineWood),
        nameof(Pine),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Normal, Needles: true));

    /// <summary>
    ///     Spruce wood.
    /// </summary>
    public Wood Spruce { get; } = builder.BuildWood(
        (Language.SpruceLeaves, Language.SpruceLog, Language.SpruceWood),
        nameof(Spruce),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Dense, Needles: true));

    /// <summary>
    ///     Fir wood.
    /// </summary>
    public Wood Fir { get; } = builder.BuildWood(
        (Language.FirLeaves, Language.FirLog, Language.FirWood),
        nameof(Fir),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Dense, Needles: true));

    /// <summary>
    ///     Mahogany wood.
    /// </summary>
    public Wood Mahogany { get; } = builder.BuildWood(
        (Language.MahoganyLeaves, Language.MahoganyLog, Language.MahoganyWood),
        nameof(Mahogany),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Teak wood.
    /// </summary>
    public Wood Teak { get; } = builder.BuildWood(
        (Language.TeakLeaves, Language.TeakLog, Language.TeakWood),
        nameof(Teak),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Ebony wood.
    /// </summary>
    public Wood Ebony { get; } = builder.BuildWood(
        (Language.EbonyLeaves, Language.EbonyLog, Language.EbonyWood),
        nameof(Ebony),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.LongSpheroid, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Coconut palm wood.
    /// </summary>
    public Wood CoconutPalm { get; } = builder.BuildWood(
        (Language.CoconutPalmLeaves, Language.CoconutPalmLog, Language.CoconutPalmWood),
        nameof(CoconutPalm),
        new Wood.Tree(Wood.Tree.Growth.Tall, Wood.Tree.CrownShape.Palm, Wood.Tree.CrownDensity.Sparse, Ground: Wood.Tree.GroundType.Sand));

    /// <summary>
    ///     Date palm wood.
    /// </summary>
    public Wood DatePalm { get; } = builder.BuildWood(
        (Language.DatePalmLeaves, Language.DatePalmLog, Language.DatePalmWood),
        nameof(DatePalm),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Palm, Wood.Tree.CrownDensity.Sparse, Ground: Wood.Tree.GroundType.Sand));

    /// <summary>
    ///     Acacia wood.
    /// </summary>
    public Wood Acacia { get; } = builder.BuildWood(
        (Language.AcaciaLeaves, Language.AcaciaLog, Language.AcaciaWood),
        nameof(Acacia),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Baobab wood.
    /// </summary>
    public Wood Baobab { get; } = builder.BuildWood(
        (Language.BaobabLeaves, Language.BaobabLog, Language.BaobabWood),
        nameof(Baobab),
        new Wood.Tree(Wood.Tree.Growth.Medium, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Shepherd's tree wood.
    /// </summary>
    public Wood ShepherdsTree { get; } = builder.BuildWood(
        (Language.ShepherdsTreeLeaves, Language.ShepherdsTreeLog, Language.ShepherdsTreeWood),
        nameof(ShepherdsTree),
        new Wood.Tree(Wood.Tree.Growth.Shrub, Wood.Tree.CrownShape.Sphere, Wood.Tree.CrownDensity.Dense));

    /// <summary>
    ///     Juniper wood.
    /// </summary>
    public Wood Juniper { get; } = builder.BuildWood(
        (Language.JuniperLeaves, Language.JuniperLog, Language.JuniperWood),
        nameof(Juniper),
        new Wood.Tree(Wood.Tree.Growth.Short, Wood.Tree.CrownShape.Cone, Wood.Tree.CrownDensity.Sparse));

    /// <summary>
    ///     Mesquite wood.
    /// </summary>
    public Wood Mesquite { get; } = builder.BuildWood(
        (Language.MesquiteLeaves, Language.MesquiteLog, Language.MesquiteWood),
        nameof(Mesquite),
        new Wood.Tree(Wood.Tree.Growth.Shrub, Wood.Tree.CrownShape.FlatSpheroid, Wood.Tree.CrownDensity.Sparse));

}

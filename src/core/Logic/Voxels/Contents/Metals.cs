// <copyright file="Metals.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Materials;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     All sorts of metals and their ores, as well as other metal-related blocks.
/// </summary>
public class Metals(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Iron is the metal with the elemental symbol Fe.
    /// </summary>
    public Metal Iron { get; } = builder.BuildMetal(new CID(nameof(Iron)),
        [
            (new CID(nameof(Language.OreMagnetite)), Language.OreMagnetite),
            (new CID(nameof(Language.OreHematite)), Language.OreHematite)
        ],
        []);

    /// <summary>
    ///     Gold is the metal with the elemental symbol Au.
    /// </summary>
    public Metal Gold { get; } = builder.BuildMetal(new CID(nameof(Gold)),
        [],
        [
            (new CID(nameof(Language.NativeGold)), Language.NativeGold),
            (new CID(nameof(Language.Electrum)), Language.Electrum)
        ]);

    /// <summary>
    ///     Silver is the metal with the elemental symbol Ag.
    /// </summary>
    public Metal Silver { get; } = builder.BuildMetal(new CID(nameof(Silver)),
        [],
        [
            (new CID(nameof(Language.NativeSilver)), Language.NativeSilver)
        ]);

    /// <summary>
    ///     Platinum is the metal with the elemental symbol Pt.
    /// </summary>
    public Metal Platinum { get; } = builder.BuildMetal(new CID(nameof(Platinum)),
        [],
        [
            (new CID(nameof(Language.NativePlatinum)), Language.NativePlatinum)
        ]);

    /// <summary>
    ///     Copper is the metal with the elemental symbol Cu.
    /// </summary>
    public Metal Copper { get; } = builder.BuildMetal(new CID(nameof(Copper)),
        [
            (new CID(nameof(Language.OreChalcopyrite)), Language.OreChalcopyrite),
            (new CID(nameof(Language.OreMalachite)), Language.OreMalachite)
        ],
        [
            (new CID(nameof(Language.NativeCopper)), Language.NativeCopper)
        ]);

    /// <summary>
    ///     Aluminum is the metal with the elemental symbol Al.
    /// </summary>
    public Metal Aluminum { get; } = builder.BuildMetal(new CID(nameof(Aluminum)),
        [
            (new CID(nameof(Language.OreBauxite)), Language.OreBauxite)
        ],
        []);

    /// <summary>
    ///     Lead is the metal with the elemental symbol Pb.
    /// </summary>
    public Metal Lead { get; } = builder.BuildMetal(new CID(nameof(Lead)),
        [
            (new CID(nameof(Language.OreGalena)), Language.OreGalena)
        ],
        []);

    /// <summary>
    ///     Tin is the metal with the elemental symbol Sn.
    /// </summary>
    public Metal Tin { get; } = builder.BuildMetal(new CID(nameof(Tin)),
        [
            (new CID(nameof(Language.OreCassiterite)), Language.OreCassiterite)
        ],
        []);

    /// <summary>
    ///     Mercury is the metal with the elemental symbol Hg.
    /// </summary>
    public Metal Mercury { get; } = builder.BuildMetal(new CID(nameof(Mercury)),
        [
            (new CID(nameof(Language.OreCinnabar)), Language.OreCinnabar)
        ],
        []);

    /// <summary>
    ///     Zinc is the metal with the elemental symbol Zn.
    /// </summary>
    public Metal Zinc { get; } = builder.BuildMetal(new CID(nameof(Zinc)),
        [
            (new CID(nameof(Language.OreSphalerite)), Language.OreSphalerite)
        ],
        []);

    /// <summary>
    ///     Chromium is the metal with the elemental symbol Cr.
    /// </summary>
    public Metal Chromium { get; } = builder.BuildMetal(new CID(nameof(Chromium)),
        [
            (new CID(nameof(Language.OreChromite)), Language.OreChromite)
        ],
        []);

    /// <summary>
    ///     Manganese is the metal with the elemental symbol Mn.
    /// </summary>
    public Metal Manganese { get; } = builder.BuildMetal(new CID(nameof(Manganese)),
        [
            (new CID(nameof(Language.OrePyrolusite)), Language.OrePyrolusite)
        ],
        []);

    /// <summary>
    ///     Titanium is the metal with the elemental symbol Ti.
    /// </summary>
    public Metal Titanium { get; } = builder.BuildMetal(new CID(nameof(Titanium)),
        [
            (new CID(nameof(Language.OreRutile)), Language.OreRutile)
        ],
        []);

    /// <summary>
    ///     Nickel is the metal with the elemental symbol Ni.
    /// </summary>
    public Metal Nickel { get; } = builder.BuildMetal(new CID(nameof(Nickel)),
        [
            (new CID(nameof(Language.OrePentlandite)), Language.OrePentlandite)
        ],
        []);

    /// <summary>
    ///     Zirconium is the metal with the elemental symbol Zr.
    /// </summary>
    public Metal Zirconium { get; } = builder.BuildMetal(new CID(nameof(Zirconium)),
        [
            (new CID(nameof(Language.OreZircon)), Language.OreZircon)
        ],
        []);

    /// <summary>
    ///     Magnesium is the metal with the elemental symbol Mg.
    /// </summary>
    public Metal Magnesium { get; } = builder.BuildMetal(new CID(nameof(Magnesium)),
        [
            (new CID(nameof(Language.OreDolomite)), Language.OreDolomite)
        ],
        []);

    /// <summary>
    ///     Strontium is the metal with the elemental symbol Sr.
    /// </summary>
    public Metal Strontium { get; } = builder.BuildMetal(new CID(nameof(Strontium)),
        [
            (new CID(nameof(Language.OreCelestine)), Language.OreCelestine)
        ],
        []);

    /// <summary>
    ///     Uranium is the metal with the elemental symbol U.
    /// </summary>
    public Metal Uranium { get; } = builder.BuildMetal(new CID(nameof(Uranium)),
        [
            (new CID(nameof(Language.OreUraninite)), Language.OreUraninite)
        ],
        []);

    /// <summary>
    ///     Bismuth is the metal with the elemental symbol Bi.
    /// </summary>
    public Metal Bismuth { get; } = builder.BuildMetal(new CID(nameof(Bismuth)),
        [
            (new CID(nameof(Language.OreBismuthinite)), Language.OreBismuthinite)
        ],
        []);

    /// <summary>
    ///     Beryllium is the metal with the elemental symbol Be.
    /// </summary>
    public Metal Beryllium { get; } = builder.BuildMetal(new CID(nameof(Beryllium)),
        [
            (new CID(nameof(Language.OreBeryl)), Language.OreBeryl)
        ],
        []);

    /// <summary>
    ///     Molybdenum is the metal with the elemental symbol Mo.
    /// </summary>
    public Metal Molybdenum { get; } = builder.BuildMetal(new CID(nameof(Molybdenum)),
        [
            (new CID(nameof(Language.OreMolybdenite)), Language.OreMolybdenite)
        ],
        []);

    /// <summary>
    ///     Cobalt is the metal with the elemental symbol Co.
    /// </summary>
    public Metal Cobalt { get; } = builder.BuildMetal(new CID(nameof(Cobalt)),
        [
            (new CID(nameof(Language.OreCobaltite)), Language.OreCobaltite)
        ],
        []);

    /// <summary>
    ///     Lithium is the metal with the elemental symbol Li.
    /// </summary>
    public Metal Lithium { get; } = builder.BuildMetal(new CID(nameof(Lithium)),
        [
            (new CID(nameof(Language.OreSpodumene)), Language.OreSpodumene)
        ],
        []);

    /// <summary>
    ///     Vanadium is the metal with the elemental symbol V.
    /// </summary>
    public Metal Vanadium { get; } = builder.BuildMetal(new CID(nameof(Vanadium)),
        [
            (new CID(nameof(Language.OreVanadinite)), Language.OreVanadinite)
        ],
        []);

    /// <summary>
    ///     Tungsten is the metal with the elemental symbol W.
    /// </summary>
    public Metal Tungsten { get; } = builder.BuildMetal(new CID(nameof(Tungsten)),
        [
            (new CID(nameof(Language.OreScheelite)), Language.OreScheelite)
        ],
        []);

    /// <summary>
    ///     Cadmium is the metal with the elemental symbol Cd.
    /// </summary>
    public Metal Cadmium { get; } = builder.BuildMetal(new CID(nameof(Cadmium)),
        [
            (new CID(nameof(Language.OreGreenockite)), Language.OreGreenockite) // note: sphalerite is a zinc ore, but it can also contain cadmium
        ],
        []);

    /// <summary>
    ///     When iron is exposed to oxygen and moisture, it rusts.
    ///     This blocks is a large accumulation of rust.
    /// </summary>
    public Block Rust { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Language.Rust)), Language.Rust)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("rust")))
        .Complete();

    /// <summary>
    ///     The steel block is a metal construction block.
    /// </summary>
    public Block Steel { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Steel)), Language.Steel)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("steel")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
}

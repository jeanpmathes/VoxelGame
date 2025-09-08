// <copyright file="Metals.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Behaviors.Materials;
using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// All sorts of metals and their ores, as well as other metal-related blocks.
/// </summary>
public class Metals(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    /// Iron is the metal with the elemental symbol Fe.
    /// </summary>
    public Metal Iron { get; } = builder.BuildMetal(nameof(Iron), [
            (Language.OreMagnetite, nameof(Language.OreMagnetite)),
            (Language.OreHematite, nameof(Language.OreHematite)),
        ], []);
    
    /// <summary>
    /// Gold is the metal with the elemental symbol Au.
    /// </summary>
    public Metal Gold { get; } = builder.BuildMetal(nameof(Gold), [], [
            (Language.NativeGold, nameof(Language.NativeGold)),
            (Language.Electrum, nameof(Language.Electrum))
        ]);
    
    /// <summary>
    /// Silver is the metal with the elemental symbol Ag.
    /// </summary>
    public Metal Silver { get; } = builder.BuildMetal(nameof(Silver), [], [
            (Language.NativeSilver, nameof(Language.NativeSilver))
        ]);
    
    /// <summary>
    /// Platinum is the metal with the elemental symbol Pt.
    /// </summary>
    public Metal Platinum { get; } = builder.BuildMetal(nameof(Platinum), [], [
            (Language.NativePlatinum, nameof(Language.NativePlatinum)),
        ]);
    
    /// <summary>
    /// Copper is the metal with the elemental symbol Cu.
    /// </summary>
    public Metal Copper { get; } = builder.BuildMetal(nameof(Copper), [
            (Language.OreChalcopyrite, nameof(Language.OreChalcopyrite)),
            (Language.OreMalachite, nameof(Language.OreMalachite)),
        ], [
            (Language.NativeCopper, nameof(Language.NativeCopper)), 
        ]);
    
    /// <summary>
    /// Aluminum is the metal with the elemental symbol Al.
    /// </summary>
    public Metal Aluminum { get; } = builder.BuildMetal(nameof(Aluminum), [
            (Language.OreBauxite, nameof(Language.OreBauxite)),
        ], []);
    
    /// <summary>
    /// Lead is the metal with the elemental symbol Pb.
    /// </summary>
    public Metal Lead { get; } = builder.BuildMetal(nameof(Lead), [
            (Language.OreGalena, nameof(Language.OreGalena)),
        ], []);
    
    /// <summary>
    /// Tin is the metal with the elemental symbol Sn.
    /// </summary>
    public Metal Tin { get; } = builder.BuildMetal(nameof(Tin), [
            (Language.OreCassiterite, nameof(Language.OreCassiterite)),
        ], []);

    /// <summary>
    /// Mercury is the metal with the elemental symbol Hg.
    /// </summary>
    public Metal Mercury { get; } = builder.BuildMetal(nameof(Mercury),
        [
            (Language.OreCinnabar, nameof(Language.OreCinnabar)),
        ], []);
    
    /// <summary>
    /// Zinc is the metal with the elemental symbol Zn.
    /// </summary>
    public Metal Zinc { get; } = builder.BuildMetal(nameof(Zinc), [
            (Language.OreSphalerite, nameof(Language.OreSphalerite)),
        ], []);
    
    /// <summary>
    /// Chromium is the metal with the elemental symbol Cr.
    /// </summary>
    public Metal Chromium { get; } = builder.BuildMetal(nameof(Chromium), [
            (Language.OreChromite, nameof(Language.OreChromite)),
        ], []);
    
    /// <summary>
    /// Manganese is the metal with the elemental symbol Mn.
    /// </summary>
    public Metal Manganese { get; } = builder.BuildMetal(nameof(Manganese), [
            (Language.OrePyrolusite, nameof(Language.OrePyrolusite)),
        ], []);
    
    /// <summary>
    /// Titanium is the metal with the elemental symbol Ti.
    /// </summary>
    public Metal Titanium { get; } = builder.BuildMetal(nameof(Titanium), [
            (Language.OreRutile, nameof(Language.OreRutile)),
        ], []);
    
    /// <summary>
    /// Nickel is the metal with the elemental symbol Ni.
    /// </summary>
    public Metal Nickel { get; } = builder.BuildMetal(nameof(Nickel), [
            (Language.OrePentlandite, nameof(Language.OrePentlandite)),
        ], []);
    
    /// <summary>
    /// Zirconium is the metal with the elemental symbol Zr.
    /// </summary>
    public Metal Zirconium { get; } = builder.BuildMetal(nameof(Zirconium), [
            (Language.OreZircon, nameof(Language.OreZircon)),
        ], []);
    
    /// <summary>
    /// Magnesium is the metal with the elemental symbol Mg.
    /// </summary>
    public Metal Magnesium { get; } = builder.BuildMetal(nameof(Magnesium), [
            (Language.OreDolomite, nameof(Language.OreDolomite)),
        ], []);
    
    /// <summary>
    /// Strontium is the metal with the elemental symbol Sr.
    /// </summary>
    public Metal Strontium { get; } = builder.BuildMetal(nameof(Strontium), [
            (Language.OreCelestine, nameof(Language.OreCelestine)),
        ], []);
    
    /// <summary>
    /// Uranium is the metal with the elemental symbol U.
    /// </summary>
    public Metal Uranium { get; } = builder.BuildMetal(nameof(Uranium), [
            (Language.OreUraninite, nameof(Language.OreUraninite)),
        ], []);
    
    /// <summary>
    /// Bismuth is the metal with the elemental symbol Bi.
    /// </summary>
    public Metal Bismuth { get; } = builder.BuildMetal(nameof(Bismuth), [
            (Language.OreBismuthinite, nameof(Language.OreBismuthinite)),
        ], []);
    
    /// <summary>
    /// Beryllium is the metal with the elemental symbol Be.
    /// </summary>
    public Metal Beryllium { get; } = builder.BuildMetal(nameof(Beryllium), [
            (Language.OreBeryl, nameof(Language.OreBeryl)),
        ], []);
    
    /// <summary>
    /// Molybdenum is the metal with the elemental symbol Mo.
    /// </summary>
    public Metal Molybdenum { get; } = builder.BuildMetal(nameof(Molybdenum), [
            (Language.OreMolybdenite, nameof(Language.OreMolybdenite)),
        ], []);
    
    /// <summary>
    /// Cobalt is the metal with the elemental symbol Co.
    /// </summary>
    public Metal Cobalt { get; } = builder.BuildMetal(nameof(Cobalt), [
            (Language.OreCobaltite, nameof(Language.OreCobaltite)),
        ], []);
    
    /// <summary>
    /// Lithium is the metal with the elemental symbol Li.
    /// </summary>
    public Metal Lithium { get; } = builder.BuildMetal(nameof(Lithium), [
            (Language.OreSpodumene, nameof(Language.OreSpodumene)),
        ], []);
    
    /// <summary>
    /// Vanadium is the metal with the elemental symbol V.
    /// </summary>
    public Metal Vanadium { get; } = builder.BuildMetal(nameof(Vanadium), [
            (Language.OreVanadinite, nameof(Language.OreVanadinite)),
        ], []);
    
    /// <summary>
    /// Tungsten is the metal with the elemental symbol W.
    /// </summary>
    public Metal Tungsten { get; } = builder.BuildMetal(nameof(Tungsten), [
            (Language.OreScheelite, nameof(Language.OreScheelite)),
        ], []);
    
    /// <summary>
    /// Cadmium is the metal with the elemental symbol Cd.
    /// </summary>
    public Metal Cadmium { get; } = builder.BuildMetal(nameof(Cadmium), [
            (Language.OreGreenockite, nameof(Language.OreGreenockite)), // note: sphalerite is a zinc ore, but it can also contain cadmium
        ], []);
    
    /// <summary>
    ///     When iron is exposed to oxygen and moisture, it rusts.
    ///     This blocks is a large accumulation of rust.
    /// </summary>
    public Block Rust { get; } = builder
        .BuildSimpleBlock(nameof(Language.Rust), Language.Rust)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("rust")))
        .Complete();
    
    /// <summary>
    ///     The steel block is a metal construction block.
    /// </summary>
    public Block Steel { get; } = builder
        .BuildSimpleBlock(nameof(Steel), Language.Steel)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("steel")))
        .WithBehavior<ConstructionMaterial>()
        .Complete();
}

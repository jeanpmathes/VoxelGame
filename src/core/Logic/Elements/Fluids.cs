﻿// <copyright file="Fluids.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Definitions.Fluids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     Contains all fluid definitions of the core game.
/// </summary>
public sealed partial class Fluids(Registry<Fluid> registry)
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const Int32 mPas = 15;

    private static FluidContactManager? contactManager;

    /// <summary>
    ///     Get the fluids instance.
    /// </summary>
    public static Fluids Instance { get; } = new(new Registry<Fluid>(fluid => fluid.NamedID));

    /// <summary>
    ///     The absence of a fluid.
    /// </summary>
    public Fluid None { get; } = registry.Register(new NoFluid(Language.NoFluid, nameof(None)));

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public Fluid FreshWater { get; } = registry.Register(new BasicFluid(
        Language.FreshWater,
        nameof(FreshWater),
        density: 997f,
        1 * mPas,
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("fresh_water_moving_side"), TID.Fluid("fresh_water_moving")),
        TextureLayout.Fluid(TID.Fluid("fresh_water_static_side"), TID.Fluid("fresh_water_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public Fluid SeaWater { get; } = registry.Register(new SaltWaterFluid(
        Language.SeaWater,
        nameof(SeaWater),
        density: 1023f,
        1 * mPas,
        TextureLayout.Fluid(TID.Fluid("sea_water_moving_side"), TID.Fluid("sea_water_moving")),
        TextureLayout.Fluid(TID.Fluid("sea_water_static_side"), TID.Fluid("sea_water_static"))));

    /// <summary>
    ///     Milk is a white fluid that is obtained from animals.
    /// </summary>
    public Fluid Milk { get; } = registry.Register(new BasicFluid(
        Language.Milk,
        nameof(Milk),
        density: 1033f,
        2 * mPas,
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("milk_moving_side"), TID.Fluid("milk_moving")),
        TextureLayout.Fluid(TID.Fluid("milk_static_side"), TID.Fluid("milk_static"))));

    /// <summary>
    ///     Steam is a gas created when water is heated.
    /// </summary>
    public Fluid Steam { get; } = registry.Register(new BasicFluid(
        Language.Steam,
        nameof(Steam),
        density: 0.5f,
        (Int32) (0.25 * mPas),
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("steam_moving_side"), TID.Fluid("steam_moving")),
        TextureLayout.Fluid(TID.Fluid("steam_static_side"), TID.Fluid("steam_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Lava is a hot fluid, made out of molten stone. It burns flammable objects.
    /// </summary>
    public Fluid Lava { get; } = registry.Register(new HotFluid(
        Language.Lava,
        nameof(Lava),
        density: 3100f,
        15 * mPas,
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("lava_moving_side"), TID.Fluid("lava_moving")),
        TextureLayout.Fluid(TID.Fluid("lava_static_side"), TID.Fluid("lava_static"))));

    /// <summary>
    ///     Crude oil is a flammable fluid with a high viscosity. It is lighter than water.
    /// </summary>
    public Fluid CrudeOil { get; } = registry.Register(new BasicFluid(
        Language.CrudeOil,
        nameof(CrudeOil),
        density: 870f,
        8 * mPas,
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("oil_moving_side"), TID.Fluid("oil_moving")),
        TextureLayout.Fluid(TID.Fluid("oil_static_side"), TID.Fluid("oil_static"))));

    /// <summary>
    ///     Natural gas is a flammable gas.
    /// </summary>
    public Fluid NaturalGas { get; } = registry.Register(new BasicFluid(
        Language.NaturalGas,
        nameof(NaturalGas),
        density: 0.8f,
        (Int32) (0.5 * mPas),
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("gas_moving_side"), TID.Fluid("gas_moving")),
        TextureLayout.Fluid(TID.Fluid("gas_static_side"), TID.Fluid("gas_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Concrete is a fluid that hardens when staying still for some time, forming concrete blocks.
    /// </summary>
    public Fluid Concrete { get; } = registry.Register(new ConcreteFluid(
        Language.Concrete,
        nameof(Concrete),
        density: 2400f,
        10 * mPas,
        TextureLayout.Fluid(TID.Fluid("concrete_moving_side"), TID.Fluid("concrete_moving")),
        TextureLayout.Fluid(TID.Fluid("concrete_static_side"), TID.Fluid("concrete_static"))));

    /// <summary>
    ///     Honey is a thick fluid.
    /// </summary>
    public Fluid Honey { get; } = registry.Register(new BasicFluid(
        Language.Honey,
        nameof(Honey),
        density: 1450f,
        20 * mPas,
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("honey_moving_side"), TID.Fluid("honey_moving")),
        TextureLayout.Fluid(TID.Fluid("honey_static_side"), TID.Fluid("honey_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Petrol is a flammable fluid.
    /// </summary>
    public Fluid Petrol { get; } = registry.Register(new BasicFluid(
        Language.Petrol,
        nameof(Petrol),
        density: 740f,
        (Int32) (0.9 * mPas),
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("petrol_moving_side"), TID.Fluid("petrol_moving")),
        TextureLayout.Fluid(TID.Fluid("petrol_static_side"), TID.Fluid("petrol_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Wine is a reddish fluid.
    /// </summary>
    public Fluid Wine { get; } = registry.Register(new BasicFluid(
        Language.Wine,
        nameof(Wine),
        density: 1090f,
        (Int32) (1.4 * mPas),
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("wine_moving_side"), TID.Fluid("wine_moving")),
        TextureLayout.Fluid(TID.Fluid("wine_static_side"), TID.Fluid("wine_static")),
        RenderType.Transparent));

    /// <summary>
    ///     Beer is a brown fluid.
    /// </summary>
    public Fluid Beer { get; } = registry.Register(new BasicFluid(
        Language.Beer,
        nameof(Beer),
        density: 1030f,
        (Int32) (1.5 * mPas),
        hasNeutralTint: false,
        TextureLayout.Fluid(TID.Fluid("beer_moving_side"), TID.Fluid("beer_moving")),
        TextureLayout.Fluid(TID.Fluid("beer_static_side"), TID.Fluid("beer_static")),
        RenderType.Transparent));

    /// <summary>
    ///     The contact manager instance.
    /// </summary>
    public static FluidContactManager ContactManager
    {
        get
        {
            contactManager ??= new FluidContactManager(Instance);

            return contactManager;
        }
    }

    /// <summary>
    ///     Gets the count of registered fluids.
    /// </summary>
    public Int32 Count => registry.Count;

    /// <summary>
    ///     Get all fluids in this instance.
    /// </summary>
    public IEnumerable<Fluid> Content => registry.Values;

    /// <summary>
    ///     Translates a fluid ID to a reference to the fluid that has that ID. If the ID is not valid, none is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    public Fluid TranslateID(UInt32 id)
    {
        if (Count > id) return registry[(Int32) id];

        LogUnknownFluid(logger, id, None.NamedID);

        return None;
    }

    /// <summary>
    ///     Translate a named ID to the fluid with that ID.
    /// </summary>
    /// <param name="namedID">The named ID to translate.</param>
    /// <returns>The fluid, or null.</returns>
    public Fluid? TranslateNamedID(String namedID)
    {
        return registry[namedID];
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Fluids>();

    [LoggerMessage(EventId = LogID.Fluids + 0, Level = LogLevel.Warning, Message = "No Fluid with ID '{ID}' could be found, returning {Fallback} instead")]
    private static partial void LogUnknownFluid(ILogger logger, UInt32 id, String fallback);

    #endregion LOGGING
}

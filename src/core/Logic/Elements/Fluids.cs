﻿// <copyright file="Fluids.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
public partial class Fluids
{
    /// <summary>
    ///     The maximum amount of different fluids that can be registered.
    /// </summary>
    private const Int32 FluidLimit = 32;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const Int32 mPas = 15;

    private readonly List<Fluid> fluidList = [];
    private readonly Dictionary<String, Fluid> namedFluidDictionary = new();

    private Fluids(ITextureIndexProvider indexProvider, IDominantColorProvider dominantColorProvider, ILoadingContext loadingContext)
    {
        List<Fluid> allFluids = [];

        Fluid Register(Fluid fluid)
        {
            allFluids.Add(fluid);

            return fluid;
        }

        None = Register(new NoFluid(Language.NoFluid, nameof(None)));

        FreshWater = Register(new BasicFluid(
            Language.FreshWater,
            nameof(FreshWater),
            density: 997f,
            1 * mPas,
            hasNeutralTint: false,
            TextureLayout.Fluid("fresh_water_moving_side", "fresh_water_moving"),
            TextureLayout.Fluid("fresh_water_static_side", "fresh_water_static"),
            RenderType.Transparent));

        SeaWater = Register(new SaltWaterFluid(
            Language.SeaWater,
            nameof(SeaWater),
            density: 1023f,
            1 * mPas,
            TextureLayout.Fluid("sea_water_moving_side", "sea_water_moving"),
            TextureLayout.Fluid("sea_water_static_side", "sea_water_static")));

        Milk = Register(new BasicFluid(
            Language.Milk,
            nameof(Milk),
            density: 1033f,
            2 * mPas,
            hasNeutralTint: false,
            TextureLayout.Fluid("milk_moving_side", "milk_moving"),
            TextureLayout.Fluid("milk_static_side", "milk_static")));

        Steam = Register(new BasicFluid(
            Language.Steam,
            nameof(Steam),
            density: 0.5f,
            (Int32) (0.25 * mPas),
            hasNeutralTint: false,
            TextureLayout.Fluid("steam_moving_side", "steam_moving"),
            TextureLayout.Fluid("steam_static_side", "steam_static"),
            RenderType.Transparent));

        Lava = Register(new HotFluid(
            Language.Lava,
            nameof(Lava),
            density: 3100f,
            15 * mPas,
            hasNeutralTint: false,
            TextureLayout.Fluid("lava_moving_side", "lava_moving"),
            TextureLayout.Fluid("lava_static_side", "lava_static")));

        CrudeOil = Register(new BasicFluid(
            Language.CrudeOil,
            nameof(CrudeOil),
            density: 870f,
            8 * mPas,
            hasNeutralTint: false,
            TextureLayout.Fluid("oil_moving_side", "oil_moving"),
            TextureLayout.Fluid("oil_static_side", "oil_static")));

        NaturalGas = Register(new BasicFluid(
            Language.NaturalGas,
            nameof(NaturalGas),
            density: 0.8f,
            (Int32) (0.5 * mPas),
            hasNeutralTint: false,
            TextureLayout.Fluid("gas_moving_side", "gas_moving"),
            TextureLayout.Fluid("gas_static_side", "gas_static"),
            RenderType.Transparent));

        Concrete = Register(new ConcreteFluid(
            Language.Concrete,
            nameof(Concrete),
            density: 2400f,
            10 * mPas,
            TextureLayout.Fluid("concrete_moving_side", "concrete_moving"),
            TextureLayout.Fluid("concrete_static_side", "concrete_static")));

        Honey = Register(new BasicFluid(
            Language.Honey,
            nameof(Honey),
            density: 1450f,
            20 * mPas,
            hasNeutralTint: false,
            TextureLayout.Fluid("honey_moving_side", "honey_moving"),
            TextureLayout.Fluid("honey_static_side", "honey_static"),
            RenderType.Transparent));

        Petrol = Register(new BasicFluid(
            Language.Petrol,
            nameof(Petrol),
            density: 740f,
            (Int32) (0.9 * mPas),
            hasNeutralTint: false,
            TextureLayout.Fluid("petrol_moving_side", "petrol_moving"),
            TextureLayout.Fluid("petrol_static_side", "petrol_static"),
            RenderType.Transparent));

        Wine = Register(new BasicFluid(
            Language.Wine,
            nameof(Wine),
            density: 1090f,
            (Int32) (1.4 * mPas),
            hasNeutralTint: false,
            TextureLayout.Fluid("wine_moving_side", "wine_moving"),
            TextureLayout.Fluid("wine_static_side", "wine_static"),
            RenderType.Transparent));

        Beer = Register(new BasicFluid(
            Language.Beer,
            nameof(Beer),
            density: 1030f,
            (Int32) (1.5 * mPas),
            hasNeutralTint: false,
            TextureLayout.Fluid("beer_moving_side", "beer_moving"),
            TextureLayout.Fluid("beer_static_side", "beer_static"),
            RenderType.Transparent));

        #pragma warning disable S2583 // Could become reachable if too many fluids are added.
        if (allFluids.Count > FluidLimit)
            Debug.Fail($"Not more than {FluidLimit} fluids are allowed.");
        #pragma warning restore S2583

        foreach (Fluid fluid in allFluids.Take(FluidLimit))
        {
            fluidList.Add(fluid);
            namedFluidDictionary.Add(fluid.NamedID, fluid);

            var id = (UInt32) (fluidList.Count - 1);

            fluid.SetUp(id, indexProvider, dominantColorProvider);

            loadingContext.ReportSuccess(nameof(Fluid), fluid.NamedID);
        }

        ContactManager = new FluidContactManager(this);
    }

    /// <summary>
    ///     Get the fluids instance. Only available after a call to <see cref="Load" />
    /// </summary>
    public static Fluids Instance { get; private set; } = null!;

    /// <summary>
    ///     The absence of a fluid.
    /// </summary>
    public Fluid None { get; }

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public Fluid FreshWater { get; }

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public Fluid SeaWater { get; }

    /// <summary>
    ///     Milk is a white fluid that is obtained from animals.
    /// </summary>
    public Fluid Milk { get; }

    /// <summary>
    ///     Steam is a gas created when water is heated.
    /// </summary>
    public Fluid Steam { get; }

    /// <summary>
    ///     Lava is a hot fluid, made out of molten stone. It burns flammable objects.
    /// </summary>
    public Fluid Lava { get; }

    /// <summary>
    ///     Crude oil is a flammable fluid with a high viscosity. It is lighter than water.
    /// </summary>
    public Fluid CrudeOil { get; }

    /// <summary>
    ///     Natural gas is a flammable gas.
    /// </summary>
    public Fluid NaturalGas { get; }

    /// <summary>
    ///     Concrete is a fluid that hardens when staying still for some time, forming concrete blocks.
    /// </summary>
    public Fluid Concrete { get; }

    /// <summary>
    ///     Honey is a thick fluid.
    /// </summary>
    public Fluid Honey { get; }

    /// <summary>
    ///     Petrol is a flammable fluid.
    /// </summary>
    public Fluid Petrol { get; }

    /// <summary>
    ///     Wine is a reddish fluid.
    /// </summary>
    public Fluid Wine { get; }

    /// <summary>
    ///     Beer is a brown fluid.
    /// </summary>
    public Fluid Beer { get; }

    /// <summary>
    ///     The contact manager instance.
    /// </summary>
    public FluidContactManager ContactManager { get; }

    /// <summary>
    ///     Gets the count of registered fluids..
    /// </summary>
    public Int32 Count => fluidList.Count;

    /// <summary>
    ///     Translates a fluid ID to a reference to the fluid that has that ID. If the ID is not valid, none is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    public Fluid TranslateID(UInt32 id)
    {
        if (fluidList.Count > id) return fluidList[(Int32) id];

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
        namedFluidDictionary.TryGetValue(namedID, out Fluid? fluid);

        return fluid;
    }

    /// <summary>
    ///     Calls the setup method on all blocks.
    /// </summary>
    public static void Load(ITextureIndexProvider indexProvider, IDominantColorProvider dominantColorProvider, ILoadingContext loadingContext)
    {
        using (loadingContext.BeginStep("Fluid Loading"))
        {
            Instance = new Fluids(indexProvider, dominantColorProvider, loadingContext);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Fluids>();

    [LoggerMessage(EventId = Events.UnknownFluid, Level = LogLevel.Warning, Message = "No Fluid with ID '{ID}' could be found, returning {Fallback} instead")]
    private static partial void LogUnknownFluid(ILogger logger, UInt32 id, String fallback);

    #endregion LOGGING
}

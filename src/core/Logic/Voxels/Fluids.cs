// <copyright file="Fluids.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Logic.Contents.Fluids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic.Voxels;

#pragma warning disable S1192 // Hardcoded string duplication is close together.

/// <summary>
///     Contains all fluid definitions of the core game.
/// </summary>
public sealed partial class Fluids(Registry<Fluid> registry)
{
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
        new Density {KilogramsPerCubicMeter = 997f},
        new Viscosity {MilliPascalSeconds = 1},
        hasNeutralTint: false,
        TID.Fluid("fresh_water"),
        RenderType.Transparent));

    /// <summary>
    ///     Waste water is contaminated fresh water.
    /// </summary>
    public Fluid WasteWater { get; } = registry.Register(new BasicFluid(
        Language.WasteWater,
        nameof(WasteWater),
        new Density {KilogramsPerCubicMeter = 997f},
        new Viscosity {MilliPascalSeconds = 1},
        hasNeutralTint: false,
        TID.Fluid("waste_water"),
        RenderType.Transparent));

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public Fluid SeaWater { get; } = registry.Register(new SaltWaterFluid(
        Language.SeaWater,
        nameof(SeaWater),
        new Density {KilogramsPerCubicMeter = 1023f},
        new Viscosity {MilliPascalSeconds = 1},
        TID.Fluid("sea_water")));

    /// <summary>
    ///     Milk is a white fluid that is obtained from animals.
    /// </summary>
    public Fluid Milk { get; } = registry.Register(new BasicFluid(
        Language.Milk,
        nameof(Milk),
        new Density {KilogramsPerCubicMeter = 1033f},
        new Viscosity {MilliPascalSeconds = 2},
        hasNeutralTint: false,
        TID.Fluid("milk")));

    /// <summary>
    ///     Steam is a gas created when water is heated.
    /// </summary>
    public Fluid Steam { get; } = registry.Register(new BasicFluid(
        Language.Steam,
        nameof(Steam),
        new Density {KilogramsPerCubicMeter = 0.5f},
        new Viscosity {MilliPascalSeconds = 0.25},
        hasNeutralTint: false,
        TID.Fluid("steam"),
        RenderType.Transparent));

    /// <summary>
    ///     Lava is a hot fluid, made out of molten stone. It burns flammable objects.
    /// </summary>
    public Fluid Lava { get; } = registry.Register(new HotFluid(
        Language.Lava,
        nameof(Lava),
        new Density {KilogramsPerCubicMeter = 3100f},
        new Viscosity {MilliPascalSeconds = 15},
        hasNeutralTint: false,
        TID.Fluid("lava")));

    /// <summary>
    ///     Crude oil is a flammable fluid with a high viscosity. It is lighter than water.
    /// </summary>
    public Fluid CrudeOil { get; } = registry.Register(new BasicFluid(
        Language.CrudeOil,
        nameof(CrudeOil),
        new Density {KilogramsPerCubicMeter = 870f},
        new Viscosity {MilliPascalSeconds = 8},
        hasNeutralTint: false,
        TID.Fluid("oil")));

    /// <summary>
    ///     Natural gas is a flammable gas.
    /// </summary>
    public Fluid NaturalGas { get; } = registry.Register(new BasicFluid(
        Language.NaturalGas,
        nameof(NaturalGas),
        new Density {KilogramsPerCubicMeter = 0.8f},
        new Viscosity {MilliPascalSeconds = 0.5},
        hasNeutralTint: false,
        TID.Fluid("gas"),
        RenderType.Transparent));

    /// <summary>
    ///     Concrete is a fluid that hardens when staying still for some time, forming concrete blocks.
    /// </summary>
    public Fluid Concrete { get; } = registry.Register(new ConcreteFluid(
        Language.Concrete,
        nameof(Concrete),
        new Density {KilogramsPerCubicMeter = 2400f},
        new Viscosity {MilliPascalSeconds = 10},
        TID.Fluid("concrete")));

    /// <summary>
    ///     Honey is a thick fluid.
    /// </summary>
    public Fluid Honey { get; } = registry.Register(new BasicFluid(
        Language.Honey,
        nameof(Honey),
        new Density {KilogramsPerCubicMeter = 1450f},
        new Viscosity {MilliPascalSeconds = 20},
        hasNeutralTint: false,
        TID.Fluid("honey"),
        RenderType.Transparent));

    /// <summary>
    ///     Petrol is a flammable fluid.
    /// </summary>
    public Fluid Petrol { get; } = registry.Register(new BasicFluid(
        Language.Petrol,
        nameof(Petrol),
        new Density {KilogramsPerCubicMeter = 740f},
        new Viscosity {MilliPascalSeconds = 0.9},
        hasNeutralTint: false,
        TID.Fluid("petrol"),
        RenderType.Transparent));

    /// <summary>
    ///     Wine is a reddish fluid.
    /// </summary>
    public Fluid Wine { get; } = registry.Register(new BasicFluid(
        Language.Wine,
        nameof(Wine),
        new Density {KilogramsPerCubicMeter = 1090f},
        new Viscosity {MilliPascalSeconds = 1.4},
        hasNeutralTint: false,
        TID.Fluid("wine"),
        RenderType.Transparent));

    /// <summary>
    ///     Beer is a brown fluid.
    /// </summary>
    public Fluid Beer { get; } = registry.Register(new BasicFluid(
        Language.Beer,
        nameof(Beer),
        new Density {KilogramsPerCubicMeter = 1030f},
        new Viscosity {MilliPascalSeconds = 1.5},
        hasNeutralTint: false,
        TID.Fluid("beer"),
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

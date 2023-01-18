// <copyright file="FluidManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Definitions.Fluids;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic;

public abstract partial class Fluid
{
    /// <summary>
    ///     The maximum amount of different fluids that can be registered.
    /// </summary>
    private const int FluidLimit = 32;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const int mPas = 15;

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Fluid>();

    private static readonly List<Fluid> fluidList = new();
    private static readonly Dictionary<string, Fluid> namedFluidDictionary = new();

    /// <summary>
    ///     The absence of a fluid.
    /// </summary>
    public static readonly Fluid None = new NoFluid(Language.NoFluid, nameof(None));

    /// <summary>
    ///     Water is a basic fluid, that allows the player to swim relatively easily.
    /// </summary>
    public static readonly Fluid Water = new BasicFluid(
        Language.Water,
        nameof(Water),
        density: 997f,
        1 * mPas,
        neutralTint: false,
        TextureLayout.Fluid("water_moving_side", "water_moving"),
        TextureLayout.Fluid("water_static_side", "water_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Milk is a white fluid that is obtained from animals.
    /// </summary>
    public static readonly Fluid Milk = new BasicFluid(
        Language.Milk,
        nameof(Milk),
        density: 1033f,
        2 * mPas,
        neutralTint: false,
        TextureLayout.Fluid("milk_moving_side", "milk_moving"),
        TextureLayout.Fluid("milk_static_side", "milk_static"));

    /// <summary>
    ///     Steam is a gas created when water is heated.
    /// </summary>
    public static readonly Fluid Steam = new BasicFluid(
        Language.Steam,
        nameof(Steam),
        density: 0.5f,
        (int) (0.25 * mPas),
        neutralTint: false,
        TextureLayout.Fluid("steam_moving_side", "steam_moving"),
        TextureLayout.Fluid("steam_static_side", "steam_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Lava is a hot fluid, made out of molten stone. It burns flammable objects.
    /// </summary>
    public static readonly Fluid Lava = new HotFluid(
        Language.Lava,
        nameof(Lava),
        density: 3100f,
        15 * mPas,
        neutralTint: false,
        TextureLayout.Fluid("lava_moving_side", "lava_moving"),
        TextureLayout.Fluid("lava_static_side", "lava_static"));

    /// <summary>
    ///     Crude oil is a flammable fluid with a high viscosity. It is lighter than water.
    /// </summary>
    public static readonly Fluid CrudeOil = new BasicFluid(
        Language.CrudeOil,
        nameof(CrudeOil),
        density: 870f,
        8 * mPas,
        neutralTint: false,
        TextureLayout.Fluid("oil_moving_side", "oil_moving"),
        TextureLayout.Fluid("oil_static_side", "oil_static"));

    /// <summary>
    ///     Natural gas is a flammable gas.
    /// </summary>
    public static readonly Fluid NaturalGas = new BasicFluid(
        Language.NaturalGas,
        nameof(NaturalGas),
        density: 0.8f,
        (int) (0.5 * mPas),
        neutralTint: false,
        TextureLayout.Fluid("gas_moving_side", "gas_moving"),
        TextureLayout.Fluid("gas_static_side", "gas_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Concrete is a fluid that hardens when staying still for some time, forming concrete blocks.
    /// </summary>
    public static readonly Fluid Concrete = new ConcreteFluid(
        Language.Concrete,
        nameof(Concrete),
        density: 2400f,
        10 * mPas,
        TextureLayout.Fluid("concrete_moving_side", "concrete_moving"),
        TextureLayout.Fluid("concrete_static_side", "concrete_static"));

    /// <summary>
    ///     Honey is a thick fluid.
    /// </summary>
    public static readonly Fluid Honey = new BasicFluid(
        Language.Honey,
        nameof(Honey),
        density: 1450f,
        20 * mPas,
        neutralTint: false,
        TextureLayout.Fluid("honey_moving_side", "honey_moving"),
        TextureLayout.Fluid("honey_static_side", "honey_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Petrol is a flammable fluid.
    /// </summary>
    public static readonly Fluid Petrol = new BasicFluid(
        Language.Petrol,
        nameof(Petrol),
        density: 740f,
        (int) (0.9 * mPas),
        neutralTint: false,
        TextureLayout.Fluid("petrol_moving_side", "petrol_moving"),
        TextureLayout.Fluid("petrol_static_side", "petrol_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Wine is a reddish fluid.
    /// </summary>
    public static readonly Fluid Wine = new BasicFluid(
        Language.Wine,
        nameof(Wine),
        density: 1090f,
        (int) (1.4 * mPas),
        neutralTint: false,
        TextureLayout.Fluid("wine_moving_side", "wine_moving"),
        TextureLayout.Fluid("wine_static_side", "wine_static"),
        RenderType.Transparent);

    /// <summary>
    ///     Beer is a brown fluid.
    /// </summary>
    public static readonly Fluid Beer = new BasicFluid(
        Language.Beer,
        nameof(Beer),
        density: 1030f,
        (int) (1.5 * mPas),
        neutralTint: false,
        TextureLayout.Fluid("beer_moving_side", "beer_moving"),
        TextureLayout.Fluid("beer_static_side", "beer_static"),
        RenderType.Transparent);

    /// <summary>
    ///     The contact manager instance.
    /// </summary>
    protected static readonly FluidContactManager ContactManager = new();

    /// <summary>
    ///     Gets the count of registered fluids..
    /// </summary>
    public static int Count => fluidList.Count;

    /// <summary>
    ///     Translates a fluid ID to a reference to the fluid that has that ID. If the ID is not valid, none is returned.
    /// </summary>
    /// <param name="id">The ID of the block to return.</param>
    /// <returns>The block with the ID or air if the ID is not valid.</returns>
    public static Fluid TranslateID(uint id)
    {
        if (fluidList.Count > id) return fluidList[(int) id];

        logger.LogWarning(
            Events.UnknownFluid,
            "No Fluid with ID '{ID}' could be found, returning {Fallback} instead",
            id,
            nameof(None));

        return None;
    }

    /// <summary>
    ///     Translate a named ID to the fluid with that ID.
    /// </summary>
    /// <param name="namedId">The named ID to translate.</param>
    /// <returns>The fluid, or null.</returns>
    public static Fluid? TranslateNamedID(string namedId)
    {
        namedFluidDictionary.TryGetValue(namedId, out Fluid? fluid);

        return fluid;
    }

    /// <summary>
    ///     Calls the setup method on all blocks.
    /// </summary>
    public static void LoadFluids(ITextureIndexProvider indexProvider)
    {
        using (logger.BeginScope("Fluid Loading"))
        {
            foreach (Fluid fluid in fluidList)
            {
                fluid.Setup(indexProvider);

                logger.LogDebug(Events.FluidLoad, "Loaded fluid [{Fluid}] with ID '{ID}'", fluid, fluid.Id);
            }

            logger.LogInformation(
                Events.FluidLoad,
                "Fluid setup complete, total of {Count} fluids loaded",
                Count);
        }
    }

    /// <summary>
    ///     Elevate a fluid. This tries to move the fluid up, to the next suitable position.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="pumpDistance">The maximum amount of elevation.</param>
    public static void Elevate(World world, Vector3i position, int pumpDistance)
    {
        Content? content = world.GetContent(position);

        if (content is not var (start, toElevate)) return;
        if (toElevate.Fluid == None || toElevate.Fluid.IsGas) return;

        var currentLevel = (int) toElevate.Level;

        if (start.Block is not IFillable startFillable ||
            !startFillable.AllowOutflow(world, position, BlockSide.Top)) return;

        for (var offset = 1; offset <= pumpDistance && currentLevel > -1; offset++)
        {
            Vector3i elevatedPosition = position + (0, offset, 0);

            var currentBlock = world.GetBlock(elevatedPosition)?.Block as IFillable;

            if (currentBlock == null) break;

            toElevate.Fluid.Fill(
                world,
                elevatedPosition,
                (FluidLevel) currentLevel,
                BlockSide.Bottom,
                out currentLevel);

            if (!currentBlock.AllowOutflow(world, elevatedPosition, BlockSide.Top)) break;
        }

        FluidLevel elevated = toElevate.Level - (currentLevel + 1);
        toElevate.Fluid.Take(world, position, ref elevated);
    }
}


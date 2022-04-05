// <copyright file="LiquidManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Logic.Liquids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Liquid
    {
        /// <summary>
        ///     The maximum amount of different liquids that can be registered.
        /// </summary>
        private const int LiquidLimit = 32;

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private const int mPas = 15;

        private static readonly ILogger logger = LoggingHelper.CreateLogger<Liquid>();

        private static readonly List<Liquid> liquidList = new();
        private static readonly Dictionary<string, Liquid> namedLiquidDictionary = new();

        /// <summary>
        ///     The absence of a liquid.
        /// </summary>
        public static readonly Liquid None = new NoLiquid(Language.NoLiquid, nameof(None));

        /// <summary>
        ///     Water is a basic fluid, that allows the player to swim relatively easily.
        /// </summary>
        public static readonly Liquid Water = new BasicLiquid(
            Language.Water,
            nameof(Water),
            density: 997f,
            1 * mPas,
            neutralTint: false,
            TextureLayout.Liquid("water_moving_side", "water_moving"),
            TextureLayout.Liquid("water_static_side", "water_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Milk is a white liquid that is obtained from animals.
        /// </summary>
        public static readonly Liquid Milk = new BasicLiquid(
            Language.Milk,
            nameof(Milk),
            density: 1033f,
            2 * mPas,
            neutralTint: false,
            TextureLayout.Liquid("milk_moving_side", "milk_moving"),
            TextureLayout.Liquid("milk_static_side", "milk_static"));

        /// <summary>
        ///     Steam is a gas created when water is heated.
        /// </summary>
        public static readonly Liquid Steam = new BasicLiquid(
            Language.Steam,
            nameof(Steam),
            density: 0.5f,
            (int) (0.25 * mPas),
            neutralTint: false,
            TextureLayout.Liquid("steam_moving_side", "steam_moving"),
            TextureLayout.Liquid("steam_static_side", "steam_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Lava is a hot liquid, made out of molten stone. It burns flammable objects.
        /// </summary>
        public static readonly Liquid Lava = new HotLiquid(
            Language.Lava,
            nameof(Lava),
            density: 3100f,
            15 * mPas,
            neutralTint: false,
            TextureLayout.Liquid("lava_moving_side", "lava_moving"),
            TextureLayout.Liquid("lava_static_side", "lava_static"));

        /// <summary>
        ///     Crude oil is a flammable liquid with a high viscosity. It is lighter than water.
        /// </summary>
        public static readonly Liquid CrudeOil = new BasicLiquid(
            Language.CrudeOil,
            nameof(CrudeOil),
            density: 870f,
            8 * mPas,
            neutralTint: false,
            TextureLayout.Liquid("oil_moving_side", "oil_moving"),
            TextureLayout.Liquid("oil_static_side", "oil_static"));

        /// <summary>
        ///     Natural gas is a flammable gas.
        /// </summary>
        public static readonly Liquid NaturalGas = new BasicLiquid(
            Language.NaturalGas,
            nameof(NaturalGas),
            density: 0.8f,
            (int) (0.5 * mPas),
            neutralTint: false,
            TextureLayout.Liquid("gas_moving_side", "gas_moving"),
            TextureLayout.Liquid("gas_static_side", "gas_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Concrete is a liquid that hardens when staying still for some time, forming concrete blocks.
        /// </summary>
        public static readonly Liquid Concrete = new ConcreteLiquid(
            Language.Concrete,
            nameof(Concrete),
            density: 2400f,
            10 * mPas,
            TextureLayout.Liquid("concrete_moving_side", "concrete_moving"),
            TextureLayout.Liquid("concrete_static_side", "concrete_static"));

        /// <summary>
        ///     Honey is a thick liquid.
        /// </summary>
        public static readonly Liquid Honey = new BasicLiquid(
            Language.Honey,
            nameof(Honey),
            density: 1450f,
            20 * mPas,
            neutralTint: false,
            TextureLayout.Liquid("honey_moving_side", "honey_moving"),
            TextureLayout.Liquid("honey_static_side", "honey_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Petrol is a flammable liquid.
        /// </summary>
        public static readonly Liquid Petrol = new BasicLiquid(
            Language.Petrol,
            nameof(Petrol),
            density: 740f,
            (int) (0.9 * mPas),
            neutralTint: false,
            TextureLayout.Liquid("petrol_moving_side", "petrol_moving"),
            TextureLayout.Liquid("petrol_static_side", "petrol_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Wine is a reddish liquid.
        /// </summary>
        public static readonly Liquid Wine = new BasicLiquid(
            Language.Wine,
            nameof(Wine),
            density: 1090f,
            (int) (1.4 * mPas),
            neutralTint: false,
            TextureLayout.Liquid("wine_moving_side", "wine_moving"),
            TextureLayout.Liquid("wine_static_side", "wine_static"),
            RenderType.Transparent);

        /// <summary>
        ///     Beer is a brown liquid.
        /// </summary>
        public static readonly Liquid Beer = new BasicLiquid(
            Language.Beer,
            nameof(Beer),
            density: 1030f,
            (int) (1.5 * mPas),
            neutralTint: false,
            TextureLayout.Liquid("beer_moving_side", "beer_moving"),
            TextureLayout.Liquid("beer_static_side", "beer_static"),
            RenderType.Transparent);

        /// <summary>
        ///     The contact manager instance.
        /// </summary>
        protected static readonly LiquidContactManager ContactManager = new();

        /// <summary>
        ///     Gets the count of registered liquids..
        /// </summary>
        public static int Count => liquidList.Count;

        /// <summary>
        ///     Translates a liquid ID to a reference to the liquid that has that ID. If the ID is not valid, none is returned.
        /// </summary>
        /// <param name="id">The ID of the block to return.</param>
        /// <returns>The block with the ID or air if the ID is not valid.</returns>
        public static Liquid TranslateID(uint id)
        {
            if (liquidList.Count > id) return liquidList[(int) id];

            logger.LogWarning(
                Events.UnknownLiquid,
                "No Liquid with ID '{ID}' could be found, returning {Fallback} instead",
                id,
                nameof(None));

            return None;
        }

        /// <summary>
        ///     Translate a named ID to the liquid with that ID.
        /// </summary>
        /// <param name="namedId">The named ID to translate.</param>
        /// <returns>The liquid, or null.</returns>
        public static Liquid? TranslateNamedID(string namedId)
        {
            namedLiquidDictionary.TryGetValue(namedId, out Liquid? liquid);

            return liquid;
        }

        /// <summary>
        ///     Calls the setup method on all blocks.
        /// </summary>
        public static void LoadLiquids(ITextureIndexProvider indexProvider)
        {
            using (logger.BeginScope("Liquid Loading"))
            {
                foreach (Liquid liquid in liquidList)
                {
                    liquid.Setup(indexProvider);

                    logger.LogDebug(Events.LiquidLoad, "Loaded liquid [{Liquid}] with ID '{ID}'", liquid, liquid.Id);
                }

                logger.LogInformation(
                    Events.LiquidLoad,
                    "Liquid setup complete, total of {Count} liquids loaded",
                    Count);
            }
        }

        /// <summary>
        ///     Elevate a liquid. This tries to move the liquid up, to the next suitable position.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="position">The position of the liquid.</param>
        /// <param name="pumpDistance">The maximum amount of elevation.</param>
        public static void Elevate(World world, Vector3i position, int pumpDistance)
        {
            (BlockInstance, LiquidInstance)? content = world.GetContent(position);

            if (content is not ({} start, {} toElevate)) return;
            if (toElevate.Liquid == None || toElevate.Liquid.IsGas) return;

            var currentLevel = (int) toElevate.Level;

            if (start.Block is not IFillable startFillable ||
                !startFillable.AllowOutflow(world, position, BlockSide.Top)) return;

            for (var offset = 1; offset <= pumpDistance && currentLevel > -1; offset++)
            {
                Vector3i elevatedPosition = position + (0, offset, 0);

                var currentBlock = world.GetBlock(elevatedPosition)?.Block as IFillable;

                if (currentBlock == null) break;

                toElevate.Liquid.Fill(
                    world,
                    elevatedPosition,
                    (LiquidLevel) currentLevel,
                    BlockSide.Bottom,
                    out currentLevel);

                if (!currentBlock.AllowOutflow(world, elevatedPosition, BlockSide.Top)) break;
            }

            LiquidLevel elevated = toElevate.Level - (currentLevel + 1);
            toElevate.Liquid.Take(world, position, ref elevated);
        }
    }
}

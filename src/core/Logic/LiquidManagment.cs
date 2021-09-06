// <copyright file="LiquidManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Logic.Liquids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Liquid
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Liquid>();

        public const int LiquidLimit = 32;

        private static readonly List<Liquid> liquidList = new List<Liquid>();
        private static readonly Dictionary<string, Liquid> namedLiquidDictionary = new Dictionary<string, Liquid>();

        private const int mPas = 15;

        public static readonly Liquid None = new NoLiquid(Language.NoLiquid, nameof(None));

        public static readonly Liquid Water = new BasicLiquid(
            Language.Water,
            nameof(Water),
            997f,
            1 * mPas,
            false,
            TextureLayout.Liquid("water_moving_side", "water_moving"),
            TextureLayout.Liquid("water_static_side", "water_static"),
            RenderType.Transparent);

        public static readonly Liquid Milk = new BasicLiquid(
            Language.Milk,
            nameof(Milk),
            1033f,
            2 * mPas,
            false,
            TextureLayout.Liquid("milk_moving_side", "milk_moving"),
            TextureLayout.Liquid("milk_static_side", "milk_static"));

        public static readonly Liquid Steam = new BasicLiquid(
            Language.Steam,
            nameof(Steam),
            -0.015f,
            (int) (0.25 * mPas),
            false,
            TextureLayout.Liquid("steam_moving_side", "steam_moving"),
            TextureLayout.Liquid("steam_static_side", "steam_static"),
            RenderType.Transparent);

        public static readonly Liquid Lava = new HotLiquid(
            Language.Lava,
            nameof(Lava),
            3100f,
            15 * mPas,
            false,
            TextureLayout.Liquid("lava_moving_side", "lava_moving"),
            TextureLayout.Liquid("lava_static_side", "lava_static"));

        public static readonly Liquid CrudeOil = new BasicLiquid(
            Language.CrudeOil,
            nameof(CrudeOil),
            870f,
            8 * mPas,
            false,
            TextureLayout.Liquid("oil_moving_side", "oil_moving"),
            TextureLayout.Liquid("oil_static_side", "oil_static"));

        public static readonly Liquid NaturalGas = new BasicLiquid(
            Language.NaturalGas,
            nameof(NaturalGas),
            -0.8f,
            (int) (0.5 * mPas),
            false,
            TextureLayout.Liquid("gas_moving_side", "gas_moving"),
            TextureLayout.Liquid("gas_static_side", "gas_static"),
            RenderType.Transparent);

        public static readonly Liquid Concrete = new ConcreteLiquid(
            Language.Concrete,
            nameof(Concrete),
            2400f,
            10 * mPas,
            TextureLayout.Liquid("concrete_moving_side", "concrete_moving"),
            TextureLayout.Liquid("concrete_static_side", "concrete_static"));

        public static readonly Liquid Honey = new BasicLiquid(
            Language.Honey,
            nameof(Honey),
            1450f,
            20 * mPas,
            false,
            TextureLayout.Liquid("honey_moving_side", "honey_moving"),
            TextureLayout.Liquid("honey_static_side", "honey_static"),
            RenderType.Transparent);

        public static readonly Liquid Petrol = new BasicLiquid(
            Language.Petrol,
            nameof(Petrol),
            740f,
            (int) (0.9 * mPas),
            false,
            TextureLayout.Liquid("petrol_moving_side", "petrol_moving"),
            TextureLayout.Liquid("petrol_static_side", "petrol_static"),
            RenderType.Transparent);

        public static readonly Liquid Wine = new BasicLiquid(
            Language.Wine,
            nameof(Wine),
            1090f,
            (int) (1.4 * mPas),
            false,
            TextureLayout.Liquid("wine_moving_side", "wine_moving"),
            TextureLayout.Liquid("wine_static_side", "wine_static"),
            RenderType.Transparent);

        public static readonly Liquid Beer = new BasicLiquid(
            Language.Beer,
            nameof(Beer),
            1030f,
            (int) (1.5 * mPas),
            false,
            TextureLayout.Liquid("beer_moving_side", "beer_moving"),
            TextureLayout.Liquid("beer_static_side", "beer_static"),
            RenderType.Transparent);

        protected static readonly LiquidContactManager ContactManager = new LiquidContactManager();

        /// <summary>
        /// Translates a liquid ID to a reference to the liquid that has that ID. If the ID is not valid, none is returned.
        /// </summary>
        /// <param name="id">The ID of the block to return.</param>
        /// <returns>The block with the ID or air if the ID is not valid.</returns>
        public static Liquid TranslateID(uint id)
        {
            if (liquidList.Count > id)
            {
                return liquidList[(int) id];
            }
            else
            {
                logger.LogWarning(
                    "No Liquid with the ID {id} could be found, returning {fallback} instead.",
                    id,
                    nameof(Liquid.None));

                return Liquid.None;
            }
        }

        public static Liquid TranslateNamedID(string namedId)
        {
            if (namedLiquidDictionary.TryGetValue(namedId, out Liquid? liquid))
            {
                return liquid;
            }
            else
            {
                logger.LogWarning(
                    "No Liquid with the named ID {id} could be found, returning {fallback} instead.",
                    namedId,
                    nameof(Liquid.None));

                return Liquid.None;
            }
        }

        /// <summary>
        /// Gets the count of registered liquids..
        /// </summary>
        public static int Count => liquidList.Count;

        /// <summary>
        /// Calls the setup method on all blocks.
        /// </summary>
        public static void LoadLiquids(ITextureIndexProvider indexProvider)
        {
            using (logger.BeginScope("Liquid Loading"))
            {
                foreach (Liquid liquid in liquidList)
                {
                    liquid.Setup(indexProvider);

                    logger.LogDebug(Events.LiquidLoad, "Loaded the liquid [{liquid}] with ID {id}.", liquid, liquid.Id);
                }

                logger.LogInformation("Liquid setup complete. A total of {count} liquids have been loaded.", Count);
            }
        }

        public static void Elevate(World world, int x, int y, int z, int pumpDistance)
        {
            (Block? start, Liquid? toElevate) =
                world.GetPosition(x, y, z, out _, out LiquidLevel initialLevel, out _);

            if (start == null || toElevate == null) return;

            if (toElevate == Liquid.None || toElevate.IsGas) return;

            var currentLevel = (int) initialLevel;

            if (!(start is IFillable startFillable) ||
                !startFillable.AllowOutflow(world, x, y, z, BlockSide.Top)) return;

            for (var offset = 1; offset <= pumpDistance && currentLevel > -1; offset++)
            {
                int currentY = y + offset;

                var currentBlock = world.GetBlock(x, currentY, z, out _) as IFillable;

                if (currentBlock?.AllowInflow(world, x, currentY, z, BlockSide.Bottom, toElevate) != true) break;

                toElevate.Fill(world, x, currentY, z, (LiquidLevel) currentLevel, out currentLevel);

                if (!currentBlock.AllowOutflow(world, x, currentY, z, BlockSide.Top)) break;
            }

            LiquidLevel elevated = initialLevel - (currentLevel + 1);
            toElevate.Take(world, x, y, z, ref elevated);
        }
    }
}
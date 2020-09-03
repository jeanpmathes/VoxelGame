// <copyright file="LiquidManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VoxelGame.Logic.Liquids;
using VoxelGame.Resources.Language;

namespace VoxelGame.Logic
{
    public abstract partial class Liquid
    {
        private static readonly ILogger logger = Program.CreateLogger<Liquid>();

        public const int LiquidLimit = 32;

        private static readonly Dictionary<uint, Liquid> liquidDictionary = new Dictionary<uint, Liquid>();
        private static readonly Dictionary<string, Liquid> namedLiquidDictionary = new Dictionary<string, Liquid>();

        public static readonly Liquid None = new NoLiquid(Language.NoLiquid, nameof(None));
        public static readonly Liquid Water = new BasicLiquid(Language.Water, nameof(Water), 1f, 15, true, TextureLayout.Liquid("water_moving_side", "water_moving"), TextureLayout.Liquid("water_static_side", "water_static"));

        /// <summary>
        /// Translates a liquid ID to a reference to the liquid that has that ID. If the ID is not valid, none is returned.
        /// </summary>
        /// <param name="id">The ID of the block to return.</param>
        /// <returns>The block with the ID or air if the ID is not valid.</returns>
        public static Liquid TranslateID(uint id)
        {
            if (liquidDictionary.TryGetValue(id, out Liquid? liquid))
            {
                return liquid;
            }
            else
            {
                logger.LogWarning("No Liquid with the ID {id} could be found, returning {fallback} instead.", id, nameof(Liquid.None));

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
                logger.LogWarning("No Liquid with the named ID {id} could be found, returning {fallback} instead.", namedId, nameof(Liquid.None));

                return Liquid.None;
            }
        }

        /// <summary>
        /// Gets the count of registered liquids..
        /// </summary>
        public static int Count { get => liquidDictionary.Count; }

        /// <summary>
        /// Calls the setup method on all blocks.
        /// </summary>
        public static void LoadLiquids()
        {
            using (logger.BeginScope("Liquid Loading"))
            {
                foreach (Liquid liquid in liquidDictionary.Values)
                {
                    liquid.Setup();

                    logger.LogDebug(LoggingEvents.LiquidLoad, "Loaded the liquid [{liquid}] with ID {id}.", liquid, liquid.Id);
                }

                logger.LogInformation("Liquid setup complete. A total of {count} liquids have been loaded.", Count);
            }
        }
    }
}
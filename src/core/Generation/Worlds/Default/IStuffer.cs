﻿// <copyright file="IStuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Stuffer is used when a local negative offset is applied to the terrain.
///     The stuffer stuffs the space between the global height and the local height.
/// </summary>
public interface IStuffer
{
    /// <summary>
    ///     Get the content of this stuffer for a position.
    /// </summary>
    /// <param name="temperature">The temperature of the position.</param>
    /// <returns>The content of the stuffer.</returns>
    public Content GetContent(Temperature temperature);

    /// <summary>
    /// Simply stuffs with ice.
    /// </summary>
    public sealed class Ice : IStuffer
    {
        private readonly Content content = new(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

        /// <inheritdoc />
        public Content GetContent(Temperature temperature)
        {
            return content;
        }
    }

    /// <summary>
    ///     Stuffs with water, or ice if the temperature is low.
    /// </summary>
    public sealed class Water : IStuffer
    {
        private readonly Content water = new(BlockInstance.Default, Fluids.Instance.FreshWater.AsInstance());
        private readonly Content ice = new(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

        /// <inheritdoc />
        public Content GetContent(Temperature temperature)
        {
            return temperature.IsFreezing ? ice : water;
        }
    }
}

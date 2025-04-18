// <copyright file="Stuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Stuffer is used when a local negative offset is applied to the terrain.
///     The space between the global height and the local height is stuffed by the stuffer.
/// </summary>
public abstract class Stuffer
{
    /// <summary>
    ///     Get the content of this stuffer for a position.
    /// </summary>
    /// <param name="temperature">The temperature of the position.</param>
    /// <returns>The content of the stuffer.</returns>
    public abstract Content GetContent(Temperature temperature);

    /// <summary>
    /// Simply stuffs with ice.
    /// </summary>
    public sealed class Ice : Stuffer
    {
        private readonly Content content = new(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

        /// <inheritdoc />
        public override Content GetContent(Temperature temperature)
        {
            return content;
        }
    }

    /// <summary>
    ///     Stuffs with water, or ice if temperature is low.
    /// </summary>
    public sealed class Water : Stuffer
    {
        private readonly Content water = new(BlockInstance.Default, Fluids.Instance.FreshWater.AsInstance());
        private readonly Content ice = new(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

        /// <inheritdoc />
        public override Content GetContent(Temperature temperature)
        {
            return temperature.IsFreezing ? ice : water;
        }
    }
}

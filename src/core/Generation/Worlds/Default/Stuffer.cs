// <copyright file="Stuffer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     Stuffer is used when a local negative offset is applied to the terrain.
///     The space between the global height and the local height is stuffed by the stuffer.
/// </summary>
public abstract class Stuffer
{
    /// <summary>
    ///     Get the content for a given block.
    /// </summary>
    /// <returns>The content of the stuffer.</returns>
    public abstract Content GetContent();

    /// <summary>
    /// </summary>
    public sealed class Ice : Stuffer
    {
        private readonly Content content = new(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

        /// <inheritdoc />
        public override Content GetContent()
        {
            return content;
        }
    }
}

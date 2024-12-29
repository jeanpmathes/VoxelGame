// <copyright file="UserInterfaceResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.Platform;
using VoxelGame.UI.Resources;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     Combines all resources the user interface declares.
/// </summary>
public record UserInterfaceResources(IGwenGui GUI, FontBundle Fonts, Skin DefaultSkin, Skin AlternativeSkin, IReadOnlyList<Attribution> Attributions)
{
    /// <summary>
    ///     Retrieve the resources from a resource context.
    /// </summary>
    /// <param name="context">The context to retrieve the resources from.</param>
    /// <returns>The resources or <c>null</c> if they could not be retrieved.</returns>
    public static UserInterfaceResources? Retrieve(IResourceContext context)
    {
        var gui = context.Get<IGwenGui>();
        var fonts = context.Get<FontBundle>();

        if (gui == null || fonts == null)
            return null;

        var defaultSkin = context.Get<Skin>(VGuiLoader.DefaultSkin);
        var alternativeSkin = context.Get<Skin>(VGuiLoader.AlternativeSkin);

        if (defaultSkin == null || alternativeSkin == null)
            return null;

        IReadOnlyList<Attribution> attributions = context.GetAll<Attribution>().ToList();

        return new UserInterfaceResources(gui, fonts, defaultSkin, alternativeSkin, attributions);
    }
}

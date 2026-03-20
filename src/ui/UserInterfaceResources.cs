// <copyright file="UserInterfaceResources.cs" company="VoxelGame">
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

using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Presentation.Platform;
using VoxelGame.Presentation.Resources;
using VoxelGame.Presentation.Utilities;

namespace VoxelGame.Presentation;

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
        IGwenGui? gui = context.Get<IGwenGui>();
        FontBundle? fonts = context.Get<FontBundle>();

        if (gui == null || fonts == null)
            return null;

        Skin? defaultSkin = context.Get<Skin>(GameGuiLoader.DefaultSkin);
        Skin? alternativeSkin = context.Get<Skin>(GameGuiLoader.AlternativeSkin);

        if (defaultSkin == null || alternativeSkin == null)
            return null;

        IReadOnlyList<Attribution> attributions = context.GetAll<Attribution>().ToList();

        return new UserInterfaceResources(gui, fonts, defaultSkin, alternativeSkin, attributions);
    }
}

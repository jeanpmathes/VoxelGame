// <copyright file="ClientResources.cs" company="VoxelGame">
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

using VoxelGame.Client.Console;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Resources;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.Resources;

namespace VoxelGame.Client.Resources;

/// <summary>
///     The content (resources) of the client.
/// </summary>
public class ClientContent : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the client resources catalog.
    /// </summary>
    public ClientContent() : base([
        new Textures(),
        new EngineLoader(),
        new Models(),
        new CoreContent(),
        new PlayerContent(),
        new CommandLoader(),
        new UserInterfaceContent()
    ]) {}
}

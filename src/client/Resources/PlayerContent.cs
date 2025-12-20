// <copyright file="Players.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using System.IO;
using VoxelGame.Client.Visuals;
using VoxelGame.Client.Visuals.Textures;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Resources;

/// <summary>
///     All player-associated resources.
/// </summary>
public sealed class PlayerContent : ResourceCatalog
{
    private static readonly FileInfo crosshairPath = FileSystem.GetResourceDirectory("Textures", "UI").GetFile("crosshair.png");

    /// <summary>
    ///     Creates the resource catalog.
    /// </summary>
    public PlayerContent() : base([
        new SingleTextureLoader(crosshairPath, fallbackResolution: 32),
        new Linker()
    ]) {}

    private sealed class Linker : IResourceLinker
    {
        public void Link(IResourceContext context)
        {
            context.Require<Engine>(engine =>
                context.Require<SingleTexture>(RID.Path(crosshairPath),
                    crosshair =>
                    {
                        engine.CrosshairPipeline.SetTexture(crosshair.Texture);

                        return [];
                    }));
        }
    }
}

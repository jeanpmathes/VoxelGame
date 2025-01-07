// <copyright file="Players.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
                        engine.CrosshairVFX.SetTexture(crosshair.Texture);

                        return [];
                    }));
        }
    }
}

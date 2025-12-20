// <copyright file="GuiLoader.cs" company="VoxelGame">
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.UI.Platform;
using VoxelGame.UI.Resources;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     Loads the VoxelGame <see cref="GameGui" /> object.
/// </summary>
public sealed class GameGuiLoader : IResourceLoader
{
    /// <summary>
    ///     The default skin identifier.
    /// </summary>
    public static readonly RID DefaultSkin = RID.Named<Skin>("Default");

    /// <summary>
    ///     The alternative skin identifier.
    /// </summary>
    public static readonly RID AlternativeSkin = RID.Named<Skin>("Alternative");

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Client>(client =>
        {
            FileInfo skin1 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin1.png");
            FileInfo skin2 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin2.png");
            FileInfo shader = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");

            List<FileInfo> skinFiles = [skin1, skin2];
            Dictionary<FileInfo, Exception> skinLoadingErrors = new();

            String? shaderLoadingError = null;

            Dictionary<String, TexturePreload> textures = GetTexturePreloads();
            Dictionary<String, Exception?> textureLoadingErrors = new();

            List<IResource> resources = [];

            IGwenGui gui = GwenGuiFactory.CreateFromClient(
                client,
                GwenGuiSettings.Default.From(settings =>
                {
                    settings.SkinFiles = skinFiles;
                    settings.SkinLoadingErrorCallback = (file, exception) => skinLoadingErrors[file] = exception;

                    settings.SkinLoadedCallback = (index, skin) =>
                    {
                        if (index == 0) resources.Add(new Skin(DefaultSkin, skin));
                        else if (index == 1) resources.Add(new Skin(AlternativeSkin, skin));
                    };

                    settings.ShaderFile = shader;

                    settings.ShaderLoadingErrorCallback =
                        exception =>
                        {
                            shaderLoadingError = exception;
                            Debugger.Break();
                        };

                    foreach ((String _, TexturePreload texture) in textures)
                        settings.TexturePreloads.Add(texture);

                    settings.TexturePreloadErrorCallback = (texture, exception) => textureLoadingErrors[texture.Name] = exception;
                }));

            gui.Load();

            ReportSkinLoading(skinFiles, skinLoadingErrors, context);
            ReportTextureLoading(textures, textureLoadingErrors, context);
            IResource? missing = ReportShaderLoading(shaderLoadingError, shader, context);

            resources.Add(missing ?? gui);

            Modals.SetUpLanguage();

            return resources;
        });
    }

    private static FileInfo GetImageFile(String name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Images").GetFile($"{name}.png");
    }

    private static FileInfo GetIconFile(String name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Icons").GetFile($"{name}.png");
    }

    private static Dictionary<String, TexturePreload> GetTexturePreloads()
    {
        Dictionary<String, TexturePreload> textures = new();

        foreach (String name in Icons.Instance.IconNames) textures[name] = new TexturePreload(GetIconFile(name), name);

        foreach (String name in Icons.Instance.ImageNames) textures[name] = new TexturePreload(GetImageFile(name), name);

        return textures;
    }

    private static MissingResource? ReportShaderLoading(String? shaderLoadingError, FileSystemInfo shader, IResourceContext context)
    {
        context.ReportDiscovery(ResourceTypes.Shader, RID.Path(shader), errorMessage: shaderLoadingError);

        return shaderLoadingError != null
            ? new MissingResource(ResourceTypes.Shader, RID.Path(shader), ResourceIssue.FromMessage(Level.Error, shaderLoadingError))
            : null;
    }

    private static void ReportSkinLoading(List<FileInfo> skinFiles, IReadOnlyDictionary<FileInfo, Exception> skinLoadingErrors, IResourceContext context)
    {
        foreach (FileInfo skinFile in skinFiles)
            context.ReportDiscovery(ResourceTypes.Texture, RID.Path(skinFile), skinLoadingErrors.GetValueOrDefault(skinFile));
    }

    private static void ReportTextureLoading(Dictionary<String, TexturePreload> textures, IReadOnlyDictionary<String, Exception?> textureLoadingErrors, IResourceContext context)
    {
        foreach ((String name, TexturePreload texture) in textures)
            context.ReportDiscovery(ResourceTypes.Texture, RID.Path(texture.File), textureLoadingErrors.GetValueOrDefault(name));
    }
}

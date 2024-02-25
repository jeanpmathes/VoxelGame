// <copyright file="GwenGuiSettings.cs" company="Gwen.Net">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>Gwen.Net, jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using Gwen.Net.Skin;

namespace VoxelGame.UI.Platform;

/// <summary>
///     Describes a texture to preload.
/// </summary>
public record TexturePreload(FileInfo File, string Name);

/// <summary>
///     The settings for the gwen gui.
/// </summary>
public class GwenGuiSettings
{
    /// <summary>
    ///     Default settings.
    /// </summary>
    public static readonly GwenGuiSettings Default = new();

    private GwenGuiSettings() {}

    /// <summary>
    ///     The skin files to load. Must contain at least one file.
    /// </summary>
    public IEnumerable<FileInfo> SkinFiles { get; set; } = new[] {new FileInfo("DefaultSkin.png")};

    /// <summary>
    ///     The error callback for the skin loading.
    /// </summary>
    public Action<FileInfo, Exception> SkinLoadingErrorCallback { get; set; } = (_, e) => throw e;

    /// <summary>
    ///     The callback for when a skin is loaded.
    /// </summary>
    public Action<int, SkinBase> SkinLoadedCallback { get; set; } = (_, _) => {};

    /// <summary>
    ///     List of textures to preload.
    /// </summary>
    public ICollection<TexturePreload> TexturePreloads { get; } = new List<TexturePreload>();

    /// <summary>
    ///     Callback for texture preloading errors.
    /// </summary>
    public Action<TexturePreload, Exception> TexturePreloadErrorCallback { get; set; } = (_, e) => throw e;

    /// <summary>
    ///     The shader file to load.
    /// </summary>
    public FileInfo ShaderFile { get; set; } = new("GUI.shader");

    /// <summary>
    ///     The error callback for the shader loading.
    /// </summary>
    public Action<string> ShaderLoadingErrorCallback { get; set; } = e => throw new InvalidOperationException(e);

    /// <summary>
    ///     Apply a modifier to the settings.
    /// </summary>
    public GwenGuiSettings From(Action<GwenGuiSettings> settingsModifier)
    {
        settingsModifier.Invoke(this);

        return this;
    }
}

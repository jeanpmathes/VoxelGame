﻿// <copyright file="UIResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gwen.Net.RichText;
using Gwen.Net.Skin;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.UI.Platform;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     All resources used by the UI, such as images and icons.
/// </summary>
public sealed class UIResources : IDisposable
{
    private static readonly List<string> iconNames = [];
    private static readonly List<string> imageNames = [];
    private readonly List<Attribution> attributions = [];

    internal string ResetIcon { get; } = GetIcon("reset");
    internal string LoadIcon { get; } = GetIcon("load");
    internal string DeleteIcon { get; } = GetIcon("delete");
    internal string WarningIcon { get; } = GetIcon("warning");
    internal string ErrorIcon { get; } = GetIcon("error");
    internal string InfoIcon { get; } = GetIcon("info");
    internal string RenameIcon { get; } = GetIcon("rename");
    internal string SearchIcon { get; } = GetIcon("search");
    internal string ClearIcon { get; } = GetIcon("clear");
    internal string DuplicateIcon { get; } = GetIcon("duplicate");
    internal string StarFilledIcon { get; } = GetIcon("star_filled");
    internal string StarEmptyIcon { get; } = GetIcon("star_empty");

    internal string StartImage { get; } = GetImage("start");

    internal SkinBase DefaultSkin { get; private set; } = null!;
    internal SkinBase AlternativeSkin { get; private set; } = null!;

    internal IGwenGui GUI { get; private set; } = null!;

    internal FontHolder Fonts { get; private set; } = null!;

    private static string GetIcon(string name)
    {
        iconNames.Add(name);

        return name;
    }

    private static string GetImage(string name)
    {
        imageNames.Add(name);

        return name;
    }

    private void LoadAttributions(LoadingContext loadingContext)
    {
        DirectoryInfo directory = FileSystem.GetResourceDirectory("Attribution");

        if (!directory.Exists)
        {
            loadingContext.ReportWarning(Events.MissingDepository, nameof(Attribution), directory, "Directory does not exist");

            return;
        }

        foreach (FileInfo file in directory.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            string name = file.GetFileNameWithoutExtension().Replace(oldChar: '-', newChar: ' ');

            string? text = null;

            try
            {
                text = file.ReadAllText();
            }
            catch (IOException exception)
            {
                loadingContext.ReportWarning(Events.ResourceLoad, nameof(Attribution), file, exception);
            }

            if (text == null) continue;

            attributions.Add(new Attribution(name, text));
            loadingContext.ReportSuccess(Events.ResourceLoad, nameof(Attribution), file);
        }
    }

    private static Dictionary<string, TexturePreload> GetTexturePreloads()
    {
        Dictionary<string, TexturePreload> textures = new();

        foreach (string name in iconNames) textures[name] = new TexturePreload(GetIconName(name), name);

        foreach (string name in imageNames) textures[name] = new TexturePreload(GetImageName(name), name);

        return textures;
    }

    private void LoadGUI(Client window, LoadingContext loadingContext)
    {
        FileInfo skin1 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin1.png");
        FileInfo skin2 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin2.png");
        FileInfo shader = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");

        List<FileInfo> skinFiles = [skin1, skin2];
        Dictionary<FileInfo, Exception> skinLoadingErrors = new();

        string? shaderLoadingError = null;

        Dictionary<string, TexturePreload> textures = GetTexturePreloads();
        Dictionary<string, Exception?> textureLoadingErrors = new();

        GUI = GwenGuiFactory.CreateFromClient(
            window,
            GwenGuiSettings.Default.From(
                settings =>
                {
                    settings.SkinFiles = skinFiles;
                    settings.SkinLoadingErrorCallback = (file, exception) => skinLoadingErrors[file] = exception;

                    settings.SkinLoadedCallback = (index, skin) =>
                    {
                        if (index == 0) DefaultSkin = skin;
                        else if (index == 1) AlternativeSkin = skin;
                    };

                    settings.ShaderFile = shader;

                    settings.ShaderLoadingErrorCallback =
                        exception =>
                        {
                            shaderLoadingError = exception;
                            Debugger.Break();
                        };

                    foreach ((string _, TexturePreload texture) in textures) settings.TexturePreloads.Add(texture);

                    settings.TexturePreloadErrorCallback = (texture, exception) => textureLoadingErrors[texture.Name] = exception;
                }));

        GUI.Load();

        foreach (FileInfo skinFile in skinFiles) ReportSkinLoading(skinLoadingErrors.GetValueOrDefault(skinFile), skinFile, loadingContext);

        ReportTextureLoading(textures, textureLoadingErrors, loadingContext);
        ReportShaderLoading(shaderLoadingError, shader, loadingContext);

        Modals.SetupLanguage();

        Fonts = new FontHolder(GUI.Root.Skin);
    }

    private static void ReportSkinLoading(Exception? skinLoadingError, FileSystemInfo skinFile, LoadingContext loadingContext)
    {
        if (skinLoadingError != null)
            loadingContext.ReportWarning(Events.ResourceLoad, nameof(GUI), skinFile, skinLoadingError);
        else
            loadingContext.ReportSuccess(Events.ResourceLoad, nameof(GUI), skinFile);
    }

    private static void ReportShaderLoading(string? shaderLoadingError, FileSystemInfo shader, LoadingContext loadingContext)
    {
        const string type = "Shader";

        if (shaderLoadingError != null)
            loadingContext.ReportFailure(Events.ResourceLoad, type, shader, shaderLoadingError, abort: true);
        else
            loadingContext.ReportSuccess(Events.ResourceLoad, type, shader);
    }

    private static void ReportTextureLoading(Dictionary<string, TexturePreload> textures, IReadOnlyDictionary<string, Exception?> textureLoadingErrors, LoadingContext loadingContext)
    {
        foreach ((string name, TexturePreload texture) in textures)
        {
            Exception? error = textureLoadingErrors.GetValueOrDefault(name);

            if (error != null)
                loadingContext.ReportWarning(Events.ResourceLoad, nameof(GUI), texture.File, error);
            else
                loadingContext.ReportSuccess(Events.ResourceLoad, nameof(GUI), texture.File);
        }
    }

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load(Client window, LoadingContext loadingContext)
    {
        Throw.IfDisposed(disposed);

        using (loadingContext.BeginStep(Events.ResourceLoad, "UI"))
        {
            LoadAttributions(loadingContext);
            LoadGUI(window, loadingContext);
        }
    }

    private static (Document document, string name) CreateAttribution(Attribution attribution, Context context)
    {
        Document credits = new();

        Paragraph paragraph = new Paragraph()
            .Font(context.Fonts.Title).Text(attribution.Name).LineBreak().LineBreak()
            .Font(context.Fonts.Default)
            .Text(attribution.Text).LineBreak();

        credits.Paragraphs.Add(paragraph);

        return (credits, attribution.Name);
    }

    /// <summary>
    ///     Create the attribution documents.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <returns>The documents and their names.</returns>
    internal IEnumerable<(Document document, string name)> CreateAttributions(Context context)
    {
        Throw.IfDisposed(disposed);

        return attributions.Select(attribution => CreateAttribution(attribution, context));
    }

    private static FileInfo GetImageName(string name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Images").GetFile($"{name}.png");
    }

    private static FileInfo GetIconName(string name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Icons").GetFile($"{name}.png");
    }

    private sealed record Attribution(string Name, string Text);

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;
        if (!disposing) return;

        Fonts.Dispose();
        GUI.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~UIResources()
    {
        Dispose(disposing: false);
    }

    #endregion IDisposable Support
}

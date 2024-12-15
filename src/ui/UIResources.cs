// <copyright file="UIResources.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.Platform;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     All resources used by the UI, such as images and icons.
/// </summary>
public sealed class UIResources : IDisposable
{
    private static readonly List<String> iconNames = [];
    private static readonly List<String> imageNames = [];
    private readonly List<Attribution> attributions = [];

    internal String ResetIcon { get; } = GetIcon("reset");
    internal String LoadIcon { get; } = GetIcon("load");
    internal String DeleteIcon { get; } = GetIcon("delete");
    internal String WarningIcon { get; } = GetIcon("warning");
    internal String ErrorIcon { get; } = GetIcon("error");
    internal String InfoIcon { get; } = GetIcon("info");
    internal String RenameIcon { get; } = GetIcon("rename");
    internal String SearchIcon { get; } = GetIcon("search");
    internal String ClearIcon { get; } = GetIcon("clear");
    internal String DuplicateIcon { get; } = GetIcon("duplicate");
    internal String StarFilledIcon { get; } = GetIcon("star_filled");
    internal String StarEmptyIcon { get; } = GetIcon("star_empty");

    internal String StartImage { get; } = GetImage("start");

    internal SkinBase DefaultSkin { get; private set; } = null!;
    internal SkinBase AlternativeSkin { get; private set; } = null!;

    internal IGwenGui GUI { get; private set; } = null!;

    internal FontHolder Fonts { get; private set; } = null!;

    private static String GetIcon(String name)
    {
        iconNames.Add(name);

        return name;
    }

    private static String GetImage(String name)
    {
        imageNames.Add(name);

        return name;
    }

    private void LoadAttributions(ILoadingContext loadingContext)
    {
        DirectoryInfo directory = FileSystem.GetResourceDirectory("Attribution");

        if (!directory.Exists)
        {
            loadingContext.ReportWarning(nameof(Attribution), directory, "Directory does not exist");

            return;
        }

        foreach (FileInfo file in directory.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            String name = file.GetFileNameWithoutExtension().Replace(oldChar: '-', newChar: ' ');

            String? text = null;

            try
            {
                text = file.ReadAllText();
            }
            catch (IOException exception)
            {
                loadingContext.ReportWarning(nameof(Attribution), file, exception);
            }

            if (text == null) continue;

            attributions.Add(new Attribution(name, text));
            loadingContext.ReportSuccess(nameof(Attribution), file);
        }
    }

    private static Dictionary<String, TexturePreload> GetTexturePreloads()
    {
        Dictionary<String, TexturePreload> textures = new();

        foreach (String name in iconNames) textures[name] = new TexturePreload(GetIconName(name), name);

        foreach (String name in imageNames) textures[name] = new TexturePreload(GetImageName(name), name);

        return textures;
    }

    private void LoadGUI(Client window, ILoadingContext loadingContext)
    {
        FileInfo skin1 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin1.png");
        FileInfo skin2 = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin2.png");
        FileInfo shader = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");

        List<FileInfo> skinFiles = [skin1, skin2];
        Dictionary<FileInfo, Exception> skinLoadingErrors = new();

        String? shaderLoadingError = null;

        Dictionary<String, TexturePreload> textures = GetTexturePreloads();
        Dictionary<String, Exception?> textureLoadingErrors = new();

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

                    foreach ((String _, TexturePreload texture) in textures) settings.TexturePreloads.Add(texture);

                    settings.TexturePreloadErrorCallback = (texture, exception) => textureLoadingErrors[texture.Name] = exception;
                }));

        GUI.Load();

        foreach (FileInfo skinFile in skinFiles) ReportSkinLoading(skinLoadingErrors.GetValueOrDefault(skinFile), skinFile, loadingContext);

        ReportTextureLoading(textures, textureLoadingErrors, loadingContext);
        ReportShaderLoading(shaderLoadingError, shader, loadingContext);

        Modals.SetUpLanguage();

        Fonts = new FontHolder(GUI.Root.Skin);
    }

    private static void ReportSkinLoading(Exception? skinLoadingError, FileSystemInfo skinFile, ILoadingContext loadingContext)
    {
        if (skinLoadingError != null)
            loadingContext.ReportWarning(nameof(GUI), skinFile, skinLoadingError);
        else
            loadingContext.ReportSuccess(nameof(GUI), skinFile);
    }

    private static void ReportShaderLoading(String? shaderLoadingError, FileSystemInfo shader, ILoadingContext loadingContext)
    {
        const String type = "Shader";

        if (shaderLoadingError != null)
            loadingContext.ReportFailure(type, shader, shaderLoadingError, abort: true);
        else
            loadingContext.ReportSuccess(type, shader);
    }

    private static void ReportTextureLoading(Dictionary<String, TexturePreload> textures, IReadOnlyDictionary<String, Exception?> textureLoadingErrors, ILoadingContext loadingContext)
    {
        foreach ((String name, TexturePreload texture) in textures)
        {
            Exception? error = textureLoadingErrors.GetValueOrDefault(name);

            if (error != null)
                loadingContext.ReportWarning(nameof(GUI), texture.File, error);
            else
                loadingContext.ReportSuccess(nameof(GUI), texture.File);
        }
    }

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load(Client window, ILoadingContext loadingContext)
    {
        Throw.IfDisposed(disposed);

        using (loadingContext.BeginStep("UI"))
        {
            LoadAttributions(loadingContext);
            LoadGUI(window, loadingContext);
        }
    }

    private static (Document document, String name) CreateAttribution(Attribution attribution, Context context)
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
    internal IEnumerable<(Document document, String name)> CreateAttributions(Context context)
    {
        Throw.IfDisposed(disposed);

        return attributions.Select(attribution => CreateAttribution(attribution, context));
    }

    private static FileInfo GetImageName(String name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Images").GetFile($"{name}.png");
    }

    private static FileInfo GetIconName(String name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Icons").GetFile($"{name}.png");
    }

    private sealed record Attribution(String Name, String Text);

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
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

    #endregion DISPOSABLE
}

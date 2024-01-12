// <copyright file="UIResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gwen.Net.RichText;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.UI.Platform;
using VoxelGame.UI.UserInterfaces;
using VoxelGame.UI.Utility;

namespace VoxelGame.UI;

/// <summary>
///     All resources used by the UI, such as images and icons.
/// </summary>
public class UIResources
{
    private static readonly List<string> iconNames = new();
    private static readonly List<string> imageNames = new();
    private readonly List<Attribution> attributions = new();

    internal string ResetIcon { get; } = GetIcon("reset");
    internal string LoadIcon { get; } = GetIcon("load");
    internal string DeleteIcon { get; } = GetIcon("delete");

    internal string StartImage { get; } = GetImage("start");

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
        FileInfo skin = FileSystem.GetResourceDirectory("GUI").GetFile("VoxelSkin.png");
        FileInfo shader = FileSystem.GetResourceDirectory("Shaders").GetFile("GUI.hlsl");

        Exception? skinLoadingError = null;
        string? shaderLoadingError = null;

        Dictionary<string, TexturePreload> textures = GetTexturePreloads();
        Dictionary<string, Exception?> textureLoadingErrors = new();

        GUI = GwenGuiFactory.CreateFromClient(
            window,
            GwenGuiSettings.Default.From(
                settings =>
                {
                    settings.SkinFile = skin;
                    settings.SkinLoadingErrorCallback = exception => skinLoadingError = exception;

                    settings.ShaderFile = shader;

                    settings.ShaderLoadingErrorCallback =
                        exception => shaderLoadingError = exception;

                    foreach ((string _, TexturePreload texture) in textures) settings.TexturePreloads.Add(texture);

                    settings.TexturePreloadErrorCallback = (texture, exception) => textureLoadingErrors[texture.Name] = exception;
                }));

        GUI.Load();

        ReportSkinLoading(skinLoadingError, skin, loadingContext);
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
        using (loadingContext.BeginStep(Events.ResourceLoad, "UI"))
        {
            LoadAttributions(loadingContext);
            LoadGUI(window, loadingContext);
        }
    }

    /// <summary>
    ///     Unloads all the resources.
    /// </summary>
    public void Unload()
    {
        attributions.Clear();
        GUI.Dispose();
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

    internal IEnumerable<(Document document, string name)> CreateAttributions(Context context)
    {
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

    private sealed record Attribution(string Name, string Text); // todo: remove the no longer needed attribution files
}

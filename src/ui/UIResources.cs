// <copyright file="UIResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gwen.Net.RichText;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI;

/// <summary>
///     All resources used by the UI, such as images and icons.
/// </summary>
public class UIResources
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<UIResources>();

    private readonly List<Attribution> attributions = new();

    internal string ResetIcon { get; } = GetIconName("reset");
    internal string LoadIcon { get; } = GetIconName("load");
    internal string DeleteIcon { get; } = GetIconName("delete");

    internal string StartImage { get; } = GetImageName("start");

    private void LoadAttributions()
    {
        DirectoryInfo directory = FileSystem.AccessResourceDirectory("Attribution");

        if (!directory.Exists) return;

        foreach (FileInfo file in directory.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
        {
            string name = file.GetFileNameWithoutExtension().Replace(oldChar: '-', newChar: ' ');

            string? text = null;

            try
            {
                text = file.ReadAllText();
            }
            catch (IOException)
            {
                logger.LogWarning(Events.ResourceLoad, "Could not read attribution file: {Path}", file.FullName);
            }

            if (text == null) continue;

            attributions.Add(new Attribution(name, text));
        }
    }

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load()
    {
        LoadAttributions();

        logger.LogInformation(Events.ResourceLoad, "UI resources loaded");
    }

    /// <summary>
    ///     Unloads all the resources.
    /// </summary>
    public void Unload()
    {
        attributions.Clear();
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

    private static string GetImageName(string name)
    {
        return FileSystem.AccessResourceDirectory("GUI").GetFile($"{name}.png").FullName;
    }

    private static string GetIconName(string name)
    {
        return FileSystem.AccessResourceDirectory("GUI", "Icons").GetFile($"{name}.png").FullName;
    }

    private sealed record Attribution(string Name, string Text);
}

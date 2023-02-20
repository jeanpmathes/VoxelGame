// <copyright file="UIResources.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gwen.Net.RichText;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.UI;

/// <summary>
///     All resources used by the UI, such as images and icons.
/// </summary>
public class UIResources
{
    private readonly List<Attribution> attributions = new();

    internal string ResetIcon { get; } = GetIconName("reset");
    internal string LoadIcon { get; } = GetIconName("load");
    internal string DeleteIcon { get; } = GetIconName("delete");

    internal string StartImage { get; } = GetImageName("start");

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

    /// <summary>
    ///     Loads all the resources.
    /// </summary>
    public void Load(LoadingContext loadingContext)
    {
        using (loadingContext.BeginStep(Events.ResourceLoad, "UI"))
        {
            LoadAttributions(loadingContext);
        }
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
        return FileSystem.GetResourceDirectory("GUI").GetFile($"{name}.png").FullName;
    }

    private static string GetIconName(string name)
    {
        return FileSystem.GetResourceDirectory("GUI", "Icons").GetFile($"{name}.png").FullName;
    }

    private sealed record Attribution(string Name, string Text);
}

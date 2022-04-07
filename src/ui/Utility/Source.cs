// <copyright file="Source.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.UI.Utility;
    #pragma warning disable CA1812 // Pure static classes cannot have a logger.

/// <summary>
///     A utility class to access image resources.
/// </summary>
[SuppressMessage("ReSharper", "ConvertToStaticClass", Justification = "Pure static classes cannot have a logger.")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class Source
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Source>();
    private Source() {}

    internal static string GetImageName(string name)
    {
        return $"Resources/GUI/{name}.png";
    }

    internal static string GetIconName(string name)
    {
        return $"Resources/GUI/Icons/{name}.png";
    }

    internal static string GetTextContent(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (IOException e)
        {
            logger.LogError(Events.FileIO, e, "Could not load text content");

            return "";
        }
    }
}

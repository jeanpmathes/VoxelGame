// <copyright file="Source.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.UI.Utility
{
    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public sealed class Source
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Source>();
        private Source() {}

        public static string GetImageName(string name)
        {
            return $"Resources/GUI/{name}.png";
        }

        public static string GetIconName(string name)
        {
            return $"Resources/GUI/Icons/{name}.png";
        }

        public static string GetTextContent(string path)
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
}
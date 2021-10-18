// <copyright file="Source.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.UI.Utility
{
    public static class Source
    {
        public static string GetImageName(string name)
        {
            return $"Resources/GUI/{name}.png";
        }

        public static string GetIconName(string name)
        {
            return $"Resources/GUI/Icons/{name}.png";
        }
    }
}
// <copyright file="Quality.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;

namespace VoxelGame.Core.Visuals
{
    public enum Quality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public static class Qualities
    {
        public const int Count = 4;

        public static IEnumerable<Quality> All()
        {
            yield return Quality.Low;
            yield return Quality.Medium;
            yield return Quality.High;
            yield return Quality.Ultra;
        }

        public static string Name(this Quality quality)
        {
            return quality.ToString();
        }
    }
}

// <copyright file="ScreenshotFunc.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Definition;

internal static partial class Native
{
    /// <summary>
    ///     A function that receives a screenshot.
    /// </summary>
    internal delegate void ScreenshotFunc(IntPtr data, UInt32 width, UInt32 height);
}

// <copyright file="ScreenshotFunc.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Definition;

public static partial class Native
{
    /// <summary>
    ///     A function that receives a screenshot.
    /// </summary>
    public delegate void ScreenshotFunc(IntPtr data, uint width, uint height);
}

// <copyright file="Screen.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// Common functionality associated with the screen.
    /// </summary>
    public abstract class Screen : IDisposable
    {
        #region PUBLIC STATIC PROPERTIES

        /// <summary>
        /// Gets the window size. The value is equal to the value retrieved from <see cref="Client.Instance"/>.
        /// </summary>
        public static Vector2i Size { get => Instance.Client.Size; set { Instance.Client.Size = value; } }

        /// <summary>
        /// Gets the aspect ratio <c>x/y</c>.
        /// </summary>
        public static float AspectRatio { get => Size.X / (float)Size.Y; }

        /// <summary>
        /// Gets whether the screen is in fullscreen.
        /// </summary>
        public static bool IsFullscreen { get => Instance.Client.IsFullscreen; }

        /// <summary>
        /// Gets whether the screen is focused.
        /// </summary>
        public static bool IsFocused { get => Instance.Client.IsFocused; }

        #endregion PUBLIC STATIC PROPERTIES

        private protected static Screen Instance { get; set; } = null!;
        private protected abstract Client Client { get; set; }

        public abstract void Clear();

        public abstract void Draw();

        #region PUBLIC STATIC METHODS

        private protected abstract void SetCursor_Implementation(bool visible, bool tracked, bool grabbed);

        public static void SetCursor(bool visible, bool tracked = false, bool grabbed = false)
        {
            Instance.SetCursor_Implementation(visible, tracked, grabbed);
        }

        private protected abstract void SetFullscreen_Implementation(bool fullscreen);

        /// <summary>
        /// Set if the screen should be in fullscreen.
        /// </summary>
        /// <param name="fullscreen">If fullscreen should be active.</param>
        public static void SetFullscreen(bool fullscreen)
        {
            Instance.SetFullscreen_Implementation(fullscreen);
        }

        private protected abstract void TakeScreenshot_Implementation(string directory);

        /// <summary>
        /// Takes a screenshot and saves it to the specified directory.
        /// </summary>
        /// <param name="directory">The directory in which the screenshot should be saved.</param>
        public static void TakeScreenshot(string directory)
        {
            Instance.TakeScreenshot_Implementation(directory);
        }

        #endregion PUBLIC STATIC METHODS

        #region IDisposable Support

        protected abstract void Dispose(bool disposing);

        ~Screen()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
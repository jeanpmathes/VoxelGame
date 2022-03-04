// <copyright file="ScreenBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Rendering;
using VoxelGame.Input.Actions;

namespace VoxelGame.Client.Application
{
    /// <summary>
    ///     The behaviour of the screen. This class offers data like FPS and UPS.
    /// </summary>
    internal sealed class ScreenBehaviour : IDisposable
    {
        private const int DeltaBufferCapacity = 30;
        private readonly Client client;

        private readonly ToggleButton fullscreenToggle;
        private readonly CircularTimeBuffer renderDeltaBuffer = new(DeltaBufferCapacity);

        private readonly Screen screen;

        private readonly CircularTimeBuffer updateDeltaBuffer = new(DeltaBufferCapacity);

        internal ScreenBehaviour(Client client)
        {
            screen = new Screen(client);
            this.client = client;

            fullscreenToggle = client.Keybinds.GetToggle(client.Keybinds.Fullscreen);
        }

        /// <summary>
        ///     Get the fps of the screen.
        /// </summary>
        internal double FPS => 1.0 / renderDeltaBuffer.Average;

        /// <summary>
        ///     Get the ups of the screen.
        /// </summary>
        internal double UPS => 1.0 / updateDeltaBuffer.Average;

        /// <summary>
        ///     Clear the screen.
        /// </summary>
        internal void Clear()
        {
            screen.Clear();
        }

        /// <summary>
        ///     Draw the screen.
        /// </summary>
        /// <param name="time">The time since the last draw operation.</param>
        internal void Draw(double time)
        {
            screen.Draw();
            renderDeltaBuffer.Write(time);
        }

        /// <summary>
        ///     Update the screen.
        /// </summary>
        /// <param name="time">The time since the last update operation.</param>
        internal void Update(double time)
        {
            if (client.IsFocused && fullscreenToggle.Changed) Screen.SetFullscreen(!client.IsFullscreen);

            updateDeltaBuffer.Write(time);
        }

        #region IDisposable Support

        private bool disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~ScreenBehaviour()
        {
            Dispose(disposing: false);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing) screen.Dispose();

            disposed = true;
        }

        #endregion IDisposable Support
    }
}

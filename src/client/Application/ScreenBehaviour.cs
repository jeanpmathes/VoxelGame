// <copyright file="ScreenBehaviour.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Collections;
using VoxelGame.Support.Input.Actions;

namespace VoxelGame.Client.Application;

/// <summary>
///     The behaviour of the screen. This class offers data like FPS and UPS.
/// </summary>
internal sealed class ScreenBehaviour
{
    private const int DeltaBufferCapacity = 50;
    private readonly Client client;

    private readonly ToggleButton fullscreenToggle;
    private readonly CircularTimeBuffer renderDeltaBuffer = new(DeltaBufferCapacity);

    private readonly CircularTimeBuffer updateDeltaBuffer = new(DeltaBufferCapacity);

    internal ScreenBehaviour(Client client)
    {
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
    ///     Call when drawing the screen.
    /// </summary>
    /// <param name="time">The time since the last draw operation.</param>
    internal void Draw(double time)
    {
        renderDeltaBuffer.Write(time);
    }

    /// <summary>
    ///     Call when updating.
    /// </summary>
    /// <param name="time">The time since the last update operation.</param>
    internal void Update(double time)
    {
        if (client.IsFocused && fullscreenToggle.Changed) client.ToggleFullscreen();

        updateDeltaBuffer.Write(time);
    }
}

// <copyright file="Mouse.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Support.Input.Devices;

/// <summary>
///     Represents of the mouse.
/// </summary>
public class Mouse
{
    private readonly InputManager input;
    private Vector2d oldDelta;
    private Vector2d oldPosition;

    private Vector2i? storedPosition;

    internal Mouse(InputManager input)
    {
        this.input = input;
    }

    /// <summary>
    ///     Get the mouse delta of the current frame.
    ///     The delta is not raw, as some scaling and smoothing is applied.
    /// </summary>
    public Vector2d Delta { get; private set; }

    internal void Update()
    {
        Vector2d delta = input.Window.MousePosition - oldPosition;

        if (input.Window.Size.X == 0 || input.Window.Size.Y == 0)
        {
            Delta = Vector2d.Zero;

            return;
        }

        double xScale = 1f / input.Window.Size.X;
        double yScale = 1f / input.Window.Size.Y;

        delta = Vector2d.Multiply(delta, (xScale, -yScale)) * 1000;
        delta = Vector2d.Lerp(oldDelta, delta, blend: 0.7f);

        oldDelta = Delta;
        oldPosition = input.Window.MousePosition;
        Delta = delta;
    }

    /// <summary>
    ///     Store the mouse position to restore it later. If there is already a stored position, it will be overwritten.
    /// </summary>
    public void StorePosition()
    {
        storedPosition = input.Window.MousePosition;
    }

    /// <summary>
    ///     Restore the stored mouse position. If there is no stored position, nothing will happen.
    /// </summary>
    public void RestorePosition()
    {
        if (storedPosition == null) return;

        input.Window.MousePosition = storedPosition.Value;
    }
}


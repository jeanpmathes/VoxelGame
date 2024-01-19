// <copyright file="Mouse.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Support.Core;
using VoxelGame.Support.Definition;

namespace VoxelGame.Support.Input.Devices;

/// <summary>
///     Represents of the mouse.
/// </summary>
public class Mouse
{
    private readonly Client client;

    private Vector2d oldDelta;
    private Vector2d oldPosition;

    private Vector2i? storedPosition;

    private Vector2i position;

    private bool isCursorLocked;
    private bool isCursorLockRequiredOnFocus;

    internal Mouse(Client client)
    {
        this.client = client;

        this.client.OnFocusChange += (_, _) =>
        {
            if (this.client.IsFocused && isCursorLockRequiredOnFocus)
            {
                SetCursorLock(locked: true);
                isCursorLockRequiredOnFocus = false;
            }

            if (!this.client.IsFocused && isCursorLocked)
            {
                SetCursorLock(locked: false);
                isCursorLockRequiredOnFocus = true;
            }
        };
    }

    /// <summary>
    ///     Get or set the position of the mouse.
    /// </summary>
    public Vector2i Position
    {
        get => position;
        private set
        {
            position = value;
            Native.SetMousePosition(client, position.X, position.Y);
        }
    }

    /// <summary>
    ///     Get the mouse delta of the current frame.
    ///     The delta is not raw, as some scaling and smoothing is applied.
    /// </summary>
    public Vector2d Delta { get; private set; }

    internal void Update()
    {
        position = Native.GetMousePosition(client);

        Vector2d delta = Position - oldPosition;

        if (client.Size.X == 0 || client.Size.Y == 0)
        {
            Delta = Vector2d.Zero;

            return;
        }

        double xScale = 1f / client.Size.X;
        double yScale = 1f / client.Size.Y;

        delta = Vector2d.Multiply(delta, (xScale, -yScale)) * 1000;
        delta = Vector2d.Lerp(oldDelta, delta, blend: 0.7f);

        if (isCursorLocked) Position = new Vector2i(client.Size.X, client.Size.Y) / 2;

        oldDelta = Delta;
        oldPosition = Position;

        Delta = delta;
    }

    /// <summary>
    /// Pass a mouse move event to the mouse class.
    /// This will not trigger a call to set the mouse position.
    /// </summary>
    internal void OnMouseMove(Vector2i newPosition)
    {
        position = newPosition;
    }

    /// <summary>
    ///     Set the mouse cursor type.
    /// </summary>
    public void SetCursorType(MouseCursor cursor)
    {
        Native.SetCursorType(client, cursor);
    }

    /// <summary>
    /// Set whether the cursor should be locked, i.e. hidden and grabbed.
    /// </summary>
    /// <param name="locked">Whether the cursor should be locked.</param>
    public void SetCursorLock(bool locked)
    {
        isCursorLocked = locked;

        if (locked) storedPosition = Position;

        Native.SetCursorLock(client, locked);

        if (!locked && storedPosition != null) Position = storedPosition.Value;
    }
}

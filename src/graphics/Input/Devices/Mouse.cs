// <copyright file="Mouse.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Devices;

/// <summary>
///     Represents of the mouse.
/// </summary>
public class Mouse
{
    private readonly Client client;

    private Vector2d oldDelta;

    private Vector2i? storedPosition;

    private Vector2i oldPosition;
    private Vector2i position;

    private Boolean isCursorLocked;
    private Boolean isCursorLockRequiredOnFocus;

    internal Mouse(Client client)
    {
        this.client = client;

        this.client.FocusChanged += (_, _) =>
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

        this.client.SizeChanged += (_, e) =>
        {
            if (!isCursorLocked) return;

            oldPosition = CalculateResizedPosition(oldPosition);
            Position = CalculateResizedPosition(Position);

            if (storedPosition != null)
                storedPosition = CalculateResizedPosition(storedPosition.Value);

            Vector2i CalculateResizedPosition(Vector2i previous)
            {
                return (Vector2i) (previous.ToVector2() / e.OldSize * e.NewSize);
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
            NativeMethods.SetMousePosition(client, position.X, position.Y);
        }
    }

    /// <summary>
    ///     Get the mouse delta of the current frame.
    ///     The delta is not raw, as some scaling and smoothing is applied.
    /// </summary>
    public Vector2d Delta { get; private set; }

    private Vector2i Center => client.Size / 2;

    internal void LogicUpdate()
    {
        NativeMethods.GetMousePosition(client, out Int64 x, out Int64 y);
        position = new Vector2i((Int32) x, (Int32) y);

        Vector2d newDelta = position - oldPosition;

        if (client.Size.X == 0 || client.Size.Y == 0)
        {
            Delta = Vector2d.Zero;

            return;
        }

        Double xScale = 1f / client.Size.X;
        Double yScale = 1f / client.Size.Y;

        newDelta = Vector2d.Multiply(newDelta, (xScale, -yScale)) * 1000;
        newDelta = Vector2d.Lerp(oldDelta, newDelta, blend: 0.7f);

        if (isCursorLocked)
            Position = Center;

        oldDelta = Delta;
        oldPosition = Position;

        Delta = newDelta;
    }

    /// <summary>
    ///     Pass a mouse move event to the mouse class.
    ///     This will not trigger a call to set the mouse position.
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
        NativeMethods.SetCursorType(client, cursor);
    }

    /// <summary>
    ///     Set whether the cursor should be locked, i.e. hidden and grabbed.
    /// </summary>
    /// <param name="locked">Whether the cursor should be locked.</param>
    public void SetCursorLock(Boolean locked)
    {
        Boolean wasCursorLocked = isCursorLocked;
        isCursorLocked = locked;

        if (locked) storedPosition = Position;

        NativeMethods.SetCursorLock(client, locked);

        if (locked)
        {
            Position = Center;
            Delta = Vector2d.Zero;

            oldPosition = Position;
            oldDelta = Delta;
        }
        else
        {
            if (storedPosition != null && wasCursorLocked) Position = storedPosition.Value;

            isCursorLockRequiredOnFocus = false;
        }
    }
}

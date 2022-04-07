// <copyright file="CombinedState.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Input.Internal;

/// <summary>
///     The combined state of mouse and keyboard.
/// </summary>
internal readonly struct CombinedState : IEquatable<CombinedState>
{
    internal KeyboardState Keyboard { get; }
    internal MouseState Mouse { get; }

    private readonly Dictionary<KeyOrButton, bool> overrides;

    internal bool IsAnyKeyOrButtonDown => Keyboard.IsAnyKeyDown || Mouse.IsAnyButtonDown;

    internal KeyOrButton Any
    {
        get
        {
            foreach (Keys key in (Keys[]) Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown) continue;

                if (Keyboard[key]) return new KeyOrButton(key);
            }

            foreach (MouseButton mouseButton in (MouseButton[]) Enum.GetValues(typeof(MouseButton)))
            {
                if (mouseButton == MouseButton.Last) continue;

                if (Mouse[mouseButton]) return new KeyOrButton(mouseButton);
            }

            throw new InvalidOperationException();
        }
    }

    internal CombinedState(KeyboardState keyboard, MouseState mouse, Dictionary<KeyOrButton, bool> overrides)
    {
        Keyboard = keyboard;
        Mouse = mouse;

        this.overrides = overrides;
    }

    internal bool IsKeyOrButtonDown(KeyOrButton keyOrButton)
    {
        return overrides.ContainsKey(keyOrButton) ? overrides[keyOrButton] : keyOrButton.GetState(this);
    }

    internal bool IsKeyOrButtonUp(KeyOrButton keyOrButton)
    {
        return !IsKeyOrButtonDown(keyOrButton);
    }

    /// <inheritdoc />
    public bool Equals(CombinedState other)
    {
        return overrides.Equals(other.overrides) && Keyboard.Equals(other.Keyboard) && Mouse.Equals(other.Mouse);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CombinedState other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(overrides, Keyboard, Mouse);
    }

    public static bool operator ==(CombinedState left, CombinedState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CombinedState left, CombinedState right)
    {
        return !left.Equals(right);
    }
}

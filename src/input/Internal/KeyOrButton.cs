// <copyright file="KeyOrButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGame.Input.Internal;

/// <summary>
///     Represents a key or a button.
/// </summary>
public readonly struct KeyOrButton : IEquatable<KeyOrButton>
{
    private readonly Keys? key;
    private readonly MouseButton? button;

    /// <summary>
    ///     Create a new <see cref="KeyOrButton" /> from a <see cref="Keys" />.
    /// </summary>
    /// <param name="key">The key to use.</param>
    public KeyOrButton(Keys key)
    {
        this.key = key;
        button = null;
    }

    /// <summary>
    ///     Create a new <see cref="KeyOrButton" /> from a <see cref="MouseButton" />.
    /// </summary>
    /// <param name="button">The button to use.</param>
    public KeyOrButton(MouseButton button)
    {
        key = null;
        this.button = button;
    }

    /// <summary>
    ///     Create a new <see cref="KeyOrButton" /> from a loaded pair.
    /// </summary>
    /// <param name="settings">The settings to load from.</param>
    public KeyOrButton(KeyButtonPair settings)
    {
        Debug.Assert(!settings.Default);

        if (settings.Key != Keys.Unknown)
        {
            key = settings.Key;
            button = null;
        }
        else
        {
            key = null;
            button = settings.Button;
        }
    }

    private bool IsKeyboardKey => key != null;
    private bool IsMouseButton => button != null;

    internal bool GetState(CombinedState state)
    {
        if (IsKeyboardKey) return state.Keyboard[(Keys) key!];

        if (IsMouseButton) return state.Mouse[(MouseButton) button!];

        return false;
    }

    /// <summary>
    ///     Get serializable settings for this key or button.
    /// </summary>
    public KeyButtonPair GetSettings(bool isDefault)
    {
        return new() {Key = key ?? Keys.Unknown, Button = button ?? MouseButton.Last, Default = isDefault};
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsKeyboardKey) return key.ToString()!;

        if (IsMouseButton) return button.ToString()!;

        return "unknown";
    }

    /// <inheritdoc />
    public bool Equals(KeyOrButton other)
    {
        return key == other.key && button == other.button;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is KeyOrButton other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(key, button);
    }

    /// <summary>
    ///     Checks if two <see cref="KeyOrButton" />s are equal.
    /// </summary>
    public static bool operator ==(KeyOrButton left, KeyOrButton right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two <see cref="KeyOrButton" />s are not equal.
    /// </summary>
    public static bool operator !=(KeyOrButton left, KeyOrButton right)
    {
        return !left.Equals(right);
    }
}

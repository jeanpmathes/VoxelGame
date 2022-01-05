// <copyright file="KeyOrButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input.Internal
{
    /// <summary>
    ///     Represents a key or a button.
    /// </summary>
    public readonly struct KeyOrButton
    {
        private readonly Key? key;
        private readonly MouseButton? button;

        /// <summary>
        ///     Create a new <see cref="KeyOrButton" /> from a <see cref="Key" />.
        /// </summary>
        /// <param name="key">The key to use.</param>
        public KeyOrButton(Key key)
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

            if (settings.Key != Key.Unknown)
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

        internal bool State(CombinedState state)
        {
            if (IsKeyboardKey) return state.Keyboard[(Key) key!];

            if (IsMouseButton) return state.Mouse[(MouseButton) button!];

            return false;
        }

        /// <summary>
        ///     Get serializable settings for this key or button.
        /// </summary>
        public KeyButtonPair Settings => new() { Key = key ?? Key.Unknown, Button = button ?? MouseButton.LastButton };

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is KeyOrButton other) return key == other.key && button == other.button;

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(key, button);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsKeyboardKey) return key.ToString()!;

            if (IsMouseButton) return button.ToString()!;

            return "unknown";
        }
    }
}
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
    public readonly struct KeyOrButton
    {
        private readonly Key? key;
        private readonly MouseButton? button;

        public KeyOrButton(Key key)
        {
            this.key = key;
            button = null;
        }

        public KeyOrButton(MouseButton button)
        {
            key = null;
            this.button = button;
        }

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

        public KeyButtonPair Settings => new() { Key = key ?? Key.Unknown, Button = button ?? MouseButton.LastButton };

        public override bool Equals(object? obj)
        {
            if (obj is KeyOrButton other) return key == other.key && button == other.button;

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(key, button);
        }

        public override string ToString()
        {
            if (IsKeyboardKey) return key.ToString()!;

            if (IsMouseButton) return button.ToString()!;

            return "unknown";
        }
    }
}
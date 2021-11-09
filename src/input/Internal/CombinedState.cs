// <copyright file="CombinedState.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input.Internal
{
    internal readonly struct CombinedState
    {
        internal KeyboardState Keyboard { get; }
        internal MouseState Mouse { get; }

        private readonly Dictionary<KeyOrButton, bool> overrides;

        public bool IsAnyKeyOrButtonDown => Keyboard.IsAnyKeyDown || Mouse.IsAnyButtonDown;

        public KeyOrButton Any
        {
            get
            {
                foreach (Key key in (Key[]) Enum.GetValues(typeof(Key)))
                {
                    if (key == Key.Unknown) continue;

                    if (Keyboard[key]) return new KeyOrButton(key);
                }

                foreach (MouseButton mouseButton in (MouseButton[]) Enum.GetValues(typeof(MouseButton)))
                {
                    if (mouseButton == MouseButton.LastButton) continue;

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

        public bool IsKeyOrButtonDown(KeyOrButton keyOrButton)
        {
            return overrides.ContainsKey(keyOrButton) ? overrides[keyOrButton] : keyOrButton.State(this);
        }

        public bool IsKeyOrButtonUp(KeyOrButton keyOrButton)
        {
            return !IsKeyOrButtonDown(keyOrButton);
        }
    }
}
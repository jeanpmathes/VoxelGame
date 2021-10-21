// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input.Devices;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input
{
    public class InputManager
    {
        private readonly Dictionary<KeyOrButton, bool> overrides = new();
        private readonly HashSet<KeyOrButton> pullDowns = new();

        public InputManager(GameWindow window)
        {
            Window = window;

            Mouse = new Mouse(this);
            Listener = new InputListener();
        }

        public Mouse Mouse { get; }
        public InputListener Listener { get; }

        internal GameWindow Window { get; }

        internal CombinedState CurrentState { get; private set; }

        public void UpdateState(KeyboardState keyboard, MouseState mouse)
        {
            SetOverrides(new CombinedState(keyboard, mouse, new Dictionary<KeyOrButton, bool>()));
            CurrentState = new CombinedState(keyboard, mouse, overrides);

            Mouse.Update();
            OnUpdate?.Invoke();

            Listener.ProcessInput(CurrentState);
        }

        private void SetOverrides(CombinedState actualState)
        {
            pullDowns.RemoveWhere(
                keyOrButton =>
                {
                    if (actualState.IsKeyOrButtonDown(keyOrButton)) return false;

                    overrides.Remove(keyOrButton);

                    return true;
                });
        }

        public void AddPullDown(KeyOrButton keyOrButton)
        {
            pullDowns.Add(keyOrButton);
            overrides[keyOrButton] = false;
        }

        public event Action? OnUpdate;
    }
}

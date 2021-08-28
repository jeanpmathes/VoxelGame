// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using VoxelGame.Input.Devices;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input
{
    public class InputManager
    {
        public Mouse Mouse { get; }

        internal GameWindow Window { get; }

        public InputManager(GameWindow window)
        {
            Window = window;

            Mouse = new Mouse(this);
        }

        internal CombinedState CurrentState { get; private set; }

        public void UpdateState(KeyboardState keyboard, MouseState mouse)
        {
            CurrentState = new CombinedState(keyboard, mouse);

            Mouse.Update();
            OnUpdate?.Invoke();
        }

        public event Action? OnUpdate;
    }
}
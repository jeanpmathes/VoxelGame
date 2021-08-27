// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input
{
    public class InputManager
    {
        internal CombinedState CurrentState { get; private set; }

        public void SetState(KeyboardState keyboard, MouseState mouse)
        {
            CurrentState = new CombinedState(keyboard, mouse);

            OnUpdate?.Invoke();
        }

        public event Action? OnUpdate;
    }
}
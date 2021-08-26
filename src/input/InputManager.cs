// <copyright file="InputManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input
{
    public class InputManager
    {
        public KeyboardState CurrentKeyboardState { get; private set; }
        public MouseState CurrentMouseState { get; private set; }

        public void SetState(KeyboardState keyboard, MouseState mouse)
        {
            CurrentKeyboardState = keyboard;
            CurrentMouseState = mouse;
        }
    }
}
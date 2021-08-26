// <copyright file="KeybindManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input;

namespace VoxelGame.Client.Application
{
    internal class KeybindManager
    {
        private InputManager input;

        public KeyboardState Keyboard => input.CurrentKeyboardState;
        public MouseState Mouse => input.CurrentMouseState;

        public KeybindManager(InputManager input)
        {
            this.input = input;
        }
    }
}
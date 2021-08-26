// <copyright file="CombinedState.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input.Internal
{
    internal readonly struct CombinedState
    {
        public KeyboardState Keyboard { get; }
        public MouseState Mouse { get; }

        internal CombinedState(KeyboardState keyboard, MouseState mouse)
        {
            this.Keyboard = keyboard;
            this.Mouse = mouse;
        }

        public bool IsKeyOrButtonDown(KeyOrButton keyOrButton)
        {
            return keyOrButton.State(this);
        }

        public bool IsKeyOrButtonUp(KeyOrButton keyOrButton)
        {
            return !keyOrButton.State(this);
        }
    }
}
// <copyright file="Button.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public abstract class Button : InputAction
    {
        private protected KeyOrButton KeyOrButton { get; private set; }

        public bool IsDown { get; private protected set; }

        public bool IsUp => !IsDown;

        protected Button(KeyOrButton keyOrButton, InputManager input) : base(input)
        {
            KeyOrButton = keyOrButton;
        }

        public void SetBinding(KeyOrButton keyOrButton)
        {
            KeyOrButton = keyOrButton;
        }
    }
}
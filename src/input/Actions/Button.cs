// <copyright file="Button.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public abstract class Button : InputAction
    {
        private protected KeyOrButton KeyOrButton { get; }

        public bool IsDown { get; private protected set; }

        public bool IsUp => !IsDown;

        protected Button(Key key, InputManager input) : this(new KeyOrButton(key), input)
        {
        }

        protected Button(MouseButton mouseButton, InputManager input) : this(new KeyOrButton(mouseButton), input)
        {
        }

        private Button(KeyOrButton keyOrButton, InputManager input) : base(input)
        {
            KeyOrButton = keyOrButton;
        }
    }
}
// <copyright file="Button.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class Button : InputAction
    {
        private readonly KeyOrButton keyOrButton;

        public bool IsDown { get; private set; }

        public bool IsUp => !IsDown;

        public Button(Key key, InputManager input) : this(new KeyOrButton(key), input)
        {
        }

        public Button(MouseButton mouseButton, InputManager input) : this(new KeyOrButton(mouseButton), input)
        {
        }

        private Button(KeyOrButton keyOrButton, InputManager input) : base(input)
        {
            this.keyOrButton = keyOrButton;
        }

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;
            IsDown = state.IsKeyOrButtonDown(keyOrButton);
        }
    }
}
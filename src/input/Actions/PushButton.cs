// <copyright file="PushButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class PushButton : InputAction
    {
        private readonly KeyOrButton keyOrButton;

        private bool hasReleased;

        public bool Pushed { get; private set; }

        public PushButton(Key key, InputManager input) : this(new KeyOrButton(key), input)
        {
        }

        public PushButton(MouseButton mouseButton, InputManager input) : this(new KeyOrButton(mouseButton), input)
        {
        }

        private PushButton(KeyOrButton keyOrButton, InputManager input) : base(input)
        {
            this.keyOrButton = keyOrButton;
        }

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;

            Pushed = false;

            if (hasReleased && state.IsKeyOrButtonDown(keyOrButton))
            {
                hasReleased = false;
                Pushed = true;
            }
            else if (state.IsKeyOrButtonUp(keyOrButton))
            {
                hasReleased = true;
            }
        }
    }
}
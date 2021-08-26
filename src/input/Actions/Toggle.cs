// <copyright file="Toggle.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class Toggle
    {
        private readonly InputManager input;

        private readonly KeyOrButton keyOrButton;

        private bool hasReleased;

        public bool State { get; private set; }
        public bool Changed { get; private set; }

        public Toggle(Key key, InputManager input) : this(new KeyOrButton(key), input)
        {
        }

        public Toggle(MouseButton mouseButton, InputManager input) : this(new KeyOrButton(mouseButton), input)
        {
        }

        private Toggle(KeyOrButton keyOrButton, InputManager input)
        {
            this.keyOrButton = keyOrButton;

            this.input = input;
            input.OnUpdate += Update;
        }

        public void Clear()
        {
            State = false;
        }

        private void Update()
        {
            CombinedState state = input.CurrentState;

            Changed = false;

            if (hasReleased && state.IsKeyOrButtonDown(keyOrButton))
            {
                hasReleased = false;

                State = !State;
                Changed = true;
            }
            else if (state.IsKeyOrButtonUp(keyOrButton))
            {
                hasReleased = true;
            }
        }
    }
}
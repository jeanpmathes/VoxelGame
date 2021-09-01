// <copyright file="ToggleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class ToggleButton : Button
    {
        private bool hasReleased;
        private bool state;

        public bool State
        {
            get => state;
            private set
            {
                state = value;
                IsDown = value;
            }
        }

        public bool Changed { get; private set; }

        public ToggleButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input)
        {
        }

        public void Clear()
        {
            State = false;
        }

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;

            Changed = false;

            if (hasReleased && state.IsKeyOrButtonDown(KeyOrButton))
            {
                hasReleased = false;

                State = !State;
                Changed = true;
            }
            else if (state.IsKeyOrButtonUp(KeyOrButton))
            {
                hasReleased = true;
            }
        }
    }
}
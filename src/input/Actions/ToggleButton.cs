// <copyright file="ToggleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    /// <summary>
    ///     A toggle button, which toggles the state every time it is pressed.
    /// </summary>
    public class ToggleButton : Button
    {
        private bool hasReleased;
        private bool state;

        /// <summary>
        ///     Create a new toggle button.
        /// </summary>
        /// <param name="keyOrButton">The key or button to target.</param>
        /// <param name="input">The input manager providing the input.</param>
        public ToggleButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input) {}

        /// <summary>
        ///     Get the current button state.
        /// </summary>
        public bool State
        {
            get => state;
            private set
            {
                state = value;
                IsDown = value;
            }
        }

        /// <summary>
        ///     Whether the button was toggled this frame.
        /// </summary>
        public bool Changed { get; private set; }

        /// <summary>
        ///     Reset the button state.
        /// </summary>
        public void Clear()
        {
            State = false;
        }

        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <inheritdoc />
        protected override void Update(object? sender, EventArgs e)
        {
            CombinedState currentState = Input.CurrentState;

            Changed = false;

            if (hasReleased && currentState.IsKeyOrButtonDown(KeyOrButton))
            {
                hasReleased = false;

                State = !State;
                Changed = true;
            }
            else if (currentState.IsKeyOrButtonUp(KeyOrButton))
            {
                hasReleased = true;
            }
        }
    }
}

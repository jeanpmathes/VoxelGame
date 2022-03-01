// <copyright file="SimpleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    /// <summary>
    ///     A simple button, that can be pushed down.
    /// </summary>
    public class SimpleButton : Button
    {
        /// <summary>
        ///     Create a new simple button.
        /// </summary>
        /// <param name="keyOrButton">The button target.</param>
        /// <param name="input">The input manager.</param>
        public SimpleButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input) {}

        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <inheritdoc />
        protected override void Update(object? sender, EventArgs e)
        {
            CombinedState state = Input.CurrentState;
            IsDown = state.IsKeyOrButtonDown(KeyOrButton);
        }
    }
}

// <copyright file="Button.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    /// <summary>
    ///     A button input action.
    /// </summary>
    public abstract class Button : InputAction
    {
        /// <summary>
        ///     Create a new button.
        /// </summary>
        /// <param name="keyOrButton">The trigger key.</param>
        /// <param name="input">The input manager.</param>
        protected Button(KeyOrButton keyOrButton, InputManager input) : base(input)
        {
            KeyOrButton = keyOrButton;
        }

        /// <summary>
        ///     Get the used key or button.
        /// </summary>
        public KeyOrButton KeyOrButton { get; private set; }

        /// <summary>
        ///     Get whether the button is pressed.
        /// </summary>
        public bool IsDown { get; private protected set; }

        /// <summary>
        ///     Get whether the button is up.
        /// </summary>
        public bool IsUp => !IsDown;

        /// <summary>
        ///     Set the binding to a different key or button.
        /// </summary>
        /// <param name="keyOrButton">The new key or button.</param>
        public void SetBinding(KeyOrButton keyOrButton)
        {
            KeyOrButton = keyOrButton;
        }
    }
}

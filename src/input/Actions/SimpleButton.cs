// <copyright file="SimpleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class SimpleButton : Button
    {
        public SimpleButton(KeyOrButton keyOrButton, InputManager input) : base(keyOrButton, input) {}

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;
            IsDown = state.IsKeyOrButtonDown(KeyOrButton);
        }
    }
}
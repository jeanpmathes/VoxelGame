// <copyright file="SimpleButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;
using VoxelGame.Input.Internal;

namespace VoxelGame.Input.Actions
{
    public class SimpleButton : Button
    {
        public SimpleButton(Key key, InputManager input) : base(key, input)
        {
        }

        public SimpleButton(MouseButton mouseButton, InputManager input) : base(mouseButton, input)
        {
        }

        protected override void Update()
        {
            CombinedState state = Input.CurrentState;
            IsDown = state.IsKeyOrButtonDown(KeyOrButton);
        }
    }
}
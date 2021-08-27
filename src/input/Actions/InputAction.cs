// <copyright file="InputAction.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Input.Actions
{
    public abstract class InputAction
    {
        protected InputManager Input { get; }

        protected InputAction(InputManager input)
        {
            Input = input;

            input.OnUpdate += Update;
        }

        protected abstract void Update();
    }
}
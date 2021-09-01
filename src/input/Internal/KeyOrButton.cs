// <copyright file="KeyOrButton.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Windowing.Common.Input;

namespace VoxelGame.Input.Internal
{
    public readonly struct KeyOrButton
    {
        private readonly Key? key;
        private readonly MouseButton? button;

        public KeyOrButton(Key key)
        {
            this.key = key;
            this.button = null;
        }

        public KeyOrButton(MouseButton button)
        {
            this.key = null;
            this.button = button;
        }

        private bool IsKeyboardKey => key != null;
        private bool IsMouseButton => button != null;

        internal bool State(CombinedState state)
        {
            if (IsKeyboardKey)
            {
                return state.Keyboard[(Key) key!];
            }

            if (IsMouseButton)
            {
                return state.Mouse[(MouseButton) button!];
            }

            return false;
        }
    }
}
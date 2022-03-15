// <copyright file="Mouse.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;

namespace VoxelGame.Input.Devices
{
    /// <summary>
    ///     Represents of the mouse.
    /// </summary>
    public class Mouse
    {
        private readonly InputManager input;
        private Vector2 correction;
        private Vector2 oldDelta;

        private bool wasUnlocked;

        internal Mouse(InputManager input)
        {
            this.input = input;
        }

        /// <summary>
        ///     Get or set whether the mouse is locked to the center of the screen.
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        ///     Get the mouse delta of the current frame.
        /// </summary>
        public Vector2 Delta { get; private set; }

        private Vector2i LockPosition => new(input.Window.Size.X / 2, input.Window.Size.Y / 2);

        internal void Update()
        {
            Vector2 delta = input.Window.MouseState.Delta - correction;

            float xScale = 1f / input.Window.Size.X;
            float yScale = 1f / input.Window.Size.Y;

            delta = Vector2.Multiply(delta, (-xScale, yScale)) * 1000;
            delta = Vector2.Lerp(oldDelta, delta, blend: 0.7f);

            oldDelta = Delta;
            Delta = delta;

            if (Locked)
            {
                var lockPosition = LockPosition.ToVector2();

                correction = input.Window.MousePosition - lockPosition;
                input.Window.MousePosition = lockPosition;

                if (wasUnlocked) Delta = oldDelta = correction = Vector2.Zero;
            }
            else
            {
                correction = Vector2.Zero;
            }

            wasUnlocked = !Locked;
        }
    }
}

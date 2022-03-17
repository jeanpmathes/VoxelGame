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
        private Vector2 oldDelta;
        private Vector2 oldPosition;

        internal Mouse(InputManager input)
        {
            this.input = input;
        }

        /// <summary>
        ///     Get the mouse delta of the current frame.
        ///     The delta is not raw, as some scaling and smoothing is applied.
        /// </summary>
        public Vector2 Delta { get; private set; }

        internal void Update()
        {
            Vector2 delta = input.Window.MousePosition - oldPosition;

            float xScale = 1f / input.Window.Size.X;
            float yScale = 1f / input.Window.Size.Y;

            delta = Vector2.Multiply(delta, (xScale, -yScale)) * 1000;
            delta = Vector2.Lerp(oldDelta, delta, blend: 0.7f);

            oldDelta = Delta;
            oldPosition = input.Window.MouseState.Position;
            Delta = delta;
        }
    }
}

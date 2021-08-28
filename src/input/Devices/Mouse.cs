// <copyright file="Mouse.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;

namespace VoxelGame.Input.Devices
{
    public class Mouse
    {
        private readonly InputManager input;

        internal Mouse(InputManager input)
        {
            this.input = input;
        }

        public bool Locked { get; set; }
        public Vector2 Delta { get; private set; }

        private Vector2i LockPosition => new Vector2i(input.Window.Size.X / 2, input.Window.Size.Y / 2);

        private Vector2 oldDelta;
        private Vector2 correction;

        internal void Update()
        {
            Vector2 delta = input.Window.MouseDelta - correction;

            Console.WriteLine(delta);

            float xScale = 1f / input.Window.Size.X;
            float yScale = 1f / input.Window.Size.Y;

            delta = Vector2.Multiply(delta, (-xScale, yScale)) * 1000;
            delta = Vector2.Lerp(oldDelta, delta, 0.5f);

            oldDelta = Delta;
            Delta = delta;

            if (Locked)
            {
                var lockPosition = LockPosition.ToVector2();

                correction = input.Window.MousePosition - lockPosition;
                input.Window.MousePosition = lockPosition;
            }
            else
            {
                correction = Vector2.Zero;
            }
        }
    }
}
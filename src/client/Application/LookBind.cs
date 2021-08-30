// <copyright file="LookBind.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Input.Devices;

namespace VoxelGame.Client.Application
{
    public class LookBind
    {
        private readonly Mouse mouse;
        private readonly float sensitivity;

        public LookBind(Mouse mouse, float sensitivity)
        {
            this.mouse = mouse;
            this.sensitivity = sensitivity;
        }

        public Vector2 Value => mouse.Delta * sensitivity;
    }
}
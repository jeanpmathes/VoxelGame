// <copyright file="LookInput.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Input.Devices;

namespace VoxelGame.Client.Application
{
    public class LookInput
    {
        private readonly Mouse mouse;

        private float sensitivity;

        public LookInput(Mouse mouse, float sensitivity)
        {
            this.mouse = mouse;
            this.sensitivity = sensitivity;
        }

        public Vector2 Value => mouse.Delta * sensitivity;

        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = newSensitivity;
        }
    }
}
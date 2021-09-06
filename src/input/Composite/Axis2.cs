// <copyright file="Axis2.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Input.Composite
{
    public class Axis2
    {
        private readonly Axis x;
        private readonly Axis y;

        public Axis2(Axis x, Axis y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 Value => new Vector2(x.Value, y.Value);
    }
}
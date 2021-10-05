// <copyright file="Axis2.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Input.Composite
{
    public class InputAxis2
    {
        private readonly InputAxis x;
        private readonly InputAxis y;

        public InputAxis2(InputAxis x, InputAxis y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 Value => new(x.Value, y.Value);
    }
}
// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Actions;

namespace VoxelGame.Input.Composite
{
    public class Axis
    {
        private readonly Button positive;
        private readonly Button negative;

        public Axis(Button positive, Button negative)
        {
            this.positive = positive;
            this.negative = negative;
        }

        public float Value
        {
            get
            {
                var value = 0f;

                if (positive.IsDown) value++;
                if (negative.IsDown) value--;

                return value;
            }
        }
    }
}
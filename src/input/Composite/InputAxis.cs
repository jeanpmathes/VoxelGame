// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Actions;

namespace VoxelGame.Input.Composite
{
    public class InputAxis
    {
        private readonly Button negative;
        private readonly Button positive;

        public InputAxis(Button positive, Button negative)
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
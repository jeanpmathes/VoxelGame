// <copyright file="Axis.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Input.Actions;

namespace VoxelGame.Input.Composite
{
    /// <summary>
    ///     An input axis consisting of two <see cref="Button" />s.
    /// </summary>
    public class InputAxis
    {
        private readonly Button negative;
        private readonly Button positive;

        /// <summary>
        ///     Create a new input axis.
        /// </summary>
        /// <param name="positive">The positive button.</param>
        /// <param name="negative">The negative button.</param>
        public InputAxis(Button positive, Button negative)
        {
            this.positive = positive;
            this.negative = negative;
        }

        /// <summary>
        ///     Get the value of the axis.
        /// </summary>
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
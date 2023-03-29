// <copyright file="Axis2.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Support.Input.Composite;

/// <summary>
///     A two-dimensional axis.
/// </summary>
public class InputAxis2
{
    private readonly InputAxis x;
    private readonly InputAxis y;

    /// <summary>
    ///     Create a new axis.
    /// </summary>
    /// <param name="x">The x axis.</param>
    /// <param name="y">The y axis.</param>
    public InputAxis2(InputAxis x, InputAxis y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    ///     The current value of the axis.
    /// </summary>
    public Vector2 Value => new(x.Value, y.Value);
}



// <copyright file="IDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Graphics.Groups;

/// <summary>
///     An interface with common draw group operations, hiding the actual OpenGL internals.
/// </summary>
public interface IDrawGroup
{
    /// <summary>
    ///     Check whether the group has data.
    /// </summary>
    public bool IsFilled { get; }

    /// <summary>
    ///     Bind the vertex array. This must be called before drawing.
    /// </summary>
    public void BindVertexArray();

    /// <summary>
    ///     Draw the group.
    /// </summary>
    public void Draw();
}

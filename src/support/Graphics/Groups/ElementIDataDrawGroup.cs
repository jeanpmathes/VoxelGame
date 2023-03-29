// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Graphics.Groups;

/// <summary>
///     A draw group for meshes that are defined as elements of integer data.
/// </summary>
public class ElementIDataDrawGroup : IDrawGroup
{
    private readonly int ebo;
    private readonly int size;
    private readonly int vao;

    private readonly int vbo;

    private int elementCount;

    private ElementIDataDrawGroup(int size)
    {
        this.size = size;

        // todo: port to DirectX
        // GL.CreateBuffers(n: 1, out vbo);
        // GL.CreateBuffers(n: 1, out ebo);
        // GL.CreateVertexArrays(n: 1, out vao);
    }

    /// <inheritdoc />
    public bool IsFilled { get; private set; }

    /// <inheritdoc />
    public void BindVertexArray()
    {
        // GL.BindVertexArray(vao);
    }

    /// <inheritdoc />
    public void Draw()
    {
        DrawElements();
    }

    /// <summary>
    ///     Create a new <see cref="ElementIDataDrawGroup" />.
    /// </summary>
    /// <param name="size">The size of every element.</param>
    /// <returns>The created group.</returns>
    public static ElementIDataDrawGroup Create(int size)
    {
        return new ElementIDataDrawGroup(size);
    }

    /// <summary>
    ///     Set the data of the group.
    /// </summary>
    /// <param name="vertexDataCount">The number of vertex data entries.</param>
    /// <param name="vertexData">The vertex data. Length must be at least the number of specified entries.</param>
    /// <param name="elementDataCount">The number of element data entries.</param>
    /// <param name="elementData">
    ///     The element data, containing the indices. Length must be at least the number of specified
    ///     entries.
    /// </param>
    public void SetData(int vertexDataCount, int[] vertexData, int elementDataCount, uint[] elementData)
    {
        elementCount = elementDataCount;

        if (elementCount == 0)
        {
            IsFilled = false;

            return;
        }

        IsFilled = true;

        // GL.NamedBufferData(vbo, vertexDataCount * sizeof(int), vertexData, BufferUsageHint.DynamicDraw);
        // GL.NamedBufferData(ebo, elementDataCount * sizeof(uint), elementData, BufferUsageHint.DynamicDraw);
    }

    /// <summary>
    ///     Bind the vertex array.
    /// </summary>
    public void VertexArrayBindBuffer()
    {
        // GL.VertexArrayVertexBuffer(vao, bindingindex: 0, vbo, IntPtr.Zero, size * sizeof(int));
        // GL.VertexArrayElementBuffer(vao, ebo);
    }

    /// <summary>
    ///     Bind the attribute that receives the data.
    /// </summary>
    /// <param name="attribute">The location of the attribute.</param>
    public void VertexArrayAttributeBinding(int attribute)
    {
        // GL.EnableVertexArrayAttrib(vao, attribute);
        // GL.VertexArrayAttribIFormat(vao, attribute, size, VertexAttribType.Int, 0 * sizeof(int));
        // GL.VertexArrayAttribBinding(vao, attribute, bindingindex: 0);
    }

    /// <summary>
    ///     Draw all elements.
    /// </summary>
    public void DrawElements()
    {
        // GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, indices: 0);
    }

    /// <summary>
    ///     Delete all resources.
    /// </summary>
    public void Delete()
    {
        // GL.DeleteBuffer(vbo);
        // GL.DeleteBuffer(ebo);
        // GL.DeleteVertexArray(vao);
    }
}


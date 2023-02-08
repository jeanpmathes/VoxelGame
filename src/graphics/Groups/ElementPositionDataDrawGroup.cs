// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups;

/// <summary>
///     Draw group for meshes defined as elements of positions and integer data.
/// </summary>
public class ElementPositionDataDrawGroup : IDrawGroup
{
    private readonly int dataSize;
    private readonly int dataVBO;
    private readonly int ebo;
    private readonly int positionSize;

    private readonly int positionVBO;
    private readonly int vao;

    private int elementCount;

    private ElementPositionDataDrawGroup(int positionSize, int dataSize)
    {
        this.positionSize = positionSize;
        this.dataSize = dataSize;

        GL.CreateBuffers(n: 1, out positionVBO);
        GL.CreateBuffers(n: 1, out dataVBO);
        GL.CreateBuffers(n: 1, out ebo);
        GL.CreateVertexArrays(n: 1, out vao);
    }

    /// <inheritdoc />
    public bool IsFilled { get; private set; }

    /// <inheritdoc />
    public void BindVertexArray()
    {
        GL.BindVertexArray(vao);
    }

    /// <inheritdoc />
    public void Draw()
    {
        DrawElements();
    }

    /// <summary>
    ///     Create a new <see cref="ElementPositionDataDrawGroup" />.
    /// </summary>
    /// <param name="positionSize">The size of position data units.</param>
    /// <param name="dataSize">The size of data units.</param>
    /// <returns>The created draw group.</returns>
    public static ElementPositionDataDrawGroup Create(int positionSize, int dataSize)
    {
        return new ElementPositionDataDrawGroup(positionSize, dataSize);
    }

    /// <summary>
    ///     Set the data.
    /// </summary>
    /// <param name="positionCount">The number of entries in the position array.</param>
    /// <param name="positions">The position array.</param>
    /// <param name="dataCount">The number of entries in the data array.</param>
    /// <param name="data">The data array.</param>
    /// <param name="indexCount">The number of entries in the index array.</param>
    /// <param name="indices">The index array.</param>
    public void SetData(int positionCount, float[] positions, int dataCount, int[] data, int indexCount,
        uint[] indices)
    {
        elementCount = indexCount;

        if (elementCount == 0)
        {
            IsFilled = false;

            return;
        }

        IsFilled = true;

        GL.NamedBufferData(positionVBO, positionCount * sizeof(float), positions, BufferUsageHint.DynamicDraw);
        GL.NamedBufferData(dataVBO, dataCount * sizeof(int), data, BufferUsageHint.DynamicDraw);
        GL.NamedBufferData(ebo, indexCount * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
    }

    /// <summary>
    ///     Bind the vertex array.
    /// </summary>
    public void VertexArrayBindBuffer()
    {
        GL.VertexArrayVertexBuffer(vao, bindingindex: 0, positionVBO, IntPtr.Zero, positionSize * sizeof(float));
        GL.VertexArrayVertexBuffer(vao, bindingindex: 1, dataVBO, IntPtr.Zero, dataSize * sizeof(int));
        GL.VertexArrayElementBuffer(vao, ebo);
    }

    /// <summary>
    ///     Bind all required vertex attributes.
    /// </summary>
    /// <param name="positionAttribute">The position attribute.</param>
    /// <param name="dataAttribute">The data attribute.</param>
    public void VertexArrayAttributeBinding(int positionAttribute, int dataAttribute)
    {
        GL.EnableVertexArrayAttrib(vao, positionAttribute);
        GL.EnableVertexArrayAttrib(vao, dataAttribute);

        GL.VertexArrayAttribFormat(
            vao,
            positionAttribute,
            positionSize,
            VertexAttribType.Float,
            normalized: false,
            0 * sizeof(float));

        GL.VertexArrayAttribIFormat(vao, dataAttribute, dataSize, VertexAttribType.Int, 0 * sizeof(int));

        GL.VertexArrayAttribBinding(vao, positionAttribute, bindingindex: 0);
        GL.VertexArrayAttribBinding(vao, dataAttribute, bindingindex: 1);
    }

    /// <summary>
    ///     Draw all elements.
    /// </summary>
    public void DrawElements()
    {
        GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, indices: 0);
    }

    /// <summary>
    ///     Delete all used resources.
    /// </summary>
    public void Delete()
    {
        GL.DeleteBuffer(positionVBO);
        GL.DeleteBuffer(dataVBO);
        GL.DeleteBuffer(ebo);
        GL.DeleteVertexArray(vao);
    }
}


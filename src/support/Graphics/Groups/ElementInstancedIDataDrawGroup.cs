// <copyright file="ElementInstancedIDataDrawGroup.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support.Graphics.Groups;

/// <summary>
///     Draw groups for meshes defined as an instanced mesh out elements, with every instance receiving integer data.
/// </summary>
public class ElementInstancedIDataDrawGroup : IDrawGroup
{
    private const int ModelPositionBindingIndex = 0;
    private const int InstanceBindingIndex = 1;

    private readonly int instanceSize;

    private readonly int instanceVBO;
    private readonly int modelEBO;
    private readonly int modelElementCount;

    private readonly int modelPositionVBO;

    private readonly int vao;

    private int instanceCount;

    private ElementInstancedIDataDrawGroup((float[] vertices, uint[] indices) model, int instanceSize)
    {
        this.instanceSize = instanceSize;

        // todo: port to DirectX
        // GL.CreateBuffers(n: 1, out modelPositionVBO);
        // GL.CreateBuffers(n: 1, out modelEBO);
        // GL.CreateBuffers(n: 1, out instanceVBO);
        // GL.CreateVertexArrays(n: 1, out vao);

        modelElementCount = model.indices.Length;
        SetModelData(model);
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
        DrawElementsInstanced();
    }

    /// <summary>
    ///     Create a new <see cref="ElementInstancedIDataDrawGroup" />.
    /// </summary>
    /// <param name="model">The definition of the model.</param>
    /// <param name="instanceSize">The size of the data units every instance receives.</param>
    /// <returns>The created draw group.</returns>
    public static ElementInstancedIDataDrawGroup Create((float[] vertices, uint[] indices) model, int instanceSize)
    {
        return new ElementInstancedIDataDrawGroup(model, instanceSize);
    }

    private void SetModelData((float[] vertices, uint[] indices) model)
    {
        // GL.NamedBufferData(
        //     modelPositionVBO,
        //     model.vertices.Length * sizeof(float),
        //     model.vertices,
        //     BufferUsageHint.StaticDraw);
        //
        // GL.NamedBufferData(
        //     modelEBO,
        //     model.indices.Length * sizeof(uint),
        //     model.indices,
        //     BufferUsageHint.StaticDraw);
    }

    /// <summary>
    ///     Set the data describing the instances.
    /// </summary>
    /// <param name="instanceDataCount">The number of entries in the data array.</param>
    /// <param name="instanceData">The instance data array. Length must be at least as long as the entry count.</param>
    public void SetInstanceData(int instanceDataCount, int[] instanceData)
    {
        instanceCount = instanceDataCount / instanceSize;

        if (instanceCount == 0)
        {
            IsFilled = false;

            return;
        }

        IsFilled = true;

        // GL.NamedBufferData(instanceVBO, instanceDataCount * sizeof(int), instanceData, BufferUsageHint.DynamicDraw);
    }

    /// <summary>
    ///     Bind the vertex array.
    /// </summary>
    /// <param name="modelSize">The size of the vertex data units per vertex.</param>
    public void VertexArrayBindBuffer(int modelSize)
    {
        // GL.VertexArrayVertexBuffer(
        //     vao,
        //     ModelPositionBindingIndex,
        //     modelPositionVBO,
        //     IntPtr.Zero,
        //     modelSize * sizeof(float));
        //
        // GL.VertexArrayElementBuffer(vao, modelEBO);
        // GL.VertexArrayVertexBuffer(vao, InstanceBindingIndex, instanceVBO, IntPtr.Zero, instanceSize * sizeof(int));
    }

    /// <summary>
    ///     Bind an vertex attribute for model data.
    /// </summary>
    /// <param name="modelAttribute">The location of the attribute.</param>
    /// <param name="size">The size of the attribute.</param>
    /// <param name="offset">The offset to the vertex unit start.</param>
    public void VertexArrayModelAttributeBinding(int modelAttribute, int size, int offset)
    {
        // GL.EnableVertexArrayAttrib(vao, modelAttribute);
        //
        // GL.VertexArrayAttribFormat(
        //     vao,
        //     modelAttribute,
        //     size,
        //     VertexAttribType.Float,
        //     normalized: false,
        //     offset * sizeof(float));
        //
        // GL.VertexArrayAttribBinding(vao, modelAttribute, ModelPositionBindingIndex);
    }

    /// <summary>
    ///     Bind the attribute that will receive the instance data.
    /// </summary>
    /// <param name="instanceAttribute">The attribute to bind you.</param>
    public void VertexArrayInstanceAttributeBinding(int instanceAttribute)
    {
        // GL.EnableVertexArrayAttrib(vao, instanceAttribute);
        // GL.VertexArrayAttribIFormat(vao, instanceAttribute, instanceSize, VertexAttribType.Int, 0 * sizeof(int));
        // GL.VertexArrayAttribBinding(vao, instanceAttribute, InstanceBindingIndex);
        // GL.VertexArrayBindingDivisor(vao, InstanceBindingIndex, divisor: 1);
    }

    /// <summary>
    ///     Draw all instances.
    /// </summary>
    public void DrawElementsInstanced()
    {
        // GL.DrawElementsInstanced(
        //     PrimitiveType.Triangles,
        //     modelElementCount,
        //     DrawElementsType.UnsignedInt,
        //     IntPtr.Zero,
        //     instanceCount);
    }

    /// <summary>
    ///     Delete the used resources.
    /// </summary>
    public void Delete()
    {
        // GL.DeleteBuffer(modelPositionVBO);
        // GL.DeleteBuffer(modelEBO);
        // GL.DeleteBuffer(instanceVBO);
        // GL.DeleteVertexArray(vao);
    }
}


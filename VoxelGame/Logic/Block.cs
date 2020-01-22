using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public class Block
    {
        private float[] vertices;

        private uint[] indices =
        {
            // Front face
            0, 2, 1,
            0, 3, 2,

            // Back face
            4, 6, 5,
            4, 7, 6,

            // Left face
            8, 10, 9,
            8, 11, 10,

            // Right face
            12, 14, 13,
            12, 15, 14,

            // Bottom face
            16, 18, 17,
            16, 19, 18,

            // Top face
            20, 22, 21,
            20, 23, 22
        };

        private readonly int vertexBufferObject;
        private readonly int elementBufferObject;
        private readonly int vertexArrayObject;

        private Shader shader;
        private Texture texture;

        public Block(string texturePath, Shader shader, Tuple<int, int, int, int, int, int> sideIndices, Vector4[] sideUVs)
        {
            vertices = new float[]
            {
                // Position | UV Position
                // Front face
                0f, 0f, 0f, sideUVs[sideIndices.Item1].X, sideUVs[sideIndices.Item1].Z,
                0f, 1f, 0f, sideUVs[sideIndices.Item1].X, sideUVs[sideIndices.Item1].W,
                1f, 1f, 0f, sideUVs[sideIndices.Item1].Y, sideUVs[sideIndices.Item1].W,
                1f, 0f, 0f, sideUVs[sideIndices.Item1].Y, sideUVs[sideIndices.Item1].Z,

                // Back face
                1f, 0f, 1f, sideUVs[sideIndices.Item2].X, sideUVs[sideIndices.Item2].Z,
                1f, 1f, 1f, sideUVs[sideIndices.Item2].X, sideUVs[sideIndices.Item2].W,
                0f, 1f, 1f, sideUVs[sideIndices.Item2].Y, sideUVs[sideIndices.Item2].W,
                0f, 0f, 1f, sideUVs[sideIndices.Item2].Y, sideUVs[sideIndices.Item2].Z,

                // Left face
                0f, 0f, 1f, sideUVs[sideIndices.Item3].X, sideUVs[sideIndices.Item3].Z,
                0f, 1f, 1f, sideUVs[sideIndices.Item3].X, sideUVs[sideIndices.Item3].W,
                0f, 1f, 0f, sideUVs[sideIndices.Item3].Y, sideUVs[sideIndices.Item3].W,
                0f, 0f, 0f, sideUVs[sideIndices.Item3].Y, sideUVs[sideIndices.Item3].Z,

                // Right face
                1f, 0f, 0f, sideUVs[sideIndices.Item4].X, sideUVs[sideIndices.Item4].Z,
                1f, 1f, 0f, sideUVs[sideIndices.Item4].X, sideUVs[sideIndices.Item4].W,
                1f, 1f, 1f, sideUVs[sideIndices.Item4].Y, sideUVs[sideIndices.Item4].W,
                1f, 0f, 1f, sideUVs[sideIndices.Item4].Y, sideUVs[sideIndices.Item4].Z,

                // Bottom face
                0f, 0f, 1f, sideUVs[sideIndices.Item5].X, sideUVs[sideIndices.Item5].Z,
                0f, 0f, 0f, sideUVs[sideIndices.Item5].X, sideUVs[sideIndices.Item5].W,
                1f, 0f, 0f, sideUVs[sideIndices.Item5].Y, sideUVs[sideIndices.Item5].W,
                1f, 0f, 1f, sideUVs[sideIndices.Item5].Y, sideUVs[sideIndices.Item5].Z,

                // Top face
                0f, 1f, 0f, sideUVs[sideIndices.Item6].X, sideUVs[sideIndices.Item6].Z,
                0f, 1f, 1f, sideUVs[sideIndices.Item6].X, sideUVs[sideIndices.Item6].W,
                1f, 1f, 1f, sideUVs[sideIndices.Item6].Y, sideUVs[sideIndices.Item6].W,
                1f, 1f, 0f, sideUVs[sideIndices.Item6].Y, sideUVs[sideIndices.Item6].Z
            };

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            this.shader = shader;
            shader.Use();

            texture = new Texture(texturePath);
            texture.Use();

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void RenderBlock(Vector3 position)
        {
            GL.BindVertexArray(vertexArrayObject);

            shader.Use();
            texture.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", Game.mainCamera.GetViewMatrix());
            shader.SetMatrix4("projection", Game.mainCamera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}

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

        private float[][] sideVertices;
        private float[][] sideIndices;

        private readonly int vertexBufferObject;
        private readonly int elementBufferObject;
        private readonly int vertexArrayObject;

        private Shader shader;

        public Block(string name, Shader shader, Tuple<int, int, int, int, int, int> sideIndices)
        {
            int textureIndex = Game.Atlas.GetTextureIndex(name);

            if (textureIndex == -1)
            {
                throw new Exception($"No texture '{name}' found!");
            }

            AtlasPosition[] sideUVs =
            {
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item1),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item2),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item3),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item4),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item5),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item6)
            };

            vertices = new float[]
            {
                // Position | UV Position
                // Front face
                0f, 0f, 0f, sideUVs[0].bottomLeftU, sideUVs[0].bottomLeftV,
                0f, 1f, 0f, sideUVs[0].bottomLeftU, sideUVs[0].topRightV,
                1f, 1f, 0f, sideUVs[0].topRightU, sideUVs[0].topRightV,
                1f, 0f, 0f, sideUVs[0].topRightU, sideUVs[0].bottomLeftV,

                // Back face
                1f, 0f, 1f, sideUVs[1].bottomLeftU, sideUVs[1].bottomLeftV,
                1f, 1f, 1f, sideUVs[1].bottomLeftU, sideUVs[1].topRightV,
                0f, 1f, 1f, sideUVs[1].topRightU, sideUVs[1].topRightV,
                0f, 0f, 1f, sideUVs[1].topRightU, sideUVs[1].bottomLeftV,

                // Left face
                0f, 0f, 1f, sideUVs[2].bottomLeftU, sideUVs[2].bottomLeftV,
                0f, 1f, 1f, sideUVs[2].bottomLeftU, sideUVs[2].topRightV,
                0f, 1f, 0f, sideUVs[2].topRightU, sideUVs[2].topRightV,
                0f, 0f, 0f, sideUVs[2].topRightU, sideUVs[2].bottomLeftV,

                // Right face
                1f, 0f, 0f, sideUVs[3].bottomLeftU, sideUVs[3].bottomLeftV,
                1f, 1f, 0f, sideUVs[3].bottomLeftU, sideUVs[3].topRightV,
                1f, 1f, 1f, sideUVs[3].topRightU, sideUVs[3].topRightV,
                1f, 0f, 1f, sideUVs[3].topRightU, sideUVs[3].bottomLeftV,

                // Bottom face
                0f, 0f, 1f, sideUVs[4].bottomLeftU, sideUVs[4].bottomLeftV,
                0f, 0f, 0f, sideUVs[4].bottomLeftU, sideUVs[4].topRightV,
                1f, 0f, 0f, sideUVs[4].topRightU, sideUVs[4].topRightV,
                1f, 0f, 1f, sideUVs[4].topRightU, sideUVs[4].bottomLeftV,

                // Top face
                0f, 1f, 0f, sideUVs[5].bottomLeftU, sideUVs[5].bottomLeftV,
                0f, 1f, 1f, sideUVs[5].bottomLeftU, sideUVs[5].topRightV,
                1f, 1f, 1f, sideUVs[5].topRightU, sideUVs[5].topRightV,
                1f, 1f, 0f, sideUVs[5].topRightU, sideUVs[5].bottomLeftV
            };

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            this.shader = shader;
            shader.Use();

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
            Game.Atlas.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", Game.MainCamera.GetViewMatrix());
            shader.SetMatrix4("projection", Game.MainCamera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public float[] GetSideVertices(int side, Vector3 position)
        {
            // Should use sideVertices (has to be set in the constructor)
            throw new NotImplementedException();
        }

        public float[] GetSideIndicies(int side, Vector3 position)
        {
            // Should use sideIndicies (has to be set in the constructor)
            throw new NotImplementedException();
        }
    }
}

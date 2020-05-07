// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using VoxelGame.Physics;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public class BoxRenderer : Renderer
    {
        private readonly int vertexBufferObject;
        private readonly int elementBufferObject;
        private readonly int vertexArrayObject;

        private BoundingBox currentBoundingBox;

        private int elements;

        public BoxRenderer()
        {
            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();
        }

        private int BuildMeshData_NonRecursive(BoundingBox boundingBox, out float[] vertices, out uint[] indices)
        {
            Vector3 offset = boundingBox.Center - currentBoundingBox.Center;

            Vector3 min = (-boundingBox.Extents) + offset;
            Vector3 max = boundingBox.Extents + offset;

            vertices = new float[]
            {
                // Bottom
                min.X, min.Y, min.Z,
                max.X, min.Y, min.Z,
                max.X, min.Y, max.Z,
                min.X, min.Y, max.Z,

                // Top
                min.X, max.Y, min.Z,
                max.X, max.Y, min.Z,
                max.X, max.Y, max.Z,
                min.X, max.Y, max.Z
            };

            indices = new uint[]
            {
                // Bottom
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // Top
                4, 5,
                5, 6,
                6, 7,
                7, 4,

                // Connection
                0, 4,
                1, 5,
                2, 6,
                3, 7
            };

            return 24;
        }

        private int BuildMeshData(BoundingBox boundingBox, out float[] vertices, out uint[] indices)
        {
            int elements = BuildMeshData_NonRecursive(boundingBox, out vertices, out indices);

            if (boundingBox.ChildCount == 0)
            {
                return elements;
            }
            else
            {
                for (int i = 0; i < boundingBox.ChildCount; i++)
                {
                    int newElements = BuildMeshData(boundingBox[i], out float[] addVertices, out uint[] addIndices);

                    uint offset = (uint)(elements / 3);
                    for (int j = 0; j < addIndices.Length; j++)
                    {
                        addIndices[j] += offset;
                    }

                    float[] combinedVertices = new float[vertices.Length + addVertices.Length];
                    Array.Copy(vertices, 0, combinedVertices, 0, vertices.Length);
                    Array.Copy(addVertices, 0, combinedVertices, vertices.Length, addVertices.Length);

                    vertices = combinedVertices;

                    uint[] combinedIndices = new uint[indices.Length + addIndices.Length];
                    Array.Copy(indices, 0, combinedIndices, 0, indices.Length);
                    Array.Copy(addIndices, 0, combinedIndices, indices.Length, addIndices.Length);

                    indices = combinedIndices;

                    elements += newElements;
                }

                return elements;
            }
        }

        public void SetBoundingBox(BoundingBox boundingBox)
        {
            if (disposed)
            {
                return;
            }

            if (currentBoundingBox == boundingBox)
            {
                return;
            }

            currentBoundingBox = boundingBox;

            elements = BuildMeshData(boundingBox, out float[] vertices, out uint[] indices);

            // Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            // Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            Game.SelectionShader.Use();

            // Vertex Array Object
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int vertexLocation = Game.SelectionShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            GL.BindVertexArray(vertexArrayObject);

            Game.SelectionShader.Use();
            Game.Atlas.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Game.SelectionShader.SetMatrix4("model", model);
            Game.SelectionShader.SetMatrix4("view", Game.Player.GetViewMatrix());
            Game.SelectionShader.SetMatrix4("projection", Game.Player.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Lines, elements, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                GL.DeleteBuffer(vertexBufferObject);
                GL.DeleteBuffer(elementBufferObject);
                GL.DeleteVertexArray(vertexArrayObject);
            }
            else
            {
                System.Console.ForegroundColor = System.ConsoleColor.Yellow;
                System.Console.WriteLine("WARNING: A renderer has been disposed by GC, without deleting buffers.");
                System.Console.ResetColor();
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}
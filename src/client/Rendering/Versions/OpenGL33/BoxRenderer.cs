// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public class BoxRenderer : Rendering.BoxRenderer
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<BoxRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private BoundingBox currentBoundingBox;

        private int elements;

        public BoxRenderer()
        {
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            vao = GL.GenVertexArray();
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
            int points = BuildMeshData_NonRecursive(boundingBox, out vertices, out indices);

            if (boundingBox.ChildCount == 0)
            {
                return points;
            }
            else
            {
                for (int i = 0; i < boundingBox.ChildCount; i++)
                {
                    int newElements = BuildMeshData(boundingBox[i], out float[] addVertices, out uint[] addIndices);

                    uint offset = (uint)(points / 3);
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

                    points += newElements;
                }

                return points;
            }
        }

        public override void SetBoundingBox(BoundingBox boundingBox)
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            // Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            Client.SelectionShader.Use();

            // Vertex Array Object
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            int vertexLocation = Client.SelectionShader.GetAttribLocation("aPosition");

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

            GL.BindVertexArray(vao);

            Client.SelectionShader.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Client.SelectionShader.SetMatrix4("model", model);
            Client.SelectionShader.SetMatrix4("view", Client.Player.GetViewMatrix());
            Client.SelectionShader.SetMatrix4("projection", Client.Player.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Lines, elements, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(ebo);
                GL.DeleteVertexArray(vao);
            }
            else
            {
                logger.LogWarning(LoggingEvents.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}
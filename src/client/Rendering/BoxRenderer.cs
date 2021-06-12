// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Groups;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public class BoxRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<BoxRenderer>();

        private readonly ElementDrawGroup drawGroup;

        private BoundingBox currentBoundingBox;

        public BoxRenderer()
        {
            drawGroup = ElementDrawGroup.Create();

            Client.SelectionShader.Use();

            drawGroup.VertexArrayBindBuffer(3);

            int vertexLocation = Client.SelectionShader.GetAttributeLocation("aPosition");
            drawGroup.VertexArrayBindAttribute(vertexLocation, 3, 0);
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

            int elementCount = BuildMeshData(currentBoundingBox, boundingBox, out float[] vertices, out uint[] indices);
            drawGroup.SetData(elementCount, vertices.Length, vertices, indices.Length, indices);
        }

        protected int BuildMeshData(BoundingBox currentBoundingBox, BoundingBox boundingBox, out float[] vertices, out uint[] indices)
        {
            int points = BuildMeshData_NonRecursive(currentBoundingBox, boundingBox, out vertices, out indices);

            if (boundingBox.ChildCount == 0)
            {
                return points;
            }
            else
            {
                for (int i = 0; i < boundingBox.ChildCount; i++)
                {
                    int newElements = BuildMeshData(currentBoundingBox, boundingBox[i], out float[] addVertices, out uint[] addIndices);

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

        private static int BuildMeshData_NonRecursive(BoundingBox currentBoundingBox, BoundingBox boundingBox, out float[] vertices, out uint[] indices)
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

        public void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            drawGroup.BindVertexArray();

            Client.SelectionShader.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Client.SelectionShader.SetMatrix4("model", model);
            Client.SelectionShader.SetMatrix4("view", Client.Player.GetViewMatrix());
            Client.SelectionShader.SetMatrix4("projection", Client.Player.GetProjectionMatrix());

            drawGroup.DrawElements(PrimitiveType.Lines);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                drawGroup.Delete();
            }
            else
            {
                Logger.LogWarning(Events.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        ~BoxRenderer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
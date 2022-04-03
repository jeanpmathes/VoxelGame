// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Groups;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    ///     A renderer that renders instances of the <see cref="BoundingVolume" /> struct.
    ///     For this multiple boxes are drawn.
    /// </summary>
    public sealed class BoxRenderer : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<BoxRenderer>();

        private readonly ElementDrawGroup drawGroup;

        private BoundingVolume? currentBoundingVolume;

        /// <summary>
        ///     Create a new <see cref="BoxRenderer" />.
        /// </summary>
        public BoxRenderer()
        {
            drawGroup = ElementDrawGroup.Create();

            Shaders.Selection.Use();

            drawGroup.VertexArrayBindBuffer(size: 3);

            int vertexLocation = Shaders.Selection.GetAttributeLocation("aPosition");
            drawGroup.VertexArrayBindAttribute(vertexLocation, size: 3, offset: 0);
        }

        private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

        /// <summary>
        ///     Set the bounding box to render.
        /// </summary>
        /// <param name="boundingVolume">The bounding box.</param>
        public void SetVolume(BoundingVolume boundingVolume)
        {
            if (disposed) return;

            if (ReferenceEquals(currentBoundingVolume, boundingVolume)) return;

            currentBoundingVolume = boundingVolume;

            int elementCount = BuildMeshData(boundingVolume, out float[] vertices, out uint[] indices);
            drawGroup.SetData(elementCount, vertices.Length, vertices, indices.Length, indices);
        }

        private static int BuildMeshData(BoundingVolume boundingVolume,
            out float[] vertices, out uint[] indices)
        {
            int points = BuildMeshData_NonRecursive(boundingVolume, out vertices, out indices);

            if (boundingVolume.ChildCount == 0) return points;

            for (var i = 0; i < boundingVolume.ChildCount; i++)
            {
                int newElements = BuildMeshData(
                    boundingVolume[i],
                    out float[] addVertices,
                    out uint[] addIndices);

                var offset = (uint) (points / 3);

                for (var j = 0; j < addIndices.Length; j++) addIndices[j] += offset;

                float[] combinedVertices = new float[vertices.Length + addVertices.Length];
                Array.Copy(vertices, sourceIndex: 0, combinedVertices, destinationIndex: 0, vertices.Length);
                Array.Copy(addVertices, sourceIndex: 0, combinedVertices, vertices.Length, addVertices.Length);

                vertices = combinedVertices;

                uint[] combinedIndices = new uint[indices.Length + addIndices.Length];
                Array.Copy(indices, sourceIndex: 0, combinedIndices, destinationIndex: 0, indices.Length);
                Array.Copy(addIndices, sourceIndex: 0, combinedIndices, indices.Length, addIndices.Length);

                indices = combinedIndices;

                points += newElements;
            }

            return points;
        }

        private static int BuildMeshData_NonRecursive(BoundingVolume boundingVolume,
            out float[] vertices, out uint[] indices)
        {
            (float minX, float minY, float minZ) = boundingVolume.Min;
            (float maxX, float maxY, float maxZ) = boundingVolume.Max;

            vertices = new[]
            {
                // Bottom
                minX, minY, minZ,
                maxX, minY, minZ,
                maxX, minY, maxZ,
                minX, minY, maxZ,

                // Top
                minX, maxY, minZ,
                maxX, maxY, minZ,
                maxX, maxY, maxZ,
                minX, maxY, maxZ
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

        /// <summary>
        ///     Draw the bounding box.
        /// </summary>
        /// <param name="position">The position at which the box should be drawn.</param>
        public void Draw(Vector3 position)
        {
            if (disposed) return;

            drawGroup.BindVertexArray();

            Shaders.Selection.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Shaders.Selection.SetMatrix4("model", model);
            Shaders.Selection.SetMatrix4("view", Application.Client.Instance.CurrentGame!.Player.ViewMatrix);

            Shaders.Selection.SetMatrix4(
                "projection",
                Application.Client.Instance.CurrentGame!.Player.ProjectionMatrix);

            drawGroup.DrawElements(PrimitiveType.Lines);

            GL.BindVertexArray(array: 0);
            GL.UseProgram(program: 0);
        }

        #region IDisposable Support

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) drawGroup.Delete();
            else
                logger.LogWarning(
                    Events.UndeletedBuffers,
                    "Renderer disposed by GC without freeing storage");

            disposed = true;
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~BoxRenderer()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Dispose of this renderer.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}

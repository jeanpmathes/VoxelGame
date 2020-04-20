// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Physics;

namespace VoxelGame.Rendering
{
    public class BoxRenderer : Renderer
    {
        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        private BoundingBox currentBoundingBox;

        public BoxRenderer()
        {
            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();
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

            Vector3 min = -currentBoundingBox.Extents;
            Vector3 max = currentBoundingBox.Extents;

            float[] vertices =
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

            uint[] indices =
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

            GL.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support
        bool disposed = false;

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
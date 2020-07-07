// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace VoxelGame.Rendering
{
    public class ScreenElementRenderer : Renderer
    {
        private readonly int vertexBufferObject;
        private readonly int elementBufferObject;
        private readonly int vertexArrayObject;

        private int texUnit;

        public ScreenElementRenderer()
        {
            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();

            float[] vertices = new float[]
            {
                -0.5f, -0.5f, 0.0f, 0f, 0f,
                -0.5f,  0.5f, 0.0f, 0f, 1f,
                 0.5f,  0.5f, 0.0f, 1f, 1f,
                 0.5f, -0.5f, 0.0f, 1f, 0f
            };

            uint[] indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };

            // Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            Game.ScreenElementShader.Use();

            // Vertex Array Object
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int vertexLocation = Game.ScreenElementShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCordLocation = Game.ScreenElementShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCordLocation);
            GL.VertexAttribPointer(texCordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void SetTexture(Texture texture)
        {
            if (disposed)
            {
                return;
            }

            texUnit = texture.TextureUnit - TextureUnit.Texture0;
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(Vector3.Zero) * Matrix4.CreateScale(position.Z);

            GL.BindVertexArray(vertexArrayObject);

            Game.ScreenElementShader.Use();

            Game.ScreenElementShader.SetMatrix4("model", model);
            Game.ScreenElementShader.SetInt("tex", texUnit);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

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
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                System.Console.WriteLine("WARNING: A renderer has been disposed by GC, without deleting buffers.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                System.Console.ResetColor();
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}
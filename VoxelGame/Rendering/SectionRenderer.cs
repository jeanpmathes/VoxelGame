// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Rendering
{
    public class SectionRenderer : Renderer
    {
        private readonly int vertexBufferObject;
        private readonly int textureIndicesBufferObject;
        private readonly int elementBufferObject;
        private readonly int vertexArrayObject;

        private int elements;

        private bool hasData = false;

        public SectionRenderer()
        {
            vertexBufferObject = GL.GenBuffer();
            textureIndicesBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();
        }

        public void SetData(ref float[] verticesData, ref int[] texIndicesData, ref uint[] indicesData)
        {
            if (disposed)
            {
                return;
            }

            if (verticesData == null)
            {
                throw new System.ArgumentNullException(paramName: nameof(verticesData));
            }

            if (texIndicesData == null)
            {
                throw new System.ArgumentNullException(paramName: nameof(texIndicesData));
            }

            if (indicesData == null)
            {
                throw new System.ArgumentNullException(paramName: nameof(indicesData));
            }

            elements = indicesData.Length;

            // Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, verticesData.Length * sizeof(float), verticesData, BufferUsageHint.StaticDraw);

            // Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, textureIndicesBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, texIndicesData.Length * sizeof(int), texIndicesData, BufferUsageHint.StaticDraw);

            // Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicesData.Length * sizeof(uint), indicesData, BufferUsageHint.StaticDraw);

            int vertexLocation = Game.SectionShader.GetAttribLocation("aPosition");
            int texIndexLocation = Game.SectionShader.GetAttribLocation("aTexIndex");
            int texCoordLocation = Game.SectionShader.GetAttribLocation("aTexCoord");

            Game.SectionShader.Use();

            // Vertex Array Object
            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, textureIndicesBufferObject);
            GL.EnableVertexAttribArray(texIndexLocation);
            GL.VertexAttribIPointer(texIndexLocation, 1, VertexAttribIntegerType.Int, 0, System.IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            GL.BindVertexArray(0);

            hasData = true;
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            if (hasData)
            {
                GL.BindVertexArray(vertexArrayObject);

                Game.SectionShader.Use();

                Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
                Game.SectionShader.SetMatrix4("model", model);
                Game.SectionShader.SetMatrix4("view", Game.Player.GetViewMatrix());
                Game.SectionShader.SetMatrix4("projection", Game.Player.GetProjectionMatrix());

                GL.DrawElements(PrimitiveType.Triangles, elements, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(0);
                GL.UseProgram(0);
            }
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
                GL.DeleteBuffer(textureIndicesBufferObject);
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
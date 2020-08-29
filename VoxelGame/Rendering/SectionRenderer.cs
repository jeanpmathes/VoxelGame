// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// A renderer for <see cref="Logic.Section"/>.
    /// </summary>
    public class SectionRenderer : Renderer
    {
        private static readonly ILogger logger = Program.CreateLogger<SectionRenderer>();

        private readonly int simpleDataVBO;
        private readonly int simpleVAO;

        private readonly int complexPositionVBO;
        private readonly int complexDataVBO;
        private readonly int complexEBO;
        private readonly int complexVAO;

        private readonly int liquidDataVBO;
        private readonly int liquidEBO;
        private readonly int liquidVAO;

        private int simpleIndices;
        private int complexElements;
        private int liquidElements;

        private bool hasSimpleData;
        private bool hasComplexData;
        private bool hasLiquidData;

        public SectionRenderer()
        {
            simpleDataVBO = GL.GenBuffer();

            simpleVAO = GL.GenVertexArray();

            complexPositionVBO = GL.GenBuffer();
            complexDataVBO = GL.GenBuffer();
            complexEBO = GL.GenBuffer();

            complexVAO = GL.GenVertexArray();

            liquidDataVBO = GL.GenBuffer();
            liquidEBO = GL.GenBuffer();

            liquidVAO = GL.GenVertexArray();
        }

        public void SetData(ref SectionMeshData meshData)
        {
            if (disposed)
            {
                return;
            }

            #region SIMPLE BUFFER SETUP

            hasSimpleData = false;

            simpleIndices = meshData.simpleVertexData.Count / 2;

            if (simpleIndices != 0)
            {
                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, simpleDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.simpleVertexData.Count * sizeof(int), meshData.simpleVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                int dataLocation = Game.SimpleSectionShader.GetAttribLocation("aData");

                Game.SimpleSectionShader.Use();

                // Vertex Array Object
                GL.BindVertexArray(simpleVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, simpleDataVBO);
                GL.EnableVertexAttribArray(dataLocation);
                GL.VertexAttribIPointer(dataLocation, 2, VertexAttribIntegerType.Int, 2 * sizeof(int), IntPtr.Zero);

                GL.BindVertexArray(0);

                hasSimpleData = true;
            }

            #endregion SIMPLE BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            hasComplexData = false;

            complexElements = meshData.complexIndices.Count;

            if (complexElements != 0)
            {
                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, complexPositionVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.complexVertexPositions.Count * sizeof(float), meshData.complexVertexPositions.ExposeArray(), BufferUsageHint.StaticDraw);

                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, complexDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.complexVertexData.Count * sizeof(int), meshData.complexVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                // Element Buffer Object
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, complexEBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.complexIndices.Count * sizeof(uint), meshData.complexIndices.ExposeArray(), BufferUsageHint.StaticDraw);

                int positionLocation = Game.ComplexSectionShader.GetAttribLocation("aPosition");
                int dataLocation = Game.ComplexSectionShader.GetAttribLocation("aData");

                Game.ComplexSectionShader.Use();

                // Vertex Array Object
                GL.BindVertexArray(complexVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, complexPositionVBO);
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, complexDataVBO);
                GL.EnableVertexAttribArray(dataLocation);
                GL.VertexAttribIPointer(dataLocation, 2, VertexAttribIntegerType.Int, 2 * sizeof(int), IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, complexEBO);

                GL.BindVertexArray(0);

                hasComplexData = true;
            }

            #endregion COMPLEX BUFFER SETUP

            #region LIQUID BUFFER SETUP

            hasLiquidData = false;

            liquidElements = meshData.liquidIndices.Count;

            if (liquidElements != 0)
            {
                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, liquidDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.liquidVertexData.Count * sizeof(int), meshData.liquidVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                // Element Buffer Object
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, liquidEBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.liquidIndices.Count * sizeof(uint), meshData.liquidIndices.ExposeArray(), BufferUsageHint.StaticDraw);

                int dataLocation = Game.LiquidSectionShader.GetAttribLocation("aData");

                Game.LiquidSectionShader.Use();

                // Vertex Array Object
                GL.BindVertexArray(liquidVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, liquidDataVBO);
                GL.EnableVertexAttribArray(dataLocation);
                GL.VertexAttribIPointer(dataLocation, 2, VertexAttribIntegerType.Int, 2 * sizeof(int), IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, liquidEBO);

                GL.BindVertexArray(0);

                hasLiquidData = true;
            }

            #endregion LIQUID BUFFER SETUP

            meshData.ReturnPooled();
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            if (hasSimpleData || hasComplexData || hasLiquidData)
            {
                Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
                Matrix4 view = Game.Player.GetViewMatrix();
                Matrix4 projection = Game.Player.GetProjectionMatrix();

                #region RENDERING SIMPLE

                if (hasSimpleData)
                {
                    GL.BindVertexArray(simpleVAO);

                    Game.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

                    Game.SimpleSectionShader.Use();

                    Game.SimpleSectionShader.SetMatrix4("model", model);
                    Game.SimpleSectionShader.SetMatrix4("view", view);
                    Game.SimpleSectionShader.SetMatrix4("projection", projection);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, simpleIndices);
                }

                #endregion RENDERING SIMPLE

                #region RENDERING COMPLEX

                if (hasComplexData)
                {
                    GL.BindVertexArray(complexVAO);

                    Game.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

                    Game.ComplexSectionShader.Use();

                    Game.ComplexSectionShader.SetMatrix4("model", model);
                    Game.ComplexSectionShader.SetMatrix4("view", view);
                    Game.ComplexSectionShader.SetMatrix4("projection", projection);

                    GL.DrawElements(PrimitiveType.Triangles, complexElements, DrawElementsType.UnsignedInt, 0);
                }

                #endregion RENDERING COMPLEX

                #region RENDERING LIQUID

                if (hasLiquidData)
                {
                    GL.BindVertexArray(liquidVAO);

                    Game.LiquidSectionShader.Use();

                    Game.LiquidSectionShader.SetMatrix4("model", model);
                    Game.LiquidSectionShader.SetMatrix4("view", view);
                    Game.LiquidSectionShader.SetMatrix4("projection", projection);

                    GL.DrawElements(PrimitiveType.Triangles, liquidElements, DrawElementsType.UnsignedInt, 0);
                }

                #endregion RENDERING LIQUID

                GL.BindVertexArray(0);
                GL.UseProgram(0);
            }
        }

        #region IDisposable Support

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                GL.DeleteBuffer(simpleDataVBO);

                GL.DeleteVertexArray(simpleVAO);

                GL.DeleteBuffer(complexPositionVBO);
                GL.DeleteBuffer(complexDataVBO);
                GL.DeleteBuffer(complexEBO);

                GL.DeleteVertexArray(complexVAO);

                GL.DeleteBuffer(liquidDataVBO);
                GL.DeleteBuffer(liquidEBO);

                GL.DeleteVertexArray(liquidVAO);
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
// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Runtime.Intrinsics.X86;

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
            GL.CreateBuffers(1, out simpleDataVBO);
            GL.CreateVertexArrays(1, out simpleVAO);

            GL.CreateBuffers(1, out complexPositionVBO);
            GL.CreateBuffers(1, out complexDataVBO);
            GL.CreateBuffers(1, out complexEBO);
            GL.CreateVertexArrays(1, out complexVAO);

            GL.CreateBuffers(1, out liquidDataVBO);
            GL.CreateBuffers(1, out liquidEBO);
            GL.CreateVertexArrays(1, out liquidVAO);
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
                GL.NamedBufferData(simpleDataVBO, meshData.simpleVertexData.Count * sizeof(int), meshData.simpleVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Game.SimpleSectionShader.GetAttribLocation("aData");

                Game.SimpleSectionShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(simpleVAO, 0, simpleDataVBO, IntPtr.Zero, 2 * sizeof(int));

                GL.EnableVertexArrayAttrib(simpleVAO, dataLocation);
                GL.VertexArrayAttribIFormat(simpleVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));
                GL.VertexArrayAttribBinding(simpleVAO, dataLocation, 0);

                hasSimpleData = true;
            }

            #endregion SIMPLE BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            hasComplexData = false;

            complexElements = meshData.complexIndices.Count;

            if (complexElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(complexPositionVBO, meshData.complexVertexPositions.Count * sizeof(float), meshData.complexVertexPositions.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Vertex Buffer Object
                GL.NamedBufferData(complexDataVBO, meshData.complexVertexData.Count * sizeof(int), meshData.complexVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(complexEBO, meshData.complexIndices.Count * sizeof(uint), meshData.complexIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int positionLocation = Game.ComplexSectionShader.GetAttribLocation("aPosition");
                int dataLocation = Game.ComplexSectionShader.GetAttribLocation("aData");

                Game.ComplexSectionShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(complexVAO, 0, complexPositionVBO, IntPtr.Zero, 3 * sizeof(float));
                GL.VertexArrayVertexBuffer(complexVAO, 1, complexDataVBO, IntPtr.Zero, 2 * sizeof(int));
                GL.VertexArrayElementBuffer(complexVAO, complexEBO);

                GL.EnableVertexArrayAttrib(complexVAO, positionLocation);
                GL.EnableVertexArrayAttrib(complexVAO, dataLocation);

                GL.VertexArrayAttribFormat(complexVAO, positionLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
                GL.VertexArrayAttribIFormat(complexVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));

                GL.VertexArrayAttribBinding(complexVAO, positionLocation, 0);
                GL.VertexArrayAttribBinding(complexVAO, dataLocation, 1);

                hasComplexData = true;
            }

            #endregion COMPLEX BUFFER SETUP

            #region LIQUID BUFFER SETUP

            hasLiquidData = false;

            liquidElements = meshData.liquidIndices.Count;

            if (liquidElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(liquidDataVBO, meshData.liquidVertexData.Count * sizeof(int), meshData.liquidVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(liquidEBO, meshData.liquidIndices.Count * sizeof(uint), meshData.liquidIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Game.LiquidSectionShader.GetAttribLocation("aData");

                Game.LiquidSectionShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(liquidVAO, 0, liquidDataVBO, IntPtr.Zero, 2 * sizeof(int));
                GL.VertexArrayElementBuffer(liquidVAO, liquidEBO);

                GL.EnableVertexArrayAttrib(liquidVAO, dataLocation);
                GL.VertexArrayAttribIFormat(liquidVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));
                GL.VertexArrayAttribBinding(liquidVAO, dataLocation, 0);

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
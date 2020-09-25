// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Diagnostics;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    /// <summary>
    /// A renderer for <see cref="Logic.Section"/>.
    /// </summary>
    public class SectionRenderer : Rendering.SectionRenderer
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();

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

        public override void SetData(ref SectionMeshData meshData)
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

                int dataLocation = Client.SimpleSectionShader.GetAttribLocation("aData");

                Client.SimpleSectionShader.Use();

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

                int positionLocation = Client.ComplexSectionShader.GetAttribLocation("aPosition");
                int dataLocation = Client.ComplexSectionShader.GetAttribLocation("aData");

                Client.ComplexSectionShader.Use();

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

                int dataLocation = Client.LiquidSectionShader.GetAttribLocation("aData");

                Client.LiquidSectionShader.Use();

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
            Debug.Fail("Sections should be drawn using DrawStage.");

            for (int stage = 0; stage < 3; stage++)
            {
                PrepareStage(0);
                DrawStage(0, position);
                FinishStage(0);
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public override void PrepareStage(int stage)
        {
            Matrix4 view = Client.Player.GetViewMatrix();
            Matrix4 projection = Client.Player.GetProjectionMatrix();

            switch (stage)
            {
                case 0: PrepareSimpleBuffer(view, projection); break;
                case 1: PrepareComplexBuffer(view, projection); break;
                case 2: PrepareLiquidBuffer(view, projection); break;
            }
        }

        public override void DrawStage(int stage, Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);

            switch (stage)
            {
                case 0: DrawSimpleBuffer(model); break;
                case 1: DrawComplexBuffer(model); break;
                case 2: DrawLiquidBuffer(model); break;
            }
        }

        public override void FinishStage(int stage)
        {
            switch (stage)
            {
                // case 2:
                // case 1:
                case 2: FinishLiqduiBuffer(); break;
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.SimpleSectionShader.Use();

            Client.SimpleSectionShader.SetMatrix4("view", view);
            Client.SimpleSectionShader.SetMatrix4("projection", projection);

            Client.SimpleSectionShader.SetInt("firstArrayTexture", 1);
            Client.SimpleSectionShader.SetInt("secondArrayTexture", 2);
            Client.SimpleSectionShader.SetInt("thirdArrayTexture", 3);
            Client.SimpleSectionShader.SetInt("fourthArrayTexture", 4);
        }

        private void DrawSimpleBuffer(Matrix4 model)
        {
            if (hasSimpleData)
            {
                GL.BindVertexArray(simpleVAO);

                Client.SimpleSectionShader.SetMatrix4("model", model);

                GL.DrawArrays(PrimitiveType.Triangles, 0, simpleIndices);
            }
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            Client.ComplexSectionShader.Use();

            Client.ComplexSectionShader.SetMatrix4("view", view);
            Client.ComplexSectionShader.SetMatrix4("projection", projection);

            Client.ComplexSectionShader.SetInt("firstArrayTexture", 1);
            Client.ComplexSectionShader.SetInt("secondArrayTexture", 2);
            Client.ComplexSectionShader.SetInt("thirdArrayTexture", 3);
            Client.ComplexSectionShader.SetInt("fourthArrayTexture", 4);
        }

        private void DrawComplexBuffer(Matrix4 model)
        {
            if (hasComplexData)
            {
                GL.BindVertexArray(complexVAO);

                Client.ComplexSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, complexElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        private static void PrepareLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Client.LiquidSectionShader.Use();

            Client.LiquidSectionShader.SetMatrix4("view", view);
            Client.LiquidSectionShader.SetMatrix4("projection", projection);

            Client.LiquidSectionShader.SetInt("arrayTexture", 5);
        }

        private void DrawLiquidBuffer(Matrix4 model)
        {
            if (hasLiquidData)
            {
                GL.BindVertexArray(liquidVAO);

                Client.LiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, liquidElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        private static void FinishLiqduiBuffer()
        {
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
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
// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering.Versions.OpenGL46
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

        private readonly int opaqueLiquidDataVBO;
        private readonly int opaqueLiquidEBO;
        private readonly int opaqueLiquidVAO;

        private readonly int transparentLiquidDataVBO;
        private readonly int transparentLiquidEBO;
        private readonly int transparentLiquidVAO;

        private int simpleIndices;
        private int complexElements;
        private int opaqueLiquidElements;
        private int transparentLiquidElements;

        private bool hasSimpleData;
        private bool hasComplexData;
        private bool hasOpaqueLiquidData;
        private bool hasTransparentLiquidData;

        public SectionRenderer()
        {
            GL.CreateBuffers(1, out simpleDataVBO);
            GL.CreateVertexArrays(1, out simpleVAO);

            GL.CreateBuffers(1, out complexPositionVBO);
            GL.CreateBuffers(1, out complexDataVBO);
            GL.CreateBuffers(1, out complexEBO);
            GL.CreateVertexArrays(1, out complexVAO);

            GL.CreateBuffers(1, out opaqueLiquidDataVBO);
            GL.CreateBuffers(1, out opaqueLiquidEBO);
            GL.CreateVertexArrays(1, out opaqueLiquidVAO);

            GL.CreateBuffers(1, out transparentLiquidDataVBO);
            GL.CreateBuffers(1, out transparentLiquidEBO);
            GL.CreateVertexArrays(1, out transparentLiquidVAO);
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

            #region LIQUID BUFFERS SETUP

            hasOpaqueLiquidData = false;

            opaqueLiquidElements = meshData.opaqueLiquidIndices.Count;

            if (opaqueLiquidElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(opaqueLiquidDataVBO, meshData.opaqueLiquidVertexData.Count * sizeof(int), meshData.opaqueLiquidVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(opaqueLiquidEBO, meshData.opaqueLiquidIndices.Count * sizeof(uint), meshData.opaqueLiquidIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Client.LiquidSectionShader.GetAttribLocation("aData");

                Client.LiquidSectionShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(opaqueLiquidVAO, 0, opaqueLiquidDataVBO, IntPtr.Zero, 2 * sizeof(int));
                GL.VertexArrayElementBuffer(opaqueLiquidVAO, opaqueLiquidEBO);

                GL.EnableVertexArrayAttrib(opaqueLiquidVAO, dataLocation);
                GL.VertexArrayAttribIFormat(opaqueLiquidVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));
                GL.VertexArrayAttribBinding(opaqueLiquidVAO, dataLocation, 0);

                hasOpaqueLiquidData = true;
            }

            hasTransparentLiquidData = false;

            transparentLiquidElements = meshData.transparentLiquidIndices.Count;

            if (transparentLiquidElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(transparentLiquidDataVBO, meshData.transparentLiquidVertexData.Count * sizeof(int), meshData.transparentLiquidVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(transparentLiquidEBO, meshData.transparentLiquidIndices.Count * sizeof(uint), meshData.transparentLiquidIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Client.LiquidSectionShader.GetAttribLocation("aData");

                Client.LiquidSectionShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(transparentLiquidVAO, 0, transparentLiquidDataVBO, IntPtr.Zero, 2 * sizeof(int));
                GL.VertexArrayElementBuffer(transparentLiquidVAO, transparentLiquidEBO);

                GL.EnableVertexArrayAttrib(transparentLiquidVAO, dataLocation);
                GL.VertexArrayAttribIFormat(transparentLiquidVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));
                GL.VertexArrayAttribBinding(transparentLiquidVAO, dataLocation, 0);

                hasTransparentLiquidData = true;
            }

            #endregion LIQUID BUFFERS SETUP

            meshData.ReturnPooled();
        }

        protected override void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.SimpleSectionShader.Use();

            Client.SimpleSectionShader.SetMatrix4("view", view);
            Client.SimpleSectionShader.SetMatrix4("projection", projection);
        }

        protected override void DrawSimpleBuffer(Matrix4 model)
        {
            if (hasSimpleData)
            {
                GL.BindVertexArray(simpleVAO);

                Client.SimpleSectionShader.SetMatrix4("model", model);

                GL.DrawArrays(PrimitiveType.Triangles, 0, simpleIndices);
            }
        }

        protected override void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            Client.ComplexSectionShader.Use();

            Client.ComplexSectionShader.SetMatrix4("view", view);
            Client.ComplexSectionShader.SetMatrix4("projection", projection);
        }

        protected override void DrawComplexBuffer(Matrix4 model)
        {
            if (hasComplexData)
            {
                GL.BindVertexArray(complexVAO);

                Client.ComplexSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, complexElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        protected override void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.LiquidSectionShader.Use();

            Client.LiquidSectionShader.SetMatrix4("view", view);
            Client.LiquidSectionShader.SetMatrix4("projection", projection);
        }

        protected override void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (hasOpaqueLiquidData)
            {
                GL.BindVertexArray(opaqueLiquidVAO);

                Client.LiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, opaqueLiquidElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        protected override void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Client.LiquidSectionShader.Use();

            Client.LiquidSectionShader.SetMatrix4("view", view);
            Client.LiquidSectionShader.SetMatrix4("projection", projection);
        }

        protected override void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (hasTransparentLiquidData)
            {
                GL.BindVertexArray(transparentLiquidVAO);

                Client.LiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, transparentLiquidElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        protected override void FinishTransparentLiquidBuffer()
        {
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }

        #region IDisposable Support

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

                GL.DeleteBuffer(opaqueLiquidDataVBO);
                GL.DeleteBuffer(opaqueLiquidEBO);
                GL.DeleteVertexArray(opaqueLiquidVAO);

                GL.DeleteBuffer(transparentLiquidDataVBO);
                GL.DeleteBuffer(transparentLiquidEBO);
                GL.DeleteVertexArray(transparentLiquidVAO);
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
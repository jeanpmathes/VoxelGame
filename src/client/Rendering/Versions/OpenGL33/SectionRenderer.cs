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
            simpleDataVBO = GL.GenBuffer();
            simpleVAO = GL.GenVertexArray();

            complexPositionVBO = GL.GenBuffer();
            complexDataVBO = GL.GenBuffer();
            complexEBO = GL.GenBuffer();
            complexVAO = GL.GenVertexArray();

            opaqueLiquidDataVBO = GL.GenBuffer();
            opaqueLiquidEBO = GL.GenBuffer();
            opaqueLiquidVAO = GL.GenVertexArray();

            transparentLiquidDataVBO = GL.GenBuffer();
            transparentLiquidEBO = GL.GenBuffer();
            transparentLiquidVAO = GL.GenVertexArray();
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
                GL.BindBuffer(BufferTarget.ArrayBuffer, simpleDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.simpleVertexData.Count * sizeof(int), meshData.simpleVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                int dataLocation = Client.SimpleSectionShader.GetAttribLocation("aData");

                Client.SimpleSectionShader.Use();

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

                int positionLocation = Client.ComplexSectionShader.GetAttribLocation("aPosition");
                int dataLocation = Client.ComplexSectionShader.GetAttribLocation("aData");

                Client.ComplexSectionShader.Use();

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

            #region VARYING HEIGHT BUFFER SETUP

            // todo

            #endregion VARYING HEIGHT BUFFER SETUP

            #region LIQUID BUFFERS SETUP

            hasOpaqueLiquidData = false;

            opaqueLiquidElements = meshData.opaqueLiquidIndices.Count;

            if (opaqueLiquidElements != 0)
            {
                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, opaqueLiquidDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.opaqueLiquidVertexData.Count * sizeof(int), meshData.opaqueLiquidVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                // Element Buffer Object
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, opaqueLiquidEBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.opaqueLiquidIndices.Count * sizeof(uint), meshData.opaqueLiquidIndices.ExposeArray(), BufferUsageHint.StaticDraw);

                int dataLocation = Client.OpaqueLiquidSectionShader.GetAttribLocation("aData");

                Client.OpaqueLiquidSectionShader.Use();

                // Vertex Array Object
                GL.BindVertexArray(opaqueLiquidVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, opaqueLiquidDataVBO);
                GL.EnableVertexAttribArray(dataLocation);
                GL.VertexAttribIPointer(dataLocation, 2, VertexAttribIntegerType.Int, 2 * sizeof(int), IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, opaqueLiquidEBO);

                GL.BindVertexArray(0);

                hasOpaqueLiquidData = true;
            }

            hasTransparentLiquidData = false;

            transparentLiquidElements = meshData.transparentLiquidIndices.Count;

            if (transparentLiquidElements != 0)
            {
                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, transparentLiquidDataVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, meshData.transparentLiquidVertexData.Count * sizeof(int), meshData.transparentLiquidVertexData.ExposeArray(), BufferUsageHint.StaticDraw);

                // Element Buffer Object
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, transparentLiquidEBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.transparentLiquidIndices.Count * sizeof(uint), meshData.transparentLiquidIndices.ExposeArray(), BufferUsageHint.StaticDraw);

                int dataLocation = Client.TransparentLiquidSectionShader.GetAttribLocation("aData");

                Client.TransparentLiquidSectionShader.Use();

                // Vertex Array Object
                GL.BindVertexArray(transparentLiquidVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, transparentLiquidDataVBO);
                GL.EnableVertexAttribArray(dataLocation);
                GL.VertexAttribIPointer(dataLocation, 2, VertexAttribIntegerType.Int, 2 * sizeof(int), IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, transparentLiquidEBO);

                GL.BindVertexArray(0);

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

            Client.SimpleSectionShader.SetInt("firstArrayTexture", 1);
            Client.SimpleSectionShader.SetInt("secondArrayTexture", 2);
            Client.SimpleSectionShader.SetInt("thirdArrayTexture", 3);
            Client.SimpleSectionShader.SetInt("fourthArrayTexture", 4);
        }

        protected override void DrawSimpleBuffer(Matrix4 model)
        {
            if (hasSimpleData)
            {
                GL.BindVertexArray(simpleVAO);

                Client.SimpleSectionShader.SetMatrix4("model", model);

                GL.DrawArrays(PrimitiveType.Triangles, 0, simpleIndices);

                GL.BindVertexArray(0);
            }
        }

        protected override void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
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

        protected override void DrawComplexBuffer(Matrix4 model)
        {
            if (hasComplexData)
            {
                GL.BindVertexArray(complexVAO);

                Client.ComplexSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, complexElements, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(0);
            }
        }

        protected override void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            throw new NotImplementedException();
        }

        protected override void DrawVaryingHeightBuffer(Matrix4 model)
        {
            throw new NotImplementedException();
        }

        protected override void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.OpaqueLiquidSectionShader.Use();

            Client.OpaqueLiquidSectionShader.SetMatrix4("view", view);
            Client.OpaqueLiquidSectionShader.SetMatrix4("projection", projection);

            Client.OpaqueLiquidSectionShader.SetInt("arrayTexture", 5);
        }

        protected override void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (hasOpaqueLiquidData)
            {
                GL.BindVertexArray(opaqueLiquidVAO);

                Client.OpaqueLiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, opaqueLiquidElements, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(0);
            }
        }

        protected override void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Rendering.Screen.FillDepthTexture();

            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Client.TransparentLiquidSectionShader.Use();

            Client.TransparentLiquidSectionShader.SetMatrix4("view", view);
            Client.TransparentLiquidSectionShader.SetMatrix4("projection", projection);

            Client.TransparentLiquidSectionShader.SetInt("arrayTexture", 5);
            Client.TransparentLiquidSectionShader.SetInt("depthTex", 20);
        }

        protected override void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (hasTransparentLiquidData)
            {
                GL.BindVertexArray(transparentLiquidVAO);

                Client.TransparentLiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, transparentLiquidElements, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(0);
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
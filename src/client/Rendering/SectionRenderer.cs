﻿// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer for <see cref="Logic.Section"/>.
    /// </summary>
    public class SectionRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<SectionRenderer>();

        public const int DrawStageCount = 5;

        private readonly int simpleDataVBO;
        private readonly int simpleVAO;

        private readonly int complexPositionVBO;
        private readonly int complexDataVBO;
        private readonly int complexEBO;
        private readonly int complexVAO;

        private readonly int varyingHeightDataVBO;
        private readonly int varyingHeightEBO;
        private readonly int varyingHeightVAO;

        private readonly int opaqueLiquidDataVBO;
        private readonly int opaqueLiquidEBO;
        private readonly int opaqueLiquidVAO;

        private readonly int transparentLiquidDataVBO;
        private readonly int transparentLiquidEBO;
        private readonly int transparentLiquidVAO;

        private int simpleIndices;
        private int complexElements;
        private int varyingHeightElements;
        private int opaqueLiquidElements;
        private int transparentLiquidElements;

        private bool hasSimpleData;
        private bool hasComplexData;
        private bool hasVaryingHeightData;
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

            GL.CreateBuffers(1, out varyingHeightDataVBO);
            GL.CreateBuffers(1, out varyingHeightEBO);
            GL.CreateVertexArrays(1, out varyingHeightVAO);

            GL.CreateBuffers(1, out opaqueLiquidDataVBO);
            GL.CreateBuffers(1, out opaqueLiquidEBO);
            GL.CreateVertexArrays(1, out opaqueLiquidVAO);

            GL.CreateBuffers(1, out transparentLiquidDataVBO);
            GL.CreateBuffers(1, out transparentLiquidEBO);
            GL.CreateVertexArrays(1, out transparentLiquidVAO);
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

                int dataLocation = Client.SimpleSectionShader.GetAttributeLocation("aData");

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

                int positionLocation = Client.ComplexSectionShader.GetAttributeLocation("aPosition");
                int dataLocation = Client.ComplexSectionShader.GetAttributeLocation("aData");

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

            #region VARYING HEIGHT BUFFER SETUP

            hasVaryingHeightData = false;

            varyingHeightElements = meshData.varyingHeightIndices.Count;

            if (varyingHeightElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(varyingHeightDataVBO, meshData.varyingHeightVertexData.Count * sizeof(int), meshData.varyingHeightVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(varyingHeightEBO, meshData.varyingHeightIndices.Count * sizeof(uint), meshData.varyingHeightIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Client.VaryingHeightShader.GetAttributeLocation("aData");

                Client.VaryingHeightShader.Use();

                // Vertex Array Object
                GL.VertexArrayVertexBuffer(varyingHeightVAO, 0, varyingHeightDataVBO, IntPtr.Zero, 2 * sizeof(int));
                GL.VertexArrayElementBuffer(varyingHeightVAO, varyingHeightEBO);

                GL.EnableVertexArrayAttrib(varyingHeightVAO, dataLocation);
                GL.VertexArrayAttribIFormat(varyingHeightVAO, dataLocation, 2, VertexAttribType.Int, 0 * sizeof(int));
                GL.VertexArrayAttribBinding(varyingHeightVAO, dataLocation, 0);

                hasVaryingHeightData = true;
            }

            #endregion VARYING HEIGHT BUFFER SETUP

            #region LIQUID BUFFERS SETUP

            hasOpaqueLiquidData = false;

            opaqueLiquidElements = meshData.opaqueLiquidIndices.Count;

            if (opaqueLiquidElements != 0)
            {
                // Vertex Buffer Object
                GL.NamedBufferData(opaqueLiquidDataVBO, meshData.opaqueLiquidVertexData.Count * sizeof(int), meshData.opaqueLiquidVertexData.ExposeArray(), BufferUsageHint.DynamicDraw);

                // Element Buffer Object
                GL.NamedBufferData(opaqueLiquidEBO, meshData.opaqueLiquidIndices.Count * sizeof(uint), meshData.opaqueLiquidIndices.ExposeArray(), BufferUsageHint.DynamicDraw);

                int dataLocation = Client.OpaqueLiquidSectionShader.GetAttributeLocation("aData");

                Client.OpaqueLiquidSectionShader.Use();

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

                int dataLocation = Client.TransparentLiquidSectionShader.GetAttributeLocation("aData");

                Client.TransparentLiquidSectionShader.Use();

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

        public static void PrepareStage(int stage)
        {
            Matrix4 view = Client.Player.GetViewMatrix();
            Matrix4 projection = Client.Player.GetProjectionMatrix();

            switch (stage)
            {
                case 0: PrepareSimpleBuffer(view, projection); break;
                case 1: PrepareComplexBuffer(view, projection); break;
                case 2: PrepareVaryingHeightBuffer(view, projection); break;
                case 3: PrepareOpaqueLiquidBuffer(view, projection); break;
                case 4: PrepareTransparentLiquidBuffer(view, projection); break;
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.SimpleSectionShader.Use();

            Client.SimpleSectionShader.SetMatrix4("view", view);
            Client.SimpleSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            Client.ComplexSectionShader.Use();

            Client.ComplexSectionShader.SetMatrix4("view", view);
            Client.ComplexSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.VaryingHeightShader.Use();

            Client.VaryingHeightShader.SetMatrix4("view", view);
            Client.VaryingHeightShader.SetMatrix4("projection", projection);
        }

        private static void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Client.OpaqueLiquidSectionShader.Use();

            Client.OpaqueLiquidSectionShader.SetMatrix4("view", view);
            Client.OpaqueLiquidSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Screen.FillDepthTexture();

            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Client.TransparentLiquidSectionShader.Use();

            Client.TransparentLiquidSectionShader.SetMatrix4("view", view);
            Client.TransparentLiquidSectionShader.SetMatrix4("projection", projection);
        }

        public void DrawStage(int stage, Vector3 position)
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
                case 2: DrawVaryingHeightBuffer(model); break;
                case 3: DrawOpaqueLiquidBuffer(model); break;
                case 4: DrawTransparentLiquidBuffer(model); break;
            }
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

        private void DrawComplexBuffer(Matrix4 model)
        {
            if (hasComplexData)
            {
                GL.BindVertexArray(complexVAO);

                Client.ComplexSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, complexElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void DrawVaryingHeightBuffer(Matrix4 model)
        {
            if (hasVaryingHeightData)
            {
                GL.BindVertexArray(varyingHeightVAO);

                Client.VaryingHeightShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, varyingHeightElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (hasOpaqueLiquidData)
            {
                GL.BindVertexArray(opaqueLiquidVAO);

                Client.OpaqueLiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, opaqueLiquidElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (hasTransparentLiquidData)
            {
                GL.BindVertexArray(transparentLiquidVAO);

                Client.TransparentLiquidSectionShader.SetMatrix4("model", model);

                GL.DrawElements(PrimitiveType.Triangles, transparentLiquidElements, DrawElementsType.UnsignedInt, 0);
            }
        }

        public static void FinishStage(int stage)
        {
            switch (stage)
            {
                case 4: FinishTransparentLiquidBuffer(); break;
            }
        }

        private static void FinishTransparentLiquidBuffer()
        {
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
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

                GL.DeleteBuffer(varyingHeightDataVBO);
                GL.DeleteBuffer(varyingHeightEBO);
                GL.DeleteVertexArray(varyingHeightVAO);

                GL.DeleteBuffer(opaqueLiquidDataVBO);
                GL.DeleteBuffer(opaqueLiquidEBO);
                GL.DeleteVertexArray(opaqueLiquidVAO);

                GL.DeleteBuffer(transparentLiquidDataVBO);
                GL.DeleteBuffer(transparentLiquidEBO);
                GL.DeleteVertexArray(transparentLiquidVAO);
            }
            else
            {
                Logger.LogWarning(Events.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        ~SectionRenderer()
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
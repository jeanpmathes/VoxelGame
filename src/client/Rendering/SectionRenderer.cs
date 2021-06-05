// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Graphics.Groups;
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

        private readonly ElementIDataDrawGroup varyingHeightDrawGroup;
        private readonly ElementIDataDrawGroup opaqueLiquidDrawGroup;
        private readonly ElementIDataDrawGroup transparentLiquidDrawGroup;

        private int simpleIndices;
        private int complexElements;

        private bool hasSimpleData;
        private bool hasComplexData;

        public SectionRenderer()
        {
            GL.CreateBuffers(1, out simpleDataVBO);
            GL.CreateVertexArrays(1, out simpleVAO);

            GL.CreateBuffers(1, out complexPositionVBO);
            GL.CreateBuffers(1, out complexDataVBO);
            GL.CreateBuffers(1, out complexEBO);
            GL.CreateVertexArrays(1, out complexVAO);

            varyingHeightDrawGroup = ElementIDataDrawGroup.Create();
            opaqueLiquidDrawGroup = ElementIDataDrawGroup.Create();
            transparentLiquidDrawGroup = ElementIDataDrawGroup.Create();
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

            varyingHeightDrawGroup.SetData(
                meshData.varyingHeightVertexData.Count, meshData.varyingHeightVertexData.ExposeArray(),
                meshData.varyingHeightIndices.Count, meshData.varyingHeightIndices.ExposeArray());

            if (varyingHeightDrawGroup.IsFilled)
            {
                const int size = 2;

                varyingHeightDrawGroup.VertexArrayBindBuffer(size);

                int dataLocation = Client.VaryingHeightShader.GetAttributeLocation("aData");
                Client.VaryingHeightShader.Use();

                varyingHeightDrawGroup.VertexArrayAttributeBinding(dataLocation, size);
            }

            #endregion VARYING HEIGHT BUFFER SETUP

            #region OPAQUE LIQUID BUFFER SETUP

            opaqueLiquidDrawGroup.SetData(
                meshData.opaqueLiquidVertexData.Count, meshData.opaqueLiquidVertexData.ExposeArray(),
                meshData.opaqueLiquidIndices.Count, meshData.opaqueLiquidIndices.ExposeArray());

            if (opaqueLiquidDrawGroup.IsFilled)
            {
                const int size = 2;

                opaqueLiquidDrawGroup.VertexArrayBindBuffer(size);

                int dataLocation = Client.OpaqueLiquidSectionShader.GetAttributeLocation("aData");
                Client.OpaqueLiquidSectionShader.Use();

                opaqueLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation, size);
            }

            #endregion OPAQUE LIQUID BUFFER SETUP

            #region TRANSPARENT LIQUID BUFFER SETUP

            transparentLiquidDrawGroup.SetData(
                meshData.transparentLiquidVertexData.Count, meshData.transparentLiquidVertexData.ExposeArray(),
                meshData.transparentLiquidIndices.Count, meshData.transparentLiquidIndices.ExposeArray());

            if (transparentLiquidDrawGroup.IsFilled)
            {
                const int size = 2;

                transparentLiquidDrawGroup.VertexArrayBindBuffer(size);

                int dataLocation = Client.TransparentLiquidSectionShader.GetAttributeLocation("aData");
                Client.TransparentLiquidSectionShader.Use();

                transparentLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation, size);
            }

            #endregion TRANSPARENT LIQUID BUFFER SETUP

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
            if (!varyingHeightDrawGroup.IsFilled) return;

            varyingHeightDrawGroup.BindVertexArray();
            Client.VaryingHeightShader.SetMatrix4("model", model);
            varyingHeightDrawGroup.DrawElements();
        }

        private void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (!opaqueLiquidDrawGroup.IsFilled) return;

            opaqueLiquidDrawGroup.BindVertexArray();
            Client.OpaqueLiquidSectionShader.SetMatrix4("model", model);
            opaqueLiquidDrawGroup.DrawElements();
        }

        private void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (!transparentLiquidDrawGroup.IsFilled) return;

            transparentLiquidDrawGroup.BindVertexArray();
            Client.TransparentLiquidSectionShader.SetMatrix4("model", model);
            transparentLiquidDrawGroup.DrawElements();
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

                varyingHeightDrawGroup.Delete();
                opaqueLiquidDrawGroup.Delete();
                transparentLiquidDrawGroup.Delete();
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
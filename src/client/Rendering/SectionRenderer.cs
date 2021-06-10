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
    /// A renderer for <see cref="VoxelGame.Core.Logic.Section"/>.
    /// </summary>
    public class SectionRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<SectionRenderer>();

        public const int DrawStageCount = 5;

        private readonly ArrayIDataDrawGroup simpleDrawGroup;

        private readonly ElementPositionDataDrawGroup complexDrawGroup;

        private readonly ElementIDataDrawGroup varyingHeightDrawGroup;
        private readonly ElementIDataDrawGroup opaqueLiquidDrawGroup;
        private readonly ElementIDataDrawGroup transparentLiquidDrawGroup;

        public SectionRenderer()
        {
            simpleDrawGroup = ArrayIDataDrawGroup.Create(2);
            complexDrawGroup = ElementPositionDataDrawGroup.Create(3, 2);
            varyingHeightDrawGroup = ElementIDataDrawGroup.Create(2);
            opaqueLiquidDrawGroup = ElementIDataDrawGroup.Create(2);
            transparentLiquidDrawGroup = ElementIDataDrawGroup.Create(2);
        }

        public void SetData(ref SectionMeshData meshData)
        {
            if (disposed)
            {
                return;
            }

            #region SIMPLE BUFFER SETUP

            simpleDrawGroup.SetData(meshData.simpleVertexData.Count, meshData.simpleVertexData.ExposeArray());

            if (simpleDrawGroup.IsFilled)
            {
                simpleDrawGroup.VertexArrayBindBuffer();

                int dataLocation = Client.SimpleSectionShader.GetAttributeLocation("aData");
                Client.SimpleSectionShader.Use();

                simpleDrawGroup.VertexArrayAttributeBinding(dataLocation);
            }

            #endregion SIMPLE BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            complexDrawGroup.SetData(meshData.complexVertexPositions.Count, meshData.complexVertexPositions.ExposeArray(),
                meshData.complexVertexData.Count, meshData.complexVertexData.ExposeArray(),
                meshData.complexIndices.Count, meshData.complexIndices.ExposeArray());

            if (complexDrawGroup.IsFilled)
            {
                complexDrawGroup.VertexArrayBindBuffer();

                int positionLocation = Client.ComplexSectionShader.GetAttributeLocation("aPosition");
                int dataLocation = Client.ComplexSectionShader.GetAttributeLocation("aData");

                Client.ComplexSectionShader.Use();

                complexDrawGroup.VertexArrayAttributeBinding(positionLocation, dataLocation);
            }

            #endregion COMPLEX BUFFER SETUP

            #region VARYING HEIGHT BUFFER SETUP

            varyingHeightDrawGroup.SetData(
                meshData.varyingHeightVertexData.Count, meshData.varyingHeightVertexData.ExposeArray(),
                meshData.varyingHeightIndices.Count, meshData.varyingHeightIndices.ExposeArray());

            if (varyingHeightDrawGroup.IsFilled)
            {
                varyingHeightDrawGroup.VertexArrayBindBuffer();

                int dataLocation = Client.VaryingHeightShader.GetAttributeLocation("aData");
                Client.VaryingHeightShader.Use();

                varyingHeightDrawGroup.VertexArrayAttributeBinding(dataLocation);
            }

            #endregion VARYING HEIGHT BUFFER SETUP

            #region OPAQUE LIQUID BUFFER SETUP

            opaqueLiquidDrawGroup.SetData(
                meshData.opaqueLiquidVertexData.Count, meshData.opaqueLiquidVertexData.ExposeArray(),
                meshData.opaqueLiquidIndices.Count, meshData.opaqueLiquidIndices.ExposeArray());

            if (opaqueLiquidDrawGroup.IsFilled)
            {
                opaqueLiquidDrawGroup.VertexArrayBindBuffer();

                int dataLocation = Client.OpaqueLiquidSectionShader.GetAttributeLocation("aData");
                Client.OpaqueLiquidSectionShader.Use();

                opaqueLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);
            }

            #endregion OPAQUE LIQUID BUFFER SETUP

            #region TRANSPARENT LIQUID BUFFER SETUP

            transparentLiquidDrawGroup.SetData(
                meshData.transparentLiquidVertexData.Count, meshData.transparentLiquidVertexData.ExposeArray(),
                meshData.transparentLiquidIndices.Count, meshData.transparentLiquidIndices.ExposeArray());

            if (transparentLiquidDrawGroup.IsFilled)
            {
                transparentLiquidDrawGroup.VertexArrayBindBuffer();

                int dataLocation = Client.TransparentLiquidSectionShader.GetAttributeLocation("aData");
                Client.TransparentLiquidSectionShader.Use();

                transparentLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);
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
            if (!simpleDrawGroup.IsFilled) return;

            simpleDrawGroup.BindVertexArray();
            Client.SimpleSectionShader.SetMatrix4("model", model);
            simpleDrawGroup.DrawArrays();
        }

        private void DrawComplexBuffer(Matrix4 model)
        {
            if (!complexDrawGroup.IsFilled) return;

            complexDrawGroup.BindVertexArray();
            Client.ComplexSectionShader.SetMatrix4("model", model);
            complexDrawGroup.DrawElements();
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
                simpleDrawGroup.Delete();
                complexDrawGroup.Delete();
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
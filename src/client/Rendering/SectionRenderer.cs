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

        public const int DrawStageCount = 6;

        private readonly ArrayIDataDrawGroup simpleDrawGroup;
        private readonly ArrayIDataDrawGroup crossPlantDrawGroup;

        private readonly ElementPositionDataDrawGroup complexDrawGroup;

        private readonly ElementIDataDrawGroup varyingHeightDrawGroup;
        private readonly ElementIDataDrawGroup opaqueLiquidDrawGroup;
        private readonly ElementIDataDrawGroup transparentLiquidDrawGroup;

        public SectionRenderer()
        {
            simpleDrawGroup = ArrayIDataDrawGroup.Create(2);
            crossPlantDrawGroup = ArrayIDataDrawGroup.Create(2);
            complexDrawGroup = ElementPositionDataDrawGroup.Create(3, 2);
            varyingHeightDrawGroup = ElementIDataDrawGroup.Create(2);
            opaqueLiquidDrawGroup = ElementIDataDrawGroup.Create(2);
            transparentLiquidDrawGroup = ElementIDataDrawGroup.Create(2);

            #region SIMPLE BUFFER SETUP

            simpleDrawGroup.VertexArrayBindBuffer();

            Shaders.SimpleSectionShader.Use();
            int dataLocation = Shaders.SimpleSectionShader.GetAttributeLocation("aData");

            simpleDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion SIMPLE BUFFER SETUP

            #region CROSS PLANT BUFFER SETUP

            crossPlantDrawGroup.VertexArrayBindBuffer();

            Shaders.CrossPlantSectionShader.Use();
            dataLocation = Shaders.CrossPlantSectionShader.GetAttributeLocation("aData");

            crossPlantDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion CROSS PLANT BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            complexDrawGroup.VertexArrayBindBuffer();

            Shaders.ComplexSectionShader.Use();
            int positionLocation = Shaders.ComplexSectionShader.GetAttributeLocation("aPosition");
            dataLocation = Shaders.ComplexSectionShader.GetAttributeLocation("aData");

            complexDrawGroup.VertexArrayAttributeBinding(positionLocation, dataLocation);

            #endregion COMPLEX BUFFER SETUP

            #region VARYING HEIGHT BUFFER SETUP

            varyingHeightDrawGroup.VertexArrayBindBuffer();

            Shaders.VaryingHeightShader.Use();
            dataLocation = Shaders.VaryingHeightShader.GetAttributeLocation("aData");

            varyingHeightDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion VARYING HEIGHT BUFFER SETUP

            #region OPAQUE LIQUID BUFFER SETUP

            opaqueLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.OpaqueLiquidSectionShader.Use();
            dataLocation = Shaders.OpaqueLiquidSectionShader.GetAttributeLocation("aData");

            opaqueLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion OPAQUE LIQUID BUFFER SETUP

            #region TRANSPARENT LIQUID BUFFER SETUP

            transparentLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.TransparentLiquidSectionShader.Use();
            dataLocation = Shaders.TransparentLiquidSectionShader.GetAttributeLocation("aData");

            transparentLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion TRANSPARENT LIQUID BUFFER SETUP
        }

        public void SetData(ref SectionMeshData meshData)
        {
            if (disposed)
            {
                return;
            }

            simpleDrawGroup.SetData(meshData.simpleVertexData.Count, meshData.simpleVertexData.ExposeArray());

            crossPlantDrawGroup.SetData(meshData.crossPlantVertexData.Count, meshData.crossPlantVertexData.ExposeArray());

            complexDrawGroup.SetData(meshData.complexVertexPositions.Count, meshData.complexVertexPositions.ExposeArray(),
                meshData.complexVertexData.Count, meshData.complexVertexData.ExposeArray(),
                meshData.complexIndices.Count, meshData.complexIndices.ExposeArray());

            varyingHeightDrawGroup.SetData(
                meshData.varyingHeightVertexData.Count, meshData.varyingHeightVertexData.ExposeArray(),
                meshData.varyingHeightIndices.Count, meshData.varyingHeightIndices.ExposeArray());

            opaqueLiquidDrawGroup.SetData(
                meshData.opaqueLiquidVertexData.Count, meshData.opaqueLiquidVertexData.ExposeArray(),
                meshData.opaqueLiquidIndices.Count, meshData.opaqueLiquidIndices.ExposeArray());

            transparentLiquidDrawGroup.SetData(
                meshData.transparentLiquidVertexData.Count, meshData.transparentLiquidVertexData.ExposeArray(),
                meshData.transparentLiquidIndices.Count, meshData.transparentLiquidIndices.ExposeArray());

            meshData.ReturnPooled();
        }

        public static void PrepareStage(int stage)
        {
            Matrix4 view = Client.Player.GetViewMatrix();
            Matrix4 projection = Client.Player.GetProjectionMatrix();

            switch (stage)
            {
                case 0: PrepareSimpleBuffer(view, projection); break;
                case 1: PrepareCrossPlantBuffer(view, projection); break;
                case 2: PrepareComplexBuffer(view, projection); break;
                case 3: PrepareVaryingHeightBuffer(view, projection); break;
                case 4: PrepareOpaqueLiquidBuffer(view, projection); break;
                case 5: PrepareTransparentLiquidBuffer(view, projection); break;
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.SimpleSectionShader.Use();

            Shaders.SimpleSectionShader.SetMatrix4("view", view);
            Shaders.SimpleSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareCrossPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            Shaders.CrossPlantSectionShader.Use();

            Shaders.CrossPlantSectionShader.SetMatrix4("view", view);
            Shaders.CrossPlantSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            Shaders.ComplexSectionShader.Use();

            Shaders.ComplexSectionShader.SetMatrix4("view", view);
            Shaders.ComplexSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.VaryingHeightShader.Use();

            Shaders.VaryingHeightShader.SetMatrix4("view", view);
            Shaders.VaryingHeightShader.SetMatrix4("projection", projection);
        }

        private static void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.OpaqueLiquidSectionShader.Use();

            Shaders.OpaqueLiquidSectionShader.SetMatrix4("view", view);
            Shaders.OpaqueLiquidSectionShader.SetMatrix4("projection", projection);
        }

        private static void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Screen.FillDepthTexture();

            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shaders.TransparentLiquidSectionShader.Use();

            Shaders.TransparentLiquidSectionShader.SetMatrix4("view", view);
            Shaders.TransparentLiquidSectionShader.SetMatrix4("projection", projection);
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
                case 1: DrawCrossPlantBuffer(model); break;
                case 2: DrawComplexBuffer(model); break;
                case 3: DrawVaryingHeightBuffer(model); break;
                case 4: DrawOpaqueLiquidBuffer(model); break;
                case 5: DrawTransparentLiquidBuffer(model); break;
            }
        }

        private void DrawSimpleBuffer(Matrix4 model)
        {
            if (!simpleDrawGroup.IsFilled) return;

            simpleDrawGroup.BindVertexArray();
            Shaders.SimpleSectionShader.SetMatrix4("model", model);
            simpleDrawGroup.DrawArrays();
        }

        private void DrawCrossPlantBuffer(Matrix4 model)
        {
            if (!crossPlantDrawGroup.IsFilled) return;

            crossPlantDrawGroup.BindVertexArray();
            Shaders.CrossPlantSectionShader.SetMatrix4("model", model);
            crossPlantDrawGroup.DrawArrays();
        }

        private void DrawComplexBuffer(Matrix4 model)
        {
            if (!complexDrawGroup.IsFilled) return;

            complexDrawGroup.BindVertexArray();
            Shaders.ComplexSectionShader.SetMatrix4("model", model);
            complexDrawGroup.DrawElements();
        }

        private void DrawVaryingHeightBuffer(Matrix4 model)
        {
            if (!varyingHeightDrawGroup.IsFilled) return;

            varyingHeightDrawGroup.BindVertexArray();
            Shaders.VaryingHeightShader.SetMatrix4("model", model);
            varyingHeightDrawGroup.DrawElements();
        }

        private void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (!opaqueLiquidDrawGroup.IsFilled) return;

            opaqueLiquidDrawGroup.BindVertexArray();
            Shaders.OpaqueLiquidSectionShader.SetMatrix4("model", model);
            opaqueLiquidDrawGroup.DrawElements();
        }

        private void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (!transparentLiquidDrawGroup.IsFilled) return;

            transparentLiquidDrawGroup.BindVertexArray();
            Shaders.TransparentLiquidSectionShader.SetMatrix4("model", model);
            transparentLiquidDrawGroup.DrawElements();
        }

        public static void FinishStage(int stage)
        {
            switch (stage)
            {
                case 1: FinishCrossPlantBuffer(); break;
                case 5: FinishTransparentLiquidBuffer(); break;
            }
        }

        private static void FinishCrossPlantBuffer()
        {
            GL.Enable(EnableCap.CullFace);
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
                crossPlantDrawGroup.Delete();
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
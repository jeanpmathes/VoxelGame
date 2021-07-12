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

        public const int DrawStageCount = 7;

        private readonly ArrayIDataDrawGroup simpleDrawGroup;
        private readonly ArrayIDataDrawGroup crossPlantDrawGroup;
        private readonly ArrayIDataDrawGroup cropPlantDrawGroup;

        private readonly ElementPositionDataDrawGroup complexDrawGroup;

        private readonly ElementIDataDrawGroup varyingHeightDrawGroup;
        private readonly ElementIDataDrawGroup opaqueLiquidDrawGroup;
        private readonly ElementIDataDrawGroup transparentLiquidDrawGroup;

        public SectionRenderer()
        {
            simpleDrawGroup = ArrayIDataDrawGroup.Create(2);
            crossPlantDrawGroup = ArrayIDataDrawGroup.Create(2);
            cropPlantDrawGroup = ArrayIDataDrawGroup.Create(2);

            complexDrawGroup = ElementPositionDataDrawGroup.Create(3, 2);

            varyingHeightDrawGroup = ElementIDataDrawGroup.Create(2);
            opaqueLiquidDrawGroup = ElementIDataDrawGroup.Create(2);
            transparentLiquidDrawGroup = ElementIDataDrawGroup.Create(2);

            #region SIMPLE BUFFER SETUP

            simpleDrawGroup.VertexArrayBindBuffer();

            Shaders.SimpleSection.Use();
            int dataLocation = Shaders.SimpleSection.GetAttributeLocation("aData");

            simpleDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion SIMPLE BUFFER SETUP

            #region CROSS PLANT BUFFER SETUP

            crossPlantDrawGroup.VertexArrayBindBuffer();

            Shaders.CrossPlantSection.Use();
            dataLocation = Shaders.CrossPlantSection.GetAttributeLocation("aData");

            crossPlantDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion CROSS PLANT BUFFER SETUP

            #region CROP PLANT BUFFER SETUP

            cropPlantDrawGroup.VertexArrayBindBuffer();

            Shaders.CropPlantSection.Use();
            dataLocation = Shaders.CropPlantSection.GetAttributeLocation("aData");

            cropPlantDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion CROP PLANT BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            complexDrawGroup.VertexArrayBindBuffer();

            Shaders.ComplexSection.Use();
            int positionLocation = Shaders.ComplexSection.GetAttributeLocation("aPosition");
            dataLocation = Shaders.ComplexSection.GetAttributeLocation("aData");

            complexDrawGroup.VertexArrayAttributeBinding(positionLocation, dataLocation);

            #endregion COMPLEX BUFFER SETUP

            #region VARYING HEIGHT BUFFER SETUP

            varyingHeightDrawGroup.VertexArrayBindBuffer();

            Shaders.VaryingHeight.Use();
            dataLocation = Shaders.VaryingHeight.GetAttributeLocation("aData");

            varyingHeightDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion VARYING HEIGHT BUFFER SETUP

            #region OPAQUE LIQUID BUFFER SETUP

            opaqueLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.OpaqueLiquidSection.Use();
            dataLocation = Shaders.OpaqueLiquidSection.GetAttributeLocation("aData");

            opaqueLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion OPAQUE LIQUID BUFFER SETUP

            #region TRANSPARENT LIQUID BUFFER SETUP

            transparentLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.TransparentLiquidSection.Use();
            dataLocation = Shaders.TransparentLiquidSection.GetAttributeLocation("aData");

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

            cropPlantDrawGroup.SetData(meshData.cropPlantVertexData.Count, meshData.cropPlantVertexData.ExposeArray());

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
                case 2: PrepareCropPlantBuffer(view, projection); break;
                case 3: PrepareComplexBuffer(view, projection); break;
                case 4: PrepareVaryingHeightBuffer(view, projection); break;
                case 5: PrepareOpaqueLiquidBuffer(view, projection); break;
                case 6: PrepareTransparentLiquidBuffer(view, projection); break;
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.SimpleSection.Use();

            Shaders.SimpleSection.SetMatrix4("view", view);
            Shaders.SimpleSection.SetMatrix4("projection", projection);
        }

        private static void PrepareCrossPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            Shaders.CrossPlantSection.Use();

            Shaders.CrossPlantSection.SetMatrix4("view", view);
            Shaders.CrossPlantSection.SetMatrix4("projection", projection);
        }

        private static void PrepareCropPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            Shaders.CropPlantSection.Use();

            Shaders.CropPlantSection.SetMatrix4("view", view);
            Shaders.CropPlantSection.SetMatrix4("projection", projection);
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            Shaders.ComplexSection.Use();

            Shaders.ComplexSection.SetMatrix4("view", view);
            Shaders.ComplexSection.SetMatrix4("projection", projection);
        }

        private static void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.VaryingHeight.Use();

            Shaders.VaryingHeight.SetMatrix4("view", view);
            Shaders.VaryingHeight.SetMatrix4("projection", projection);
        }

        private static void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            Shaders.OpaqueLiquidSection.Use();

            Shaders.OpaqueLiquidSection.SetMatrix4("view", view);
            Shaders.OpaqueLiquidSection.SetMatrix4("projection", projection);
        }

        private static void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Screen.FillDepthTexture();

            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shaders.TransparentLiquidSection.Use();

            Shaders.TransparentLiquidSection.SetMatrix4("view", view);
            Shaders.TransparentLiquidSection.SetMatrix4("projection", projection);
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
                case 2: DrawCropPlantBuffer(model); break;
                case 3: DrawComplexBuffer(model); break;
                case 4: DrawVaryingHeightBuffer(model); break;
                case 5: DrawOpaqueLiquidBuffer(model); break;
                case 6: DrawTransparentLiquidBuffer(model); break;
            }
        }

        private void DrawSimpleBuffer(Matrix4 model)
        {
            if (!simpleDrawGroup.IsFilled) return;

            simpleDrawGroup.BindVertexArray();
            Shaders.SimpleSection.SetMatrix4("model", model);
            simpleDrawGroup.DrawArrays();
        }

        private void DrawCrossPlantBuffer(Matrix4 model)
        {
            if (!crossPlantDrawGroup.IsFilled) return;

            crossPlantDrawGroup.BindVertexArray();
            Shaders.CrossPlantSection.SetMatrix4("model", model);
            crossPlantDrawGroup.DrawArrays();
        }

        private void DrawCropPlantBuffer(Matrix4 model)
        {
            if (!cropPlantDrawGroup.IsFilled) return;

            cropPlantDrawGroup.BindVertexArray();
            Shaders.CropPlantSection.SetMatrix4("model", model);
            cropPlantDrawGroup.DrawArrays();
        }

        private void DrawComplexBuffer(Matrix4 model)
        {
            if (!complexDrawGroup.IsFilled) return;

            complexDrawGroup.BindVertexArray();
            Shaders.ComplexSection.SetMatrix4("model", model);
            complexDrawGroup.DrawElements();
        }

        private void DrawVaryingHeightBuffer(Matrix4 model)
        {
            if (!varyingHeightDrawGroup.IsFilled) return;

            varyingHeightDrawGroup.BindVertexArray();
            Shaders.VaryingHeight.SetMatrix4("model", model);
            varyingHeightDrawGroup.DrawElements();
        }

        private void DrawOpaqueLiquidBuffer(Matrix4 model)
        {
            if (!opaqueLiquidDrawGroup.IsFilled) return;

            opaqueLiquidDrawGroup.BindVertexArray();
            Shaders.OpaqueLiquidSection.SetMatrix4("model", model);
            opaqueLiquidDrawGroup.DrawElements();
        }

        private void DrawTransparentLiquidBuffer(Matrix4 model)
        {
            if (!transparentLiquidDrawGroup.IsFilled) return;

            transparentLiquidDrawGroup.BindVertexArray();
            Shaders.TransparentLiquidSection.SetMatrix4("model", model);
            transparentLiquidDrawGroup.DrawElements();
        }

        public static void FinishStage(int stage)
        {
            switch (stage)
            {
                case 1 or 2: FinishPlantBuffer(); break;
                case 6: FinishTransparentLiquidBuffer(); break;
            }
        }

        private static void FinishPlantBuffer()
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
                cropPlantDrawGroup.Delete();
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
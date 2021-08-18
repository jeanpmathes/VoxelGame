// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Graphics.Groups;
using VoxelGame.Graphics.Objects;
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

        private const int Simple = 0;
        private const int CrossPlant = 1;
        private const int CropPlant = 2;
        private const int Complex = 3;
        private const int VaryingHeight = 4;
        private const int OpaqueLiquid = 5;
        private const int TransparentLiquid = 6;

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

            Shaders.VaryingHeightSection.Use();
            dataLocation = Shaders.VaryingHeightSection.GetAttributeLocation("aData");

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

        public void SetData(SectionMeshData meshData)
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
                case Simple: PrepareSimpleBuffer(view, projection); break;
                case CrossPlant: PrepareCrossPlantBuffer(view, projection); break;
                case CropPlant: PrepareCropPlantBuffer(view, projection); break;
                case Complex: PrepareComplexBuffer(view, projection); break;
                case VaryingHeight: PrepareVaryingHeightBuffer(view, projection); break;
                case OpaqueLiquid: PrepareOpaqueLiquidBuffer(view, projection); break;
                case TransparentLiquid: PrepareTransparentLiquidBuffer(view, projection); break;
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.SimpleSection, view, projection);
        }

        private static void PrepareCrossPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            SetupShader(Shaders.CrossPlantSection, view, projection);
        }

        private static void PrepareCropPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            SetupShader(Shaders.CropPlantSection, view, projection);
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            SetupShader(Shaders.ComplexSection, view, projection);
        }

        private static void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.VaryingHeightSection, view, projection);
        }

        private static void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.OpaqueLiquidSection, view, projection);
        }

        private static void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Screen.FillDepthTexture();

            Client.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            SetupShader(Shaders.TransparentLiquidSection, view, projection);
        }

        private static void SetupShader(Shader shader, Matrix4 view, Matrix4 projection)
        {
            shader.Use();

            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
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
                case Simple: Draw(simpleDrawGroup, Shaders.SimpleSection, model); break;
                case CrossPlant: Draw(crossPlantDrawGroup, Shaders.CrossPlantSection, model); break;
                case CropPlant: Draw(cropPlantDrawGroup, Shaders.CropPlantSection, model); break;
                case Complex: Draw(complexDrawGroup, Shaders.ComplexSection, model); break;
                case VaryingHeight: Draw(varyingHeightDrawGroup, Shaders.VaryingHeightSection, model); break;
                case OpaqueLiquid: Draw(opaqueLiquidDrawGroup, Shaders.OpaqueLiquidSection, model); break;
                case TransparentLiquid: Draw(transparentLiquidDrawGroup, Shaders.TransparentLiquidSection, model); break;
            }
        }

        private static void Draw(IDrawGroup drawGroup, Shader shader, Matrix4 model)
        {
            if (!drawGroup.IsFilled) return;

            drawGroup.BindVertexArray();
            shader.SetMatrix4("model", model);
            drawGroup.Draw();
        }

        public static void FinishStage(int stage)
        {
            switch (stage)
            {
                case CrossPlant or CropPlant: FinishPlantBuffer(); break;
                case TransparentLiquid: FinishTransparentLiquidBuffer(); break;
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
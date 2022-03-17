// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Groups;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    ///     A renderer for <see cref="VoxelGame.Core.Logic.Section" />.
    /// </summary>
    public sealed class SectionRenderer : IDisposable
    {
        private const string DataAttribute = "aData";

        private const int Simple = 0;
        private const int CrossPlant = 1;
        private const int CropPlant = 2;
        private const int Complex = 3;
        private const int VaryingHeight = 4;
        private const int OpaqueLiquid = 5;
        private const int TransparentLiquid = 6;
        private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();

        private readonly ElementPositionDataDrawGroup complexDrawGroup;
        private readonly ElementInstancedIDataDrawGroup cropPlantDrawGroup;
        private readonly ElementInstancedIDataDrawGroup crossPlantDrawGroup;
        private readonly ElementIDataDrawGroup opaqueLiquidDrawGroup;

        private readonly ArrayIDataDrawGroup simpleDrawGroup;
        private readonly ElementIDataDrawGroup transparentLiquidDrawGroup;

        private readonly ElementIDataDrawGroup varyingHeightDrawGroup;

        /// <summary>
        ///     Creates a new <see cref="SectionRenderer" />.
        /// </summary>
        public SectionRenderer()
        {
            simpleDrawGroup = ArrayIDataDrawGroup.Create(size: 2);

            crossPlantDrawGroup = ElementInstancedIDataDrawGroup.Create(
                BlockModels.CreateCrossPlantModel(Application.Client.Instance.Graphics.FoliageQuality),
                instanceSize: 2);

            cropPlantDrawGroup = ElementInstancedIDataDrawGroup.Create(
                BlockModels.CreateCropPlantModel(Application.Client.Instance.Graphics.FoliageQuality),
                instanceSize: 2);

            complexDrawGroup = ElementPositionDataDrawGroup.Create(positionSize: 3, dataSize: 2);

            varyingHeightDrawGroup = ElementIDataDrawGroup.Create(size: 2);
            opaqueLiquidDrawGroup = ElementIDataDrawGroup.Create(size: 2);
            transparentLiquidDrawGroup = ElementIDataDrawGroup.Create(size: 2);

            #region SIMPLE BUFFER SETUP

            simpleDrawGroup.VertexArrayBindBuffer();

            Shaders.SimpleSection.Use();
            int dataLocation = Shaders.SimpleSection.GetAttributeLocation(DataAttribute);

            simpleDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion SIMPLE BUFFER SETUP

            #region CROSS PLANT BUFFER SETUP

            crossPlantDrawGroup.VertexArrayBindBuffer(modelSize: 5);

            Shaders.CrossPlantSection.Use();

            dataLocation = Shaders.CrossPlantSection.GetAttributeLocation("aVertexPosition");
            crossPlantDrawGroup.VertexArrayModelAttributeBinding(dataLocation, size: 3, offset: 0);

            dataLocation = Shaders.CrossPlantSection.GetAttributeLocation("aTexCoord");
            crossPlantDrawGroup.VertexArrayModelAttributeBinding(dataLocation, size: 2, offset: 3);

            dataLocation = Shaders.CrossPlantSection.GetAttributeLocation("aInstanceData");
            crossPlantDrawGroup.VertexArrayInstanceAttributeBinding(dataLocation);

            #endregion CROSS PLANT BUFFER SETUP

            #region CROP PLANT BUFFER SETUP

            cropPlantDrawGroup.VertexArrayBindBuffer(modelSize: 8);

            Shaders.CropPlantSection.Use();

            dataLocation = Shaders.CropPlantSection.GetAttributeLocation("aVertexPositionNS");
            cropPlantDrawGroup.VertexArrayModelAttributeBinding(dataLocation, size: 3, offset: 0);

            dataLocation = Shaders.CropPlantSection.GetAttributeLocation("aVertexPositionEW");
            cropPlantDrawGroup.VertexArrayModelAttributeBinding(dataLocation, size: 3, offset: 3);

            dataLocation = Shaders.CropPlantSection.GetAttributeLocation("aTexCoord");
            cropPlantDrawGroup.VertexArrayModelAttributeBinding(dataLocation, size: 2, offset: 6);

            dataLocation = Shaders.CropPlantSection.GetAttributeLocation("aInstanceData");
            cropPlantDrawGroup.VertexArrayInstanceAttributeBinding(dataLocation);

            #endregion CROP PLANT BUFFER SETUP

            #region COMPLEX BUFFER SETUP

            complexDrawGroup.VertexArrayBindBuffer();

            Shaders.ComplexSection.Use();
            int positionLocation = Shaders.ComplexSection.GetAttributeLocation("aPosition");
            dataLocation = Shaders.ComplexSection.GetAttributeLocation(DataAttribute);

            complexDrawGroup.VertexArrayAttributeBinding(positionLocation, dataLocation);

            #endregion COMPLEX BUFFER SETUP

            #region VARYING HEIGHT BUFFER SETUP

            varyingHeightDrawGroup.VertexArrayBindBuffer();

            Shaders.VaryingHeightSection.Use();
            dataLocation = Shaders.VaryingHeightSection.GetAttributeLocation(DataAttribute);

            varyingHeightDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion VARYING HEIGHT BUFFER SETUP

            #region OPAQUE LIQUID BUFFER SETUP

            opaqueLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.OpaqueLiquidSection.Use();
            dataLocation = Shaders.OpaqueLiquidSection.GetAttributeLocation(DataAttribute);

            opaqueLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion OPAQUE LIQUID BUFFER SETUP

            #region TRANSPARENT LIQUID BUFFER SETUP

            transparentLiquidDrawGroup.VertexArrayBindBuffer();

            Shaders.TransparentLiquidSection.Use();
            dataLocation = Shaders.TransparentLiquidSection.GetAttributeLocation(DataAttribute);

            transparentLiquidDrawGroup.VertexArrayAttributeBinding(dataLocation);

            #endregion TRANSPARENT LIQUID BUFFER SETUP

        }

        /// <summary>
        ///     The number of draw stages.
        /// </summary>
        public static int DrawStageCount => 7;

        private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

        /// <summary>
        ///     Set the section mesh data to render. Must not be discarded.
        /// </summary>
        /// <param name="meshData">The mesh data to use.</param>
        public void SetData(SectionMeshData meshData)
        {
            if (disposed) return;

            simpleDrawGroup.SetData(meshData.simpleVertexData.Count, meshData.simpleVertexData.ExposeArray());

            crossPlantDrawGroup.SetInstanceData(
                meshData.crossPlantVertexData.Count,
                meshData.crossPlantVertexData.ExposeArray());

            cropPlantDrawGroup.SetInstanceData(
                meshData.cropPlantVertexData.Count,
                meshData.cropPlantVertexData.ExposeArray());

            complexDrawGroup.SetData(
                meshData.complexVertexPositions.Count,
                meshData.complexVertexPositions.ExposeArray(),
                meshData.complexVertexData.Count,
                meshData.complexVertexData.ExposeArray(),
                meshData.complexIndices.Count,
                meshData.complexIndices.ExposeArray());

            varyingHeightDrawGroup.SetData(
                meshData.varyingHeightVertexData.Count,
                meshData.varyingHeightVertexData.ExposeArray(),
                meshData.varyingHeightIndices.Count,
                meshData.varyingHeightIndices.ExposeArray());

            opaqueLiquidDrawGroup.SetData(
                meshData.opaqueLiquidVertexData.Count,
                meshData.opaqueLiquidVertexData.ExposeArray(),
                meshData.opaqueLiquidIndices.Count,
                meshData.opaqueLiquidIndices.ExposeArray());

            transparentLiquidDrawGroup.SetData(
                meshData.transparentLiquidVertexData.Count,
                meshData.transparentLiquidVertexData.ExposeArray(),
                meshData.transparentLiquidIndices.Count,
                meshData.transparentLiquidIndices.ExposeArray());

            meshData.ReturnPooled();
        }

        /// <summary>
        ///     Prepare drawing a specific stage.
        /// </summary>
        /// <param name="stage">The draw stage to prepare.</param>
        public static void PrepareStage(int stage)
        {
            Matrix4 view = Application.Client.Instance.CurrentGame!.Player.ViewMatrix;
            Matrix4 projection = Application.Client.Instance.CurrentGame!.Player.ProjectionMatrix;

            switch (stage)
            {
                case Simple:
                    PrepareSimpleBuffer(view, projection);

                    break;
                case CrossPlant:
                    PrepareCrossPlantBuffer(view, projection);

                    break;
                case CropPlant:
                    PrepareCropPlantBuffer(view, projection);

                    break;
                case Complex:
                    PrepareComplexBuffer(view, projection);

                    break;
                case VaryingHeight:
                    PrepareVaryingHeightBuffer(view, projection);

                    break;
                case OpaqueLiquid:
                    PrepareOpaqueLiquidBuffer(view, projection);

                    break;
                case TransparentLiquid:
                    PrepareTransparentLiquidBuffer(view, projection);

                    break;

                default: throw new InvalidOperationException();
            }
        }

        private static void PrepareSimpleBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.SimpleSection, view, projection);
        }

        private static void PrepareCrossPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            SetupShader(Shaders.CrossPlantSection, view, projection);
        }

        private static void PrepareCropPlantBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            GL.Disable(EnableCap.CullFace);

            SetupShader(Shaders.CropPlantSection, view, projection);
        }

        private static void PrepareComplexBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);

            SetupShader(Shaders.ComplexSection, view, projection);
        }

        private static void PrepareVaryingHeightBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.VaryingHeightSection, view, projection);
        }

        private static void PrepareOpaqueLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Application.Client.Instance.Resources.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            SetupShader(Shaders.OpaqueLiquidSection, view, projection);
        }

        private static void PrepareTransparentLiquidBuffer(Matrix4 view, Matrix4 projection)
        {
            Screen.FillDepthTexture();

            Application.Client.Instance.Resources.LiquidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

            GL.Enable(EnableCap.Blend);
            GL.DepthMask(flag: false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            SetupShader(Shaders.TransparentLiquidSection, view, projection);
        }

        private static void SetupShader(Shader shader, Matrix4 view, Matrix4 projection)
        {
            shader.Use();

            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
        }

        /// <summary>
        ///     Draw a specific stage.
        /// </summary>
        /// <param name="stage">The stage to draw.</param>
        /// <param name="position">The position at which the section should be drawn.</param>
        public void DrawStage(int stage, Vector3 position)
        {
            if (disposed) return;

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);

            switch (stage)
            {
                case Simple:
                    Draw(simpleDrawGroup, Shaders.SimpleSection, model);

                    break;
                case CrossPlant:
                    Draw(crossPlantDrawGroup, Shaders.CrossPlantSection, model);

                    break;
                case CropPlant:
                    Draw(cropPlantDrawGroup, Shaders.CropPlantSection, model);

                    break;
                case Complex:
                    Draw(complexDrawGroup, Shaders.ComplexSection, model);

                    break;
                case VaryingHeight:
                    Draw(varyingHeightDrawGroup, Shaders.VaryingHeightSection, model);

                    break;
                case OpaqueLiquid:
                    Draw(opaqueLiquidDrawGroup, Shaders.OpaqueLiquidSection, model);

                    break;
                case TransparentLiquid:
                    Draw(transparentLiquidDrawGroup, Shaders.TransparentLiquidSection, model);

                    break;

                default: throw new InvalidOperationException();
            }
        }

        private static void Draw(IDrawGroup drawGroup, Shader shader, Matrix4 model)
        {
            if (!drawGroup.IsFilled) return;

            drawGroup.BindVertexArray();
            shader.SetMatrix4("model", model);
            drawGroup.Draw();
        }

        /// <summary>
        ///     Finish drawing a specific stage.
        /// </summary>
        /// <param name="stage">The stage to finish.</param>
        public static void FinishStage(int stage)
        {
            switch (stage)
            {
                case CrossPlant or CropPlant:
                    FinishPlantBuffer();

                    break;
                case TransparentLiquid:
                    FinishTransparentLiquidBuffer();

                    break;

                default:
                    if (stage < 0 || stage >= DrawStageCount) throw new InvalidOperationException();

                    break;
            }
        }

        private static void FinishPlantBuffer()
        {
            GL.Enable(EnableCap.CullFace);
        }

        private static void FinishTransparentLiquidBuffer()
        {
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(flag: true);
        }

        #region IDisposable Support

        private bool disposed;

        private void Dispose(bool disposing)
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
                logger.LogWarning(
                    Events.UndeletedBuffers,
                    "Renderer disposed by GC without freeing storage");
            }

            disposed = true;
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~SectionRenderer()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Dispose of the renderer.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}

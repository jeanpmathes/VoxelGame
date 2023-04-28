﻿// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics.Groups;
using VoxelGame.Support.Graphics.Objects;

namespace VoxelGame.Client.Rendering;

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
    private const int OpaqueFluid = 5;
    private const int TransparentFluidAccumulate = 6;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();

    private static Matrix4d viewMatrix;
    private static Matrix4d viewProjectionMatrix;

    private readonly ElementPositionDataDrawGroup complexDrawGroup;
    private readonly ElementInstancedIDataDrawGroup cropPlantDrawGroup;
    private readonly ElementInstancedIDataDrawGroup crossPlantDrawGroup;
    private readonly ElementIDataDrawGroup opaqueFluidDrawGroup;

    private readonly ArrayIDataDrawGroup simpleDrawGroup;
    private readonly ElementIDataDrawGroup transparentFluidDrawGroup;

    private readonly ElementIDataDrawGroup varyingHeightDrawGroup;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer()
    {
        // todo: port to DirectX

        simpleDrawGroup = ArrayIDataDrawGroup.Create(size: 2);

        crossPlantDrawGroup = ElementInstancedIDataDrawGroup.Create(
            BlockModels.CreateCrossPlantModel(Application.Client.Instance.Graphics.FoliageQuality),
            instanceSize: 2);

        cropPlantDrawGroup = ElementInstancedIDataDrawGroup.Create(
            BlockModels.CreateCropPlantModel(Application.Client.Instance.Graphics.FoliageQuality),
            instanceSize: 2);

        complexDrawGroup = ElementPositionDataDrawGroup.Create(positionSize: 3, dataSize: 2);

        varyingHeightDrawGroup = ElementIDataDrawGroup.Create(size: 2);
        opaqueFluidDrawGroup = ElementIDataDrawGroup.Create(size: 2);
        transparentFluidDrawGroup = ElementIDataDrawGroup.Create(size: 2);

        disposed = true; // todo: remove this line when porting to DirectX

        return; // todo: remove this line when porting to DirectX

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

        opaqueFluidDrawGroup.VertexArrayBindBuffer();

        Shaders.OpaqueFluidSection.Use();
        dataLocation = Shaders.OpaqueFluidSection.GetAttributeLocation(DataAttribute);

        opaqueFluidDrawGroup.VertexArrayAttributeBinding(dataLocation);

        #endregion OPAQUE LIQUID BUFFER SETUP

        #region TRANSPARENT LIQUID BUFFER SETUP

        transparentFluidDrawGroup.VertexArrayBindBuffer();

        Shaders.TransparentFluidSectionAccumulate.Use();
        dataLocation = Shaders.TransparentFluidSectionAccumulate.GetAttributeLocation(DataAttribute);

        transparentFluidDrawGroup.VertexArrayAttributeBinding(dataLocation);

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
        meshData.ReturnPooled(); // todo: remove this line when porting to DirectX

        if (disposed) return; // todo: remove the usage of the count as the span already has the length

        simpleDrawGroup.SetData(meshData.simpleVertexData.Count, meshData.simpleVertexData.AsSpan().ToArray());

        crossPlantDrawGroup.SetInstanceData(
            meshData.crossPlantVertexData.Count,
            meshData.crossPlantVertexData.AsSpan().ToArray());

        cropPlantDrawGroup.SetInstanceData(
            meshData.cropPlantVertexData.Count,
            meshData.cropPlantVertexData.AsSpan().ToArray());

        complexDrawGroup.SetData(
            meshData.complexVertexPositions.Count,
            meshData.complexVertexPositions.AsSpan().ToArray(),
            meshData.complexVertexData.Count,
            meshData.complexVertexData.AsSpan().ToArray(),
            meshData.complexIndices.Count,
            meshData.complexIndices.AsSpan().ToArray());

        varyingHeightDrawGroup.SetData(
            meshData.varyingHeightVertexData.Count,
            meshData.varyingHeightVertexData.AsSpan().ToArray(),
            meshData.varyingHeightIndices.Count,
            meshData.varyingHeightIndices.AsSpan().ToArray());

        opaqueFluidDrawGroup.SetData(
            meshData.opaqueFluidVertexData.Count,
            meshData.opaqueFluidVertexData.AsSpan().ToArray(),
            meshData.opaqueFluidIndices.Count,
            meshData.opaqueFluidIndices.AsSpan().ToArray());

        transparentFluidDrawGroup.SetData(
            meshData.transparentFluidVertexData.Count,
            meshData.transparentFluidVertexData.AsSpan().ToArray(),
            meshData.transparentFluidIndices.Count,
            meshData.transparentFluidIndices.AsSpan().ToArray());

        meshData.ReturnPooled();
    }

    /// <summary>
    ///     Prepare drawing a specific stage.
    /// </summary>
    /// <param name="stage">The draw stage to prepare.</param>
    /// <param name="pass">The current rendering pass.</param>
    public static void PrepareStage(int stage, PassContext pass)
    {
        Matrix4d view = pass.ViewMatrix;
        Matrix4d projection = pass.ProjectionMatrix;

        viewMatrix = view;
        viewProjectionMatrix = view * projection;

        switch (stage)
        {
            case Simple:
                PrepareSimpleBuffer();

                break;
            case CrossPlant:
                PrepareCrossPlantBuffer();

                break;
            case CropPlant:
                PrepareCropPlantBuffer();

                break;
            case Complex:
                PrepareComplexBuffer();

                break;
            case VaryingHeight:
                PrepareVaryingHeightBuffer();

                break;
            case OpaqueFluid:
                PrepareOpaqueFluidBuffer();

                break;
            case TransparentFluidAccumulate:
                PrepareTransparentFluidBufferAccumulate();

                break;

            default: throw new InvalidOperationException();
        }
    }

    private static void PrepareSimpleBuffer()
    {
        // Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);
        //
        // GL.Enable(EnableCap.DepthClamp);
        //
        // Shaders.SimpleSection.Use();
    }

    private static void PrepareCrossPlantBuffer()
    {
        // Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);
        //
        // GL.Disable(EnableCap.CullFace);
        //
        // Shaders.CrossPlantSection.Use();
    }

    private static void PrepareCropPlantBuffer()
    {
        // Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);
        //
        // GL.Disable(EnableCap.CullFace);
        //
        // Shaders.CropPlantSection.Use();
    }

    private static void PrepareComplexBuffer()
    {
        // Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.ClampToEdge);
        //
        // GL.Enable(EnableCap.DepthClamp);
        //
        // Shaders.ComplexSection.Use();
    }

    private static void PrepareVaryingHeightBuffer()
    {
        // Application.Client.Instance.Resources.BlockTextureArray.SetWrapMode(TextureWrapMode.Repeat);
        //
        // GL.Enable(EnableCap.DepthClamp);
        //
        // Shaders.VaryingHeightSection.Use();
    }

    private static void PrepareOpaqueFluidBuffer()
    {
        // Application.Client.Instance.Resources.FluidTextureArray.SetWrapMode(TextureWrapMode.Repeat);
        //
        // Shaders.OpaqueFluidSection.Use();
    }

    private static void PrepareTransparentFluidBufferAccumulate()
    {
        /*Screen.FillDepthTexture();

        Application.Client.Instance.Resources.FluidTextureArray.SetWrapMode(TextureWrapMode.Repeat);

        Screen.DrawToTransparencyTarget();

        GL.DepthMask(flag: false);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(buf: 0, BlendingFactorSrc.One, BlendingFactorDest.One);
        GL.BlendFunc(buf: 1, BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);

        Shaders.TransparentFluidSectionAccumulate.Use();*/
    }

    /// <summary>
    ///     Draw a specific stage.
    /// </summary>
    /// <param name="stage">The stage to draw.</param>
    /// <param name="position">The position at which the section should be drawn.</param>
    public void DrawStage(int stage, Vector3d position)
    {
        if (disposed) return;

        Matrix4d model = Matrix4d.Identity * Matrix4d.CreateTranslation(position);

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
            case OpaqueFluid:
                Draw(opaqueFluidDrawGroup, Shaders.OpaqueFluidSection, model, passModelView: true);

                break;
            case TransparentFluidAccumulate:
                Draw(transparentFluidDrawGroup, Shaders.TransparentFluidSectionAccumulate, model, passModelView: true);

                break;

            default: throw new InvalidOperationException();
        }
    }

    private static void Draw(IDrawGroup drawGroup, Shader shader, Matrix4d model, bool passModelView = false)
    {
        if (!drawGroup.IsFilled) return;

        drawGroup.BindVertexArray();

        if (passModelView) shader.SetMatrix4("mv_matrix", (model * viewMatrix).ToMatrix4());
        shader.SetMatrix4("mvp_matrix", (model * viewProjectionMatrix).ToMatrix4());

        drawGroup.Draw();
    }

    /// <summary>
    ///     Finish drawing a specific stage.
    /// </summary>
    /// <param name="stage">The stage to finish.</param>
    public static void FinishStage(int stage)
    {
        /*GL.Disable(EnableCap.DepthClamp);

        switch (stage)
        {
            case Simple or Complex or VaryingHeight:
                FinishSolidBuffer();

                break;
            case CrossPlant or CropPlant:
                FinishPlantBuffer();

                break;
            case TransparentFluidAccumulate:
                FinishTransparentFluidBuffer();

                break;

            default:
                if (stage < 0 || stage >= DrawStageCount) throw new InvalidOperationException();

                break;
        }*/
    }

    private static void FinishSolidBuffer()
    {
        // GL.Disable(EnableCap.DepthClamp);
    }

    private static void FinishPlantBuffer()
    {
        // GL.Enable(EnableCap.CullFace);
    }

    private static void FinishTransparentFluidBuffer()
    {
        // Screen.DrawToPrimaryTarget();
        //
        // GL.Disable(EnableCap.Blend);
        // GL.DepthMask(flag: true);
    }

    /// <summary>
    ///     Draw all required fullscreen passes.
    /// </summary>
    public static void DrawFullscreenPasses()
    {
        DrawTransparencyPass();
    }

    private static void DrawTransparencyPass()
    {
        // GL.DepthMask(flag: false);
        // GL.DepthFunc(DepthFunction.Always);
        // GL.Enable(EnableCap.Blend);
        // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        //
        // Shaders.TransparentFluidSectionDraw.Use();
        // Screen.DrawFullScreenPass();
        //
        // GL.Disable(EnableCap.Blend);
        // GL.DepthFunc(DepthFunction.Less);
        // GL.DepthMask(flag: true);
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
            opaqueFluidDrawGroup.Delete();
            transparentFluidDrawGroup.Delete();
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

// <copyright file="Native.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using Mesh = VoxelGame.Graphics.Objects.Mesh;

namespace VoxelGame.Graphics;

/// <summary>
///     Utility methods for calling some of the native methods easily.
/// </summary>
#pragma warning disable S3242 // The specific types are matched on the native side.
#pragma warning disable S1200 // This class intentionally contains all native functions.
internal static class Native
{
    private static readonly Dictionary<IntPtr, Camera> cameras = new();
    private static readonly Dictionary<IntPtr, Light> lights = new();
    private static readonly Dictionary<UInt32, Object> draw2DCallbacks = new();
    private static Definition.Native.ScreenshotFunc? screenshotCallback;

    /// <summary>
    ///     Get current allocator statistics as a string.
    /// </summary>
    internal static unsafe String GetAllocatorStatistics(Client client)
    {
        var result = "";

        NativeMethods.PassAllocatorStatistics(client, stringPointer =>
        {
            result = Utf16StringMarshaller.ConvertToManaged(stringPointer);
        });

        return result;
    }

    /// <summary>
    ///     Get the DRED (Device Removed Extended Data) string.
    ///     This is only available in debug builds and after a device removal.
    /// </summary>
    internal static unsafe String GetDRED(Client client)
    {
        var result = "";

        NativeMethods.PassDRED(client, stringPointer =>
        {
            result = Utf16StringMarshaller.ConvertToManaged(stringPointer);
        });

        return result;
    }

    /// <summary>
    ///     Queue a screenshot to be taken. If the screenshot is already queued, this call is ignored.
    /// </summary>
    /// <param name="client">The client for which to take a screenshot.</param>
    /// <param name="callback">The callback to call when the screenshot is taken.</param>
    internal static void EnqueueScreenshot(Client client, Definition.Native.ScreenshotFunc callback)
    {
        if (screenshotCallback != null) return;

        screenshotCallback = callback;

        NativeMethods.TakeScreenshot(client,
            (data, width, height) =>
            {
                Debug.Assert(screenshotCallback != null);

                screenshotCallback(data, width, height);
                screenshotCallback = null;
            });
    }

    /// <summary>
    ///     Initialize raytracing.
    /// </summary>
    /// <typeparam name="T">The type of the shader buffer.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the raytracing pipeline.</param>
    /// <returns>The shader buffer, if any is created.</returns>
    internal static ShaderBuffer<T>? InitializeRaytracing<T>(Client client, SpacePipelineDescription description) where T : unmanaged, IEquatable<T>
    {
        IntPtr buffer = NativeMethods.InitializeRaytracing(client, description);

        return buffer == IntPtr.Zero ? null : new ShaderBuffer<T>(buffer, client);
    }

    /// <summary>
    ///     Get the camera of the native client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>The camera.</returns>
    internal static Camera GetCamera(Client client)
    {
        IntPtr camera = NativeMethods.GetCamera(client);
        Camera cameraObject;

        if (cameras.TryGetValue(camera, out Camera? @object))
        {
            cameraObject = @object;
        }
        else
        {
            cameraObject = new Camera(camera, client.Space);
            cameras.Add(camera, cameraObject);
        }

        return cameraObject;
    }

    /// <summary>
    ///     Get the light of the native client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>The light.</returns>
    internal static Light GetLight(Client client)
    {
        IntPtr light = NativeMethods.GetLight(client);
        Light lightObject;

        if (lights.TryGetValue(light, out Light? @object))
        {
            lightObject = @object;
        }
        else
        {
            lightObject = new Light(light, client.Space);
            lights.Add(light, lightObject);
        }

        return lightObject;
    }

    /// <summary>
    ///     Create a mesh.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="materialIndex">The material index, as defined in pipeline setup.</param>
    /// <returns>The mesh.</returns>
    internal static Mesh CreateMesh(Client client, UInt32 materialIndex)
    {
        IntPtr mesh = NativeMethods.CreateMesh(client, materialIndex);

        return new Mesh(mesh, client.Space);
    }

    /// <summary>
    ///     Set the vertices of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="vertices">The vertices.</param>
    internal static unsafe void SetMeshVertices(Mesh mesh, Span<SpatialVertex> vertices)
    {
        Debug.Assert(vertices.Length >= 0);

        fixed (SpatialVertex* vertexData = vertices)
        {
            NativeMethods.SetMeshVertices(mesh, vertexData, vertices.Length);
        }
    }

    /// <summary>
    ///     Set the bounds of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="bounds">The bounds.</param>
    internal static unsafe void SetMeshBounds(Mesh mesh, Span<SpatialBounds> bounds)
    {
        Debug.Assert(bounds.Length >= 0);

        fixed (SpatialBounds* boundsData = bounds)
        {
            NativeMethods.SetMeshBounds(mesh, boundsData, bounds.Length);
        }
    }

    /// <summary>
    ///     Create an effect, an object in 3D space that uses a raster pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline to use to render the effect.</param>
    /// <returns>The effect.</returns>
    internal static Effect CreateEffect(Client client, RasterPipeline pipeline)
    {
        IntPtr effect = NativeMethods.CreateEffect(client, pipeline);

        return new Effect(effect, client.Space);
    }

    /// <summary>
    ///     Set the vertices of an effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="vertices">The vertices.</param>
    internal static unsafe void SetEffectVertices(Effect effect, Span<EffectVertex> vertices)
    {
        Debug.Assert(vertices.Length >= 0);

        fixed (EffectVertex* vertexData = vertices)
        {
            NativeMethods.SetEffectVertices(effect, vertexData, vertices.Length);
        }
    }

    /// <summary>
    ///     Create a raster pipeline. Use this overload if no shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline, or null if the pipeline could not be created.</returns>
    internal static RasterPipeline? CreateRasterPipeline(
        Client client,
        RasterPipelineDescription description,
        Definition.Native.NativeErrorFunc callback)
    {
        Debug.Assert(description.BufferSize == 0);

        IntPtr pipelinePointer = NativeMethods.CreateRasterPipeline(client, description, callback);

        if (pipelinePointer == IntPtr.Zero) return null;

        RasterPipeline pipeline = new(pipelinePointer, client);

        // ReSharper disable once RedundantAssignment
        IntPtr shaderBuffer = NativeMethods.GetRasterPipelineShaderBuffer(pipeline);
        Debug.Assert(shaderBuffer == IntPtr.Zero);

        return pipeline;
    }

    /// <summary>
    ///     Create a raster pipeline. Use this overload if a shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline and associated shader buffer, or null if the pipeline could not be created.</returns>
    internal static unsafe (RasterPipeline, ShaderBuffer<T>)? CreateRasterPipeline<T>(
        Client client,
        RasterPipelineDescription description,
        Definition.Native.NativeErrorFunc callback) where T : unmanaged, IEquatable<T>
    {
        description.BufferSize = (UInt32) sizeof(T);

        IntPtr pipelinePointer = NativeMethods.CreateRasterPipeline(client, description, callback);

        if (pipelinePointer == IntPtr.Zero) return null;

        RasterPipeline pipeline = new(pipelinePointer, client);

        IntPtr shaderBuffer = NativeMethods.GetRasterPipelineShaderBuffer(pipeline);
        Debug.Assert(shaderBuffer != IntPtr.Zero);

        return (pipeline, new ShaderBuffer<T>(shaderBuffer, client));
    }

    /// <summary>
    ///     Add a draw 2D pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline, must use the <see cref="ShaderPresets.ShaderPreset.Draw2D" />.</param>
    /// <param name="priority">The priority, a higher priority means it is executed later and thus on top of other pipelines.</param>
    /// <param name="callback">Callback to be called when the pipeline is executed.</param>
    /// <returns>An object that allows removing the pipeline.</returns>
    internal static IDisposable AddDraw2DPipeline(Client client, RasterPipeline pipeline, Int32 priority, Action<Draw2D> callback)
    {
        Draw2D.Callback draw2dCallback = unmanaged => callback(new Draw2D(Draw2D.InternalMarshaller.ConvertToManaged(unmanaged)));
        UInt32 id = NativeMethods.AddDraw2DPipeline(client, pipeline, priority, draw2dCallback);

        Debug.Assert(!draw2DCallbacks.ContainsKey(id));
        draw2DCallbacks[id] = draw2dCallback;

        return new Disposer(() =>
        {
            Debug.Assert(draw2DCallbacks.ContainsKey(id));

            NativeMethods.RemoveDraw2DPipeline(client, id);
            draw2DCallbacks.Remove(id);
        });
    }

    /// <summary>
    ///     Load a texture from images.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="texture">The texture, consisting of an image for each mip level.</param>
    /// <returns>The loaded texture.</returns>
    internal static unsafe Texture LoadTexture(Client client, Span<Image> texture)
    {
        Debug.Assert(texture.Length > 0);

        TextureDescription description = new()
        {
            Width = (UInt32) texture[index: 0].Width,
            Height = (UInt32) texture[index: 0].Height,
            MipLevels = (UInt32) texture.Length,
            ColorFormat = texture[index: 0].StorageFormat.ToNative()
        };

        Int32** data = stackalloc Int32*[texture.Length];
        var handles = new GCHandle[texture.Length];

        for (var index = 0; index < texture.Length; index++)
        {
            handles[index] = GCHandle.Alloc(texture[index].GetData(texture[index].StorageFormat), GCHandleType.Pinned);
            data[index] = (Int32*) Marshal.UnsafeAddrOfPinnedArrayElement(texture[index].GetData(texture[index].StorageFormat), index: 0);
        }

        IntPtr result = NativeMethods.LoadTexture(client, data, description);

        for (var index = 0; index < texture.Length; index++) handles[index].Free();

        return new Texture(result, client, new Vector2i((Int32) description.Width, (Int32) description.Height));
    }
}

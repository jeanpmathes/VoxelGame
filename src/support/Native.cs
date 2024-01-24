//  <copyright file="Native.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     The bindings for all native functions.
/// </summary>
#pragma warning disable S3242 // The specific types are matched on the native side.
#pragma warning disable S1200 // This class intentionally contains all native functions.
public static class Native // todo: make internal, methods too
{
    private const string DllFilePath = @".\Native.dll";

    private static readonly Dictionary<IntPtr, Camera> cameras = new();

    private static readonly Dictionary<IntPtr, Light> lights = new();

    private static readonly Dictionary<uint, object> draw2DCallbacks = new();

    private static Definition.Native.ScreenshotFunc? screenshotCallback;

    /// <summary>
    ///     Show an error message box.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="caption">The caption of the message box.</param>
    public static void ShowErrorBox(string message, string caption)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeShowErrorBox([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.LPWStr)] string caption);

        NativeShowErrorBox(message, caption);
    }

    /// <summary>
    ///     Initialize the native client.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="onError">The callback for any errors.</param>
    /// <returns>A pointer to the native client.</returns>
    public static IntPtr Initialize(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeConfigure(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError);

        return NativeConfigure(configuration, onError);
    }

    /// <summary>
    ///     Finalize the native client.
    /// </summary>
    /// <param name="client">The client to finalize.</param>
    public static void Finalize(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeFinalize(IntPtr native);

        NativeFinalize(client.Native);
    }

    /// <summary>
    ///     Request that the main window is closed.
    /// </summary>
    public static void RequestClose(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeRequestClose(IntPtr native);

        NativeRequestClose(client.Native);
    }

    /// <summary>
    ///     Start the main loop of the native client. This function will not return until the client is closed.
    /// </summary>
    /// <param name="client">The client to run.</param>
    /// <returns>The exit code.</returns>
    public static int Run(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern int NativeRun(IntPtr native, int nCmdShow);

        const int nCmdShow = 1;

        return NativeRun(client.Native, nCmdShow);
    }

    /// <summary>
    ///     Get current allocator statistics as a string.
    /// </summary>
    public static string GetAllocatorStatistics(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativePassAllocatorStatistics(IntPtr native, Definition.Native.NativeWStringFunc onWString);

        var result = "";

        NativePassAllocatorStatistics(client.Native, s => result = s);

        return result;
    }

    /// <summary>
    ///     Get the DRED (Device Removed Extended Data) string. This is only available in debug builds and after a device
    ///     removal.
    /// </summary>
    public static string GetDRED(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativePassDRED(IntPtr native, Definition.Native.NativeWStringFunc onWString);

        var result = "";

        NativePassDRED(client.Native, s => result = s);

        return result;
    }

    /// <summary>
    ///     Queue a screenshot to be taken. If the screenshot is already queued, this call is ignored.
    /// </summary>
    /// <param name="client">The client for which to take a screenshot.</param>
    /// <param name="callback">The callback to call when the screenshot is taken.</param>
    public static void TakeScreenshot(Client client, Definition.Native.ScreenshotFunc callback)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeTakeScreenshot(IntPtr native, Definition.Native.ScreenshotFunc callback);

        if (screenshotCallback != null) return;

        screenshotCallback = callback;

        NativeTakeScreenshot(client.Native,
            (data, width, height) =>
            {
                Debug.Assert(screenshotCallback != null);

                screenshotCallback(data, width, height);
                screenshotCallback = null;
            });
    }

    /// <summary>
    ///     Toggle fullscreen mode.
    /// </summary>
    /// <param name="client">The client for which to toggle fullscreen.</param>
    public static void ToggleFullscreen(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeToggleFullscreen(IntPtr native);

        NativeToggleFullscreen(client.Native);
    }

    /// <summary>
    ///     Get the current mouse position.
    /// </summary>
    /// <param name="client">The client for which to get the mouse position.</param>
    /// <returns>The current mouse position, in client coordinates.</returns>
    public static (int x, int y) GetMousePosition(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeGetMousePosition(IntPtr native, out long x, out long y);

        NativeGetMousePosition(client.Native, out long x, out long y);

        return ((int) x, (int) y);
    }

    /// <summary>
    ///     Set the mouse position.
    /// </summary>
    /// <param name="client">The client for which to set the mouse position.</param>
    /// <param name="x">The new x position, in client coordinates.</param>
    /// <param name="y">The new y position, in client coordinates.</param>
    public static void SetMousePosition(Client client, int x, int y)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMousePosition(IntPtr native, long x, long y);

        NativeSetMousePosition(client.Native, x, y);
    }

    /// <summary>
    ///     Set the mouse cursor type.
    /// </summary>
    /// <param name="client">The client for which to set the cursor.</param>
    /// <param name="cursor">The cursor type to set.</param>
    public static void SetCursorType(Client client, MouseCursor cursor)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetCursorType(IntPtr native, MouseCursor cursor);

        NativeSetCursorType(client.Native, cursor);
    }

    /// <summary>
    ///     Set whether the cursor should be locked.
    ///     A locked cursor is invisible and cannot leave the window.
    /// </summary>
    /// <param name="client">The client for which to set the cursor lock.</param>
    /// <param name="locked">Whether the cursor should be locked.</param>
    public static void SetCursorLock(Client client, bool locked)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetCursorLock(IntPtr native, bool locked);

        NativeSetCursorLock(client.Native, locked);
    }

    /// <summary>
    /// Because C# cannot transform an array to a pointer of it is a struct member, all arrays are passed as arguments.
    /// </summary>
    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeInitializeRaytracing(IntPtr native,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        ShaderFileDescription[] shaderFiles,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
        string[] symbols,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        MaterialDescription[] materials,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        IntPtr[] textures,
        SpacePipelineDescription description);

    /// <summary>
    ///     Initialize raytracing.
    /// </summary>
    /// <typeparam name="T">The type of the shader buffer.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">A description of the raytracing pipeline.</param>
    /// <returns>The shader buffer, if any is created.</returns>
    public static ShaderBuffer<T>? InitializeRaytracing<T>(Client client, SpacePipeline pipeline) where T : unmanaged, IEquatable<T>
    {
        IntPtr buffer = NativeInitializeRaytracing(client.Native, pipeline.ShaderFiles, pipeline.Symbols, pipeline.Materials, pipeline.TexturePointers, pipeline.Description);

        return buffer == IntPtr.Zero ? null : new ShaderBuffer<T>(buffer, client);
    }

    /// <summary>
    ///     Get the camera of the native client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>The camera.</returns>
    public static Camera GetCamera(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeGetCamera(IntPtr native);

        IntPtr camera = NativeGetCamera(client.Native);
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
    public static Light GetLight(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeGetLight(IntPtr native);

        IntPtr light = NativeGetLight(client.Native);
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
    ///     Set the direction of a light.
    /// </summary>
    /// <param name="light">The light.</param>
    /// <param name="direction">The new direction. Must be normalized.</param>
    public static void SetLightDirection(Light light, Vector3 direction)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetLightDirection(IntPtr light, Vector3 direction);

        NativeSetLightDirection(light.Self, direction);
    }

    /// <summary>
    ///     Update the basic data of a camera.
    /// </summary>
    /// <param name="camera">The camera.</param>
    /// <param name="data">The new data.</param>
    public static void UpdateBasicCameraData(Camera camera, BasicCameraData data)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeUpdateBasicCameraData(IntPtr camera, BasicCameraData data);

        NativeUpdateBasicCameraData(camera.Self, data);
    }

    /// <summary>
    ///     Update the advanced data of a camera.
    /// </summary>
    /// <param name="camera">The camera.</param>
    /// <param name="data">The new data.</param>
    public static void UpdateAdvancedCameraData(Camera camera, AdvancedCameraData data)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeUpdateAdvancedCameraData(IntPtr camera, AdvancedCameraData data);

        NativeUpdateAdvancedCameraData(camera.Self, data);
    }

    /// <summary>
    ///     Update the data of a spatial object.
    /// </summary>
    /// <param name="spatial">The spatial object.</param>
    /// <param name="data">The new data.</param>
    public static void UpdateSpatialData(Spatial spatial, SpatialData data)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeUpdateSpatialData(IntPtr spatial, SpatialData data);

        NativeUpdateSpatialData(spatial.Self, data);
    }

    /// <summary>
    ///     Create a mesh.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="materialIndex">The material index, as defined in pipeline setup.</param>
    /// <returns>The mesh.</returns>
    public static Mesh CreateMesh(Client client, uint materialIndex)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeCreateMesh(IntPtr native, uint materialIndex);

        IntPtr mesh = NativeCreateMesh(client.Native, materialIndex);

        return new Mesh(mesh, client.Space);
    }

    /// <summary>
    ///     Set the vertices of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="vertices">The vertices.</param>
    public static unsafe void SetMeshVertices(Mesh mesh, Span<SpatialVertex> vertices)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMeshVertices(IntPtr mesh, SpatialVertex* vertices, int vertexLength);

        Debug.Assert(vertices.Length >= 0);

        fixed (SpatialVertex* vertexData = vertices)
        {
            NativeSetMeshVertices(mesh.Self, vertexData, vertices.Length);
        }
    }

    /// <summary>
    ///     Set the bounds of a mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="bounds">The bounds.</param>
    public static unsafe void SetMeshBounds(Mesh mesh, Span<SpatialBounds> bounds)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMeshVertices(IntPtr mesh, SpatialBounds* vertices, int vertexLength);

        Debug.Assert(bounds.Length >= 0);

        fixed (SpatialBounds* boundsData = bounds)
        {
            NativeSetMeshVertices(mesh.Self, boundsData, bounds.Length);
        }
    }

    /// <summary>
    ///     Create an effect, an object in 3D space that uses a raster pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline to use to render the effect.</param>
    /// <returns>The effect.</returns>
    public static Effect CreateEffect(Client client, RasterPipeline pipeline)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeCreateEffect(IntPtr native, IntPtr pipeline);

        IntPtr effect = NativeCreateEffect(client.Native, pipeline.Self);

        return new Effect(effect, client.Space);
    }

    /// <summary>
    ///     Set the vertices of an effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="vertices">The vertices.</param>
    public static unsafe void SetEffectVertices(Effect effect, Span<EffectVertex> vertices)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetEffectVertices(IntPtr effect, EffectVertex* vertices, int vertexLength);

        Debug.Assert(vertices.Length >= 0);

        fixed (EffectVertex* vertexData = vertices)
        {
            NativeSetEffectVertices(effect.Self, vertexData, vertices.Length);
        }
    }

    /// <summary>
    ///     Return a drawable to the space pool.
    ///     Using the drawable after this call is not allowed.
    /// </summary>
    /// <param name="drawable">The drawable to return.</param>
    public static void ReturnDrawable(Drawable drawable)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeReturnDrawable(IntPtr native);

        NativeReturnDrawable(drawable.Self);
    }

    /// <summary>
    ///     Set the enabled state of a drawable.
    /// </summary>
    /// <param name="drawable">The drawable.</param>
    /// <param name="enabled">Whether the drawable should be enabled.</param>
    public static void SetDrawableEnabledState(Drawable drawable, bool enabled)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetDrawableEnabledState(IntPtr native, bool enabled);

        NativeSetDrawableEnabledState(drawable.Self, enabled);
    }

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeCreateRasterPipeline(IntPtr native, RasterPipelineDescription description, Definition.Native.NativeErrorFunc callback);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeGetRasterPipelineShaderBuffer(IntPtr rasterPipeline);

    /// <summary>
    ///     Create a raster pipeline. Use this overload if no shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline, or null if the pipeline could not be created.</returns>
    public static RasterPipeline? CreateRasterPipeline(
        Client client,
        RasterPipelineDescription description,
        Definition.Native.NativeErrorFunc callback)
    {
        Debug.Assert(description.BufferSize == 0);

        IntPtr rasterPipeline = NativeCreateRasterPipeline(client.Native, description, callback);

        if (rasterPipeline == IntPtr.Zero) return null;

        IntPtr shaderBuffer = NativeGetRasterPipelineShaderBuffer(rasterPipeline);
        Debug.Assert(shaderBuffer == IntPtr.Zero);

        return new RasterPipeline(rasterPipeline, client);
    }

    /// <summary>
    ///     Create a raster pipeline. Use this overload if a shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline and associated shader buffer, or null if the pipeline could not be created.</returns>
    public static (RasterPipeline, ShaderBuffer<T>)? CreateRasterPipeline<T>(
        Client client,
        RasterPipelineDescription description,
        Definition.Native.NativeErrorFunc callback) where T : unmanaged, IEquatable<T>
    {
        description.BufferSize = (uint) Marshal.SizeOf<T>();

        IntPtr rasterPipeline = NativeCreateRasterPipeline(client.Native, description, callback);

        if (rasterPipeline == IntPtr.Zero) return null;

        IntPtr shaderBuffer = NativeGetRasterPipelineShaderBuffer(rasterPipeline);
        Debug.Assert(shaderBuffer != IntPtr.Zero);

        return (new RasterPipeline(rasterPipeline, client), new ShaderBuffer<T>(shaderBuffer, client));
    }

    /// <summary>
    ///     Set the pipeline that should be used for rendering post processing.
    /// </summary>
    public static void SetPostProcessingPipeline(Client client, RasterPipeline pipeline)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeDesignatePostProcessingPipeline(IntPtr native, IntPtr pipeline);

        NativeDesignatePostProcessingPipeline(client.Native, pipeline.Self);
    }

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern unsafe void NativeSetShaderBufferData(IntPtr shaderBuffer, void* data);

    /// <summary>
    ///     Set the data of a shader buffer.
    /// </summary>
    /// <param name="shaderBuffer">The shader buffer.</param>
    /// <param name="data">The data to set.</param>
    /// <typeparam name="T">The type of the data.</typeparam>
    public static unsafe void SetShaderBufferData<T>(ShaderBuffer<T> shaderBuffer, T data) where T : unmanaged, IEquatable<T>
    {
        T* dataPtr = &data;
        NativeSetShaderBufferData(shaderBuffer.Self, dataPtr);
    }

    /// <summary>
    ///     Add a draw 2D pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline, must use the <see cref="ShaderPresets.ShaderPreset.Draw2D"/>.</param>
    /// <param name="priority">The priority, a higher priority means it is executed later and thus on top of other pipelines.</param>
    /// <param name="callback">Callback to be called when the pipeline is executed.</param>
    /// <returns>An object that allows removing the pipeline.</returns>
    public static IDisposable AddDraw2DPipeline(Client client, RasterPipeline pipeline, int priority, Action<Draw2D> callback)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern uint NativeAddDraw2DPipeline(IntPtr native, IntPtr pipeline, int priority, Draw2D.Callback callback);

        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeRemoveDraw2DPipeline(IntPtr native, uint id);

        Draw2D.Callback draw2dCallback = @internal => callback(new Draw2D(@internal));
        uint id = NativeAddDraw2DPipeline(client.Native, pipeline.Self, priority, draw2dCallback);

        Debug.Assert(!draw2DCallbacks.ContainsKey(id));
        draw2DCallbacks[id] = draw2dCallback;

        return new Disposer(() =>
        {
            Debug.Assert(draw2DCallbacks.ContainsKey(id));

            NativeRemoveDraw2DPipeline(client.Native, id);
            draw2DCallbacks.Remove(id);
        });
    }

    /// <summary>
    ///     Load a texture from images.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="texture">The texture, consisting of an image for each mip level.</param>
    /// <returns>The loaded texture.</returns>
    public static unsafe Texture LoadTexture(Client client, Span<Image> texture)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeLoadTexture(IntPtr client, int** data, TextureDescription description);

        Debug.Assert(texture.Length > 0);

        Image.Format format = texture[index: 0].StorageFormat;

        TextureDescription description = new()
        {
            Width = (uint) texture[index: 0].Width,
            Height = (uint) texture[index: 0].Height,
            MipLevels = (uint) texture.Length,
            ColorFormat = format.ToNative()
        };

        List<GCHandle> pins = new(texture.Length);
        var subresources = new int*[texture.Length];

        for (var index = 0; index < texture.Length; index++)
        {
            int[] data = texture[index].GetData(format);
            GCHandle pin = GCHandle.Alloc(data, GCHandleType.Pinned);

            pins.Add(pin);
            subresources[index] = (int*) pin.AddrOfPinnedObject();
        }

        IntPtr result;

        fixed (int** subresourcesPtr = subresources)
        {
            result = NativeLoadTexture(client.Native, subresourcesPtr, description);
        }

        for (var i = 0; i < texture.Length; i++) pins[i].Free();

        return new Texture(result, client, new Vector2i((int) description.Width, (int) description.Height));
    }

    /// <summary>
    ///     Free a texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    public static void FreeTexture(Texture texture)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeFreeTexture(IntPtr texture);

        NativeFreeTexture(texture.Self);
    }
}

//  <copyright file="Native.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     The bindings for all native functions.
/// </summary>
public static class Native
{
    private const string DllFilePath = @".\Native.dll";

    /// <summary>
    ///     Show an error message box.
    /// </summary>
    /// <param name="message">The error message.</param>
    #pragma warning disable S3242 // The specific types are matched on the native side.
    public static void ShowErrorBox(string message)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeShowErrorBox([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.LPWStr)] string caption);

        NativeShowErrorBox(message, "Error");
    }

    /// <summary>
    ///     Initialize the native client.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="onError">The callback for any errors.</param>
    /// <param name="onErrorMessage">The callback for any error messages.</param>
    /// <returns>A pointer to the native client.</returns>
    public static IntPtr Initialize(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError, Definition.Native.NativeErrorMessageFunc onErrorMessage)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeConfigure(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError, Definition.Native.NativeErrorMessageFunc onErrorMessage);

        return NativeConfigure(configuration, onError, onErrorMessage);
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
    ///     Set the mouse cursor.
    /// </summary>
    /// <param name="client">The client for which to set the cursor.</param>
    /// <param name="cursor">The cursor to set.</param>
    public static void SetCursor(Client client, MouseCursor cursor)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetCursor(IntPtr native, MouseCursor cursor);

        NativeSetCursor(client.Native, cursor);
    }

    /// <summary>
    ///     Initialize raytracing.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">A description of the raytracing pipeline.</param>
    public static void InitializeRaytracing(Client client, SpacePipeline pipeline)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeInitializeRaytracing(IntPtr native,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
            ShaderFileDescription[] shaderFiles,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
            string[] symbols,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
            MaterialDescription[] materials,
            SpacePipelineDescription description);

        NativeInitializeRaytracing(client.Native, pipeline.ShaderFiles, pipeline.Symbols, pipeline.Materials, pipeline.Description);
    }

    private static readonly Dictionary<IntPtr, Camera> cameras = new();

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

        if (cameras.ContainsKey(camera))
        {
            cameraObject = cameras[camera];
        }
        else
        {
            cameraObject = new Camera(camera, client.Space);
            cameras.Add(camera, cameraObject);
        }

        return cameraObject;
    }

    private static readonly Dictionary<IntPtr, Light> lights = new();

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

        if (lights.ContainsKey(light))
        {
            lightObject = lights[light];
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
    /// <param name="spatialObject">The spatial object.</param>
    /// <param name="data">The new data.</param>
    public static void UpdateSpatialObjectData(SpatialObject spatialObject, SpatialObjectData data)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeUpdateSpatialObjectData(IntPtr spatialObject, SpatialObjectData data);

        NativeUpdateSpatialObjectData(spatialObject.Self, data);
    }

    /// <summary>
    ///     Create an indexed mesh object.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="materialIndex">The material index, as defined in pipeline setup.</param>
    /// <returns>The indexed mesh object.</returns>
    public static MeshObject CreateMeshObject(Client client, uint materialIndex)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeCreateMeshObject(IntPtr native, uint materialIndex);

        IntPtr indexedMeshObject = NativeCreateMeshObject(client.Native, materialIndex);

        return new MeshObject(indexedMeshObject, client.Space);
    }

    /// <summary>
    ///     Free a mesh object.
    /// </summary>
    /// <param name="meshObject">The mesh object to delete.</param>
    public static void FreeMeshObject(MeshObject meshObject)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeFreeMeshObject(IntPtr native);

        NativeFreeMeshObject(meshObject.Self);
    }

    /// <summary>
    ///     Set the enabled state of a mesh object.
    /// </summary>
    /// <param name="meshObject">The mesh object.</param>
    /// <param name="enabled">Whether the mesh object should be enabled.</param>
    public static void SetMeshObjectEnabledState(MeshObject meshObject, bool enabled)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMeshObjectEnabledState(IntPtr native, bool enabled);

        NativeSetMeshObjectEnabledState(meshObject.Self, enabled);
    }

    /// <summary>
    ///     Set the mesh data of an indexed mesh object.
    /// </summary>
    /// <param name="meshObject">The indexed mesh object.</param>
    /// <param name="vertices">The vertices.</param>
    public static unsafe void SetMeshObjectData(MeshObject meshObject, Span<SpatialVertex> vertices)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMeshObjectMesh(IntPtr indexedMeshObject, SpatialVertex* vertices, int vertexLength);

        Debug.Assert(vertices.Length >= 0);

        fixed (SpatialVertex* vertexData = vertices)
        {
            NativeSetMeshObjectMesh(meshObject.Self, vertexData, vertices.Length);
        }
    }

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeCreateRasterPipeline(IntPtr native, PipelineDescription description, Definition.Native.NativeErrorFunc callback);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeGetShaderBuffer(IntPtr rasterPipeline);

    /// <summary>
    ///     Create a raster pipeline. Use this overload if no shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline.</returns>
    public static RasterPipeline CreateRasterPipeline(Client client,
        PipelineDescription description, Definition.Native.NativeErrorFunc callback)
    {
        Debug.Assert(description.BufferSize == 0);

        IntPtr rasterPipeline = NativeCreateRasterPipeline(client.Native, description, callback);
        IntPtr shaderBuffer = NativeGetShaderBuffer(rasterPipeline);

        Debug.Assert(shaderBuffer == IntPtr.Zero);

        return new RasterPipeline(rasterPipeline, client);
    }

    /// <summary>
    ///     Create a raster pipeline. Use this overload if a shader buffer is needed.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="description">A description of the pipeline to create.</param>
    /// <param name="callback">A callback to receive error messages related to shader compilation.</param>
    /// <returns>The raster pipeline and associated shader buffer.</returns>
    public static (RasterPipeline, ShaderBuffer<T>) CreateRasterPipeline<T>(Client client,
        PipelineDescription description, Definition.Native.NativeErrorFunc callback) where T : unmanaged
    {
        description.BufferSize = (uint) Marshal.SizeOf<T>();

        IntPtr rasterPipeline = NativeCreateRasterPipeline(client.Native, description, callback);
        IntPtr shaderBuffer = NativeGetShaderBuffer(rasterPipeline);

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
    public static unsafe void SetShaderBufferData<T>(ShaderBuffer<T> shaderBuffer, T data) where T : unmanaged
    {
        T* dataPtr = &data;
        NativeSetShaderBufferData(shaderBuffer.Self, dataPtr);
    }

    private static readonly Dictionary<RasterPipeline, object> draw2DCallbacks = new();

    /// <summary>
    ///     Add a draw 2D pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="callback">Callback to be called when the pipeline is executed.</param>
    public static void AddDraw2DPipeline(Client client, RasterPipeline pipeline, Action<Draw2D> callback)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeAddDraw2DPipeline(IntPtr native, IntPtr pipeline, Draw2D.Callback callback);

        Debug.Assert(!draw2DCallbacks.ContainsKey(pipeline));

        // ReSharper disable once ConvertToLocalFunction - we need to keep the callback alive
        Draw2D.Callback draw2dCallback = @internal => callback(new Draw2D(@internal));
        draw2DCallbacks[pipeline] = draw2dCallback;
        NativeAddDraw2DPipeline(client.Native, pipeline.Self, draw2dCallback);
    }

    /// <summary>
    ///     Remove a draw 2D pipeline.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="pipeline">The pipeline.</param>
    public static void RemoveDraw2DPipeline(Client client, RasterPipeline pipeline)
    {
        Debug.Assert(draw2DCallbacks.ContainsKey(pipeline));

        // todo: implement NativeRemoveDraw2DPipeline, then call it here
        // todo: all users of AddDraw2DPipeline should call RemoveDraw2DPipeline when they are done (e.g. dispose)

        draw2DCallbacks.Remove(pipeline);
    }

    /// <summary>
    ///     Load a texture from a bitmap.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="bitmaps">All bitmaps used to build the texture.</param>
    /// <returns>The loaded texture.</returns>
    public static unsafe Texture LoadTexture(Client client, Span<Bitmap> bitmaps)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeLoadTexture(IntPtr client, IntPtr* data, TextureDescription description);

        Debug.Assert(bitmaps.Length > 0);

        TextureDescription description = new()
        {
            Width = (uint) bitmaps[index: 0].Width,
            Height = (uint) bitmaps[index: 0].Height,
            Depth = (uint) bitmaps.Length
        };

        List<IntPtr> subresources = new(bitmaps.Length);
        List<BitmapData> locks = new(bitmaps.Length);

        foreach (Bitmap bitmap in bitmaps)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(x: 0, y: 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            subresources.Add(data.Scan0);
            locks.Add(data);
        }

        IntPtr texture;

        fixed (IntPtr* subresourcesPtr = CollectionsMarshal.AsSpan(subresources))
        {
            texture = NativeLoadTexture(client.Native, subresourcesPtr, description);
        }

        for (var i = 0; i < bitmaps.Length; i++) bitmaps[i].UnlockBits(locks[i]);

        return new Texture(texture, client, new Vector2i((int) description.Width, (int) description.Height));
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

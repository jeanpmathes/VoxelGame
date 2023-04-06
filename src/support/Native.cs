//  <copyright file="Native.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     The bindings for all native functions.
/// </summary>
public static class Native
{
    /// <summary>
    ///     Show an error message box.
    /// </summary>
    /// <param name="message">The error message.</param>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    #pragma warning disable S3242 // The specific types are matched on the native side.
    public static void ShowErrorBox(string message)
    {
        [DllImport("user32.dll")]
        static extern int MessageBoxW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpText, [MarshalAs(UnmanagedType.LPWStr)] string lpCaption, uint uType);

        const uint MB_OK = 0x00000000;
        const uint MB_ICONERROR = 0x00000010;
        const uint MB_SYSTEMMODAL = 0x00001000;

        int result = MessageBoxW(IntPtr.Zero, message, "Error", MB_OK | MB_ICONERROR | MB_SYSTEMMODAL);
        Marshal.ThrowExceptionForHR(result);
    }

    private const string DllFilePath = @".\Native.dll";

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
    /// <param name="native">A pointer to the native client.</param>
    public static void Finalize(IntPtr native)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeFinalize(IntPtr native);

        NativeFinalize(native);
    }

    /// <summary>
    ///     Start the main loop of the native client. This function will not return until the client is closed.
    /// </summary>
    /// <param name="native">A pointer to the native client.</param>
    /// <returns>The exit code.</returns>
    public static int Run(IntPtr native)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern int NativeRun(IntPtr native, int nCmdShow);

        const int nCmdShow = 1;

        return NativeRun(native, nCmdShow);
    }

    /// <summary>
    ///     Set the resolution of the window.
    /// </summary>
    /// <param name="native">A pointer to the native client.</param>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    public static void SetResolution(IntPtr native, uint width, uint height)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetResolution(IntPtr native, uint width, uint height);

        NativeSetResolution(native, width, height);
    }

    /// <summary>
    ///     Toggle fullscreen mode.
    /// </summary>
    /// <param name="native">A pointer to the native client.</param>
    public static void ToggleFullscreen(IntPtr native)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeToggleFullscreen(IntPtr native);

        NativeToggleFullscreen(native);
    }

    /// <summary>
    ///     Get the current mouse position.
    /// </summary>
    /// <param name="native">A pointer to the native client.</param>
    /// <returns>The current mouse position, in client coordinates.</returns>
    public static (int x, int y) GetMousePosition(IntPtr native)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeGetMousePosition(IntPtr native, out long x, out long y);

        NativeGetMousePosition(native, out long x, out long y);

        return ((int) x, (int) y);
    }

    /// <summary>
    ///     Set the mouse position.
    /// </summary>
    /// <param name="native">A pointer to the native client.</param>
    /// <param name="x">The new x position, in client coordinates.</param>
    /// <param name="y">The new y position, in client coordinates.</param>
    public static void SetMousePosition(IntPtr native, int x, int y)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetMousePosition(IntPtr native, long x, long y);

        NativeSetMousePosition(native, x, y);
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
    ///     Update the data of a camera.
    /// </summary>
    /// <param name="camera">The camera.</param>
    /// <param name="data">The new data.</param>
    public static void UpdateCameraData(Camera camera, CameraData data)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeUpdateCameraData(IntPtr camera, CameraData data);

        NativeUpdateCameraData(camera.Self, data);
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
    ///     Create a sequenced mesh object.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>The sequenced mesh object.</returns>
    public static SequencedMeshObject CreateSequencedMeshObject(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeCreateSequencedMeshObject(IntPtr native);

        IntPtr sequencedMeshObject = NativeCreateSequencedMeshObject(client.Native);

        return new SequencedMeshObject(sequencedMeshObject, client.Space);
    }

    /// <summary>
    ///     Create an indexed mesh object.
    ///     The lengths allow to use only a part of the arrays.
    /// </summary>
    /// <param name="sequencedMeshObject">The sequenced mesh object.</param>
    /// <param name="vertices">The vertices.</param>
    /// <param name="length">The number of vertices. Must be less than or equal to the length of the vertices array.</param>
    public static unsafe void SetSequencedMeshObjectData(SequencedMeshObject sequencedMeshObject, SpatialVertex[] vertices, int length)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetSequencedMeshObjectMesh(IntPtr sequencedMeshObject, SpatialVertex* vertices, int length);

        Debug.Assert(length <= vertices.Length);
        Debug.Assert(length >= 0);

        fixed (SpatialVertex* vertexData = vertices)
        {
            NativeSetSequencedMeshObjectMesh(sequencedMeshObject.Self, vertexData, length);
        }
    }

    /// <summary>
    ///     Create an indexed mesh object.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>The indexed mesh object.</returns>
    public static IndexedMeshObject CreateIndexedMeshObject(Client client)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeCreateIndexedMeshObject(IntPtr native);

        IntPtr indexedMeshObject = NativeCreateIndexedMeshObject(client.Native);

        return new IndexedMeshObject(indexedMeshObject, client.Space);
    }

    /// <summary>
    ///     Create an indexed mesh object.
    ///     The lengths allow to use only a part of the arrays.
    /// </summary>
    /// <param name="indexedMeshObject">The indexed mesh object.</param>
    /// <param name="vertices">The vertices.</param>
    /// <param name="vertexLength">The number of vertices. Must be less than or equal to the length of the vertices array.</param>
    /// <param name="indices">The indices.</param>
    /// <param name="indexLength">The number of indices. Must be less than or equal to the length of the indices array.</param>
    public static unsafe void SetIndexedMeshObjectData(IndexedMeshObject indexedMeshObject, SpatialVertex[] vertices, int vertexLength, uint[] indices, int indexLength)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern void NativeSetIndexedMeshObjectMesh(IntPtr indexedMeshObject, SpatialVertex* vertices, int vertexLength, uint* indices, int indexLength);

        Debug.Assert(vertexLength <= vertices.Length);
        Debug.Assert(vertexLength >= 0);

        Debug.Assert(indexLength <= indices.Length);
        Debug.Assert(indexLength >= 0);

        fixed (SpatialVertex* vertexData = vertices)
        fixed (uint* indexData = indices)
        {
            NativeSetIndexedMeshObjectMesh(indexedMeshObject.Self, vertexData, vertexLength, indexData, indexLength);
        }
    }

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    private static extern IntPtr NativeCreateRasterPipeline(IntPtr native, PipelineDescription description, Definition.Native.NativeErrorMessageFunc callback);

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
        PipelineDescription description, Definition.Native.NativeErrorMessageFunc callback)
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
        PipelineDescription description, Definition.Native.NativeErrorMessageFunc callback) where T : unmanaged
    {
        description.BufferSize = (ulong) Marshal.SizeOf<T>();

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
    /// <param name="bitmap">The bitmap.</param>
    /// <returns>The loaded texture.</returns>
    public static Texture LoadTexture(Client client, Bitmap bitmap)
    {
        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        static extern IntPtr NativeLoadTexture(IntPtr client, IntPtr data, TextureDescription description);

        TextureDescription description = new()
        {
            Width = (uint) bitmap.Width,
            Height = (uint) bitmap.Height
        };

        BitmapData data = bitmap.LockBits(new Rectangle(x: 0, y: 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        IntPtr texture = NativeLoadTexture(client.Native, data.Scan0, description);
        bitmap.UnlockBits(data);

        return new Texture(texture, client);
    }
}

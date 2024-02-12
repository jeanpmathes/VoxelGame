// <copyright file="NativeMethods.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Support.Data;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;

namespace VoxelGame.Support;

internal static class NativeMethods
{
    private const string DllFilePath = @".\Native.dll";

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeShowErrorBox([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.LPWStr)] string caption);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeConfigure(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeFinalize(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeRequestClose(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern int NativeRun(IntPtr native, int nCmdShow);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativePassAllocatorStatistics(IntPtr native, Definition.Native.NativeWStringFunc onWString);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativePassDRED(IntPtr native, Definition.Native.NativeWStringFunc onWString);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeTakeScreenshot(IntPtr native, Definition.Native.ScreenshotFunc callback);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeToggleFullscreen(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeGetMousePosition(IntPtr native, out long x, out long y);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeSetMousePosition(IntPtr native, long x, long y);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeSetCursorType(IntPtr native, MouseCursor cursor);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeSetCursorLock(IntPtr native, bool locked);

    /// <summary>
    ///     Because C# cannot transform an array to a pointer of it is a struct member, all arrays are passed as arguments.
    /// </summary>
    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeInitializeRaytracing(IntPtr native,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        ShaderFileDescription[] shaderFiles,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
        string[] symbols,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        MaterialDescription[] materials,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)]
        IntPtr[] textures,
        SpacePipelineDescription description);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeGetCamera(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeGetLight(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeSetLightDirection(IntPtr light, Vector3 direction);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeUpdateBasicCameraData(IntPtr camera, BasicCameraData data);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeUpdateAdvancedCameraData(IntPtr camera, AdvancedCameraData data);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeUpdateSpatialData(IntPtr spatial, SpatialData data);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeCreateMesh(IntPtr native, uint materialIndex);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern unsafe void NativeSetMeshVertices(IntPtr mesh, SpatialVertex* vertices, int vertexLength);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern unsafe void NativeSetMeshVertices(IntPtr mesh, SpatialBounds* vertices, int vertexLength);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeCreateEffect(IntPtr native, IntPtr pipeline);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern unsafe void NativeSetEffectVertices(IntPtr effect, EffectVertex* vertices, int vertexLength);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeReturnDrawable(IntPtr native);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeSetDrawableEnabledState(IntPtr native, bool enabled);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeCreateRasterPipeline(IntPtr native, RasterPipelineDescription description, Definition.Native.NativeErrorFunc callback);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern IntPtr NativeGetRasterPipelineShaderBuffer(IntPtr rasterPipeline);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeDesignatePostProcessingPipeline(IntPtr native, IntPtr pipeline);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern unsafe void NativeSetShaderBufferData(IntPtr shaderBuffer, void* data);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern uint NativeAddDraw2DPipeline(IntPtr native, IntPtr pipeline, int priority, Draw2D.Callback callback);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeRemoveDraw2DPipeline(IntPtr native, uint id);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern unsafe IntPtr NativeLoadTexture(IntPtr client, int** data, TextureDescription description);

    [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
    internal static extern void NativeFreeTexture(IntPtr texture);
}

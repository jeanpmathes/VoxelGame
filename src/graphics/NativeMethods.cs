// <copyright file="NativeMethods.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Interop;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics;

internal static partial class NativeMethods
{
    private const String DllFilePath = @".\NativeGraphics.dll";

    [LibraryImport(DllFilePath, EntryPoint = "NativeShowErrorBox")]
    internal static partial void ShowErrorBox([MarshalAs(UnmanagedType.LPWStr)] String text, [MarshalAs(UnmanagedType.LPWStr)] String caption);

    [LibraryImport(DllFilePath, EntryPoint = "NativeConfigure")]
    internal static partial IntPtr Configure(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunc onError);

    [LibraryImport(DllFilePath, EntryPoint = "NativeFinalize")]
    internal static partial void Finalize(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRequestClose")]
    internal static partial void RequestClose(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRun")]
    internal static partial Int32 Run(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativePassAllocatorStatistics")]
    internal static partial void PassAllocatorStatistics(Client client, Definition.Native.NativeWStringFunc onWString);

    [LibraryImport(DllFilePath, EntryPoint = "NativePassDRED")]
    internal static partial void PassDRED(Client client, Definition.Native.NativeWStringFunc onWString);

    [LibraryImport(DllFilePath, EntryPoint = "NativeTakeScreenshot")]
    internal static partial void TakeScreenshot(Client client, Definition.Native.ScreenshotFunc callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeToggleFullscreen")]
    internal static partial void ToggleFullscreen(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetMousePosition")]
    internal static partial void GetMousePosition(Client client, out Int64 x, out Int64 y);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMousePosition")]
    internal static partial void SetMousePosition(Client client, Int64 x, Int64 y);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetCursorType")]
    internal static partial void SetCursorType(Client client, MouseCursor cursor);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetCursorLock")]
    internal static partial void SetCursorLock(Client client, Bool locked);

    [LibraryImport(DllFilePath, EntryPoint = "NativeInitializeRaytracing")]
    internal static partial IntPtr InitializeRaytracing(Client client, SpacePipelineDescription description);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetCamera")]
    internal static partial IntPtr GetCamera(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetLight")]
    internal static partial IntPtr GetLight(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetLightDirection")]
    internal static partial void SetLightDirection(Light light, [MarshalUsing(typeof(Vector3Marshaller))] Vector3 direction);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateBasicCameraData")]
    internal static partial void UpdateBasicCameraData(Camera camera, BasicCameraData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateAdvancedCameraData")]
    internal static partial void UpdateAdvancedCameraData(Camera camera, AdvancedCameraData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateSpatialData")]
    internal static partial void UpdateSpatialData(Spatial spatial, SpatialData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateMesh")]
    internal static partial IntPtr CreateMesh(Client client, UInt32 materialIndex);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMeshVertices")]
    internal static unsafe partial void SetMeshVertices(Mesh mesh, SpatialVertex* vertices, Int32 vertexLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMeshBounds")]
    internal static unsafe partial void SetMeshBounds(Mesh mesh, SpatialBounds* vertices, Int32 boundLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateEffect")]
    internal static partial IntPtr CreateEffect(Client client, RasterPipeline pipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetEffectVertices")]
    internal static unsafe partial void SetEffectVertices(Effect effect, EffectVertex* vertices, Int32 vertexLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeReturnDrawable")]
    internal static partial void ReturnDrawable(Drawable drawable);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetDrawableEnabledState")]
    internal static partial void SetDrawableEnabledState(Drawable drawable, Bool enabled);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateRasterPipeline")]
    internal static partial IntPtr CreateRasterPipeline(Client client, RasterPipelineDescription description, Definition.Native.NativeErrorFunc callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetRasterPipelineShaderBuffer")]
    internal static partial IntPtr GetRasterPipelineShaderBuffer(RasterPipeline rasterPipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeDesignatePostProcessingPipeline")]
    internal static partial void DesignatePostProcessingPipeline(Client client, RasterPipeline pipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetShaderBufferData")]
    internal static unsafe partial void SetShaderBufferData(ShaderBuffer shaderBuffer, void* data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeAddDraw2DPipeline")]
    internal static partial UInt32 AddDraw2DPipeline(Client client, RasterPipeline pipeline, Int32 priority, Draw2D.Callback callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRemoveDraw2DPipeline")]
    internal static partial void RemoveDraw2DPipeline(Client client, UInt32 id);

    [LibraryImport(DllFilePath, EntryPoint = "NativeLoadTexture")]
    internal static unsafe partial IntPtr LoadTexture(Client client, Int32** data, TextureDescription description);

    [LibraryImport(DllFilePath, EntryPoint = "NativeFreeTexture")]
    internal static partial void FreeTexture(Texture texture);
}

// <copyright file="NativeMethods.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Core.Visuals.Colors;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Definition.UserInterface;
using VoxelGame.Graphics.Interfaces;
using VoxelGame.Graphics.Interop;
using VoxelGame.Graphics.Objects;
using VoxelGame.Graphics.Objects.UserInterface;
using VoxelGame.GUI.Utilities;
using VoxelGame.Toolkit.Interop;
using Mesh = VoxelGame.Graphics.Objects.Mesh;

namespace VoxelGame.Graphics;

internal static partial class NativeMethods
{
    private const String DllFilePath = @".\NativeGraphics.dll";
    private const DllImportSearchPath SearchPath = DllImportSearchPath.AssemblyDirectory;

    [LibraryImport(DllFilePath, EntryPoint = "NativeShowErrorBox")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ShowErrorBox([MarshalAs(UnmanagedType.LPWStr)] String text, [MarshalAs(UnmanagedType.LPWStr)] String caption);

    [LibraryImport(DllFilePath, EntryPoint = "NativeConfigure")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr Configure(Definition.Native.NativeConfiguration configuration, Definition.Native.NativeErrorFunction onError);

    [LibraryImport(DllFilePath, EntryPoint = "NativeFinalize")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void Finalize(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRequestClose")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void RequestClose(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRun")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial Int32 Run(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetTimeScale")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetTimeScale(Client client, Double timeScale);

    [LibraryImport(DllFilePath, EntryPoint = "NativePassAllocatorStatistics")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void PassAllocatorStatistics(Client client, Definition.Native.NativeWStringFunction onWString);

    [LibraryImport(DllFilePath, EntryPoint = "NativePassDRED")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void PassDRED(Client client, Definition.Native.NativeWStringFunction onWString);

    [LibraryImport(DllFilePath, EntryPoint = "NativeTakeScreenshot")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void TakeScreenshot(Client client, Definition.Native.ScreenshotFunc callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeToggleFullscreen")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ToggleFullscreen(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetMousePosition")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void GetMousePosition(Client client, out Int64 x, out Int64 y);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMousePosition")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetMousePosition(Client client, Int64 x, Int64 y);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetCursorType")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetCursorType(Client client, MouseCursor cursor);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetCursorLock")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetCursorLock(Client client, Bool locked);

    [LibraryImport(DllFilePath, EntryPoint = "NativeInitializeRaytracing")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr InitializeRaytracing(Client client, SpacePipelineDescription description);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetCamera")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr GetCamera(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetLight")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr GetLight(Client client);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetSpaceIsRendered")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetSpaceIsRendered(Client client, Bool isRendered);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetLightConfiguration")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetLightConfiguration(
        Light light,
        [MarshalUsing(typeof(Vector3Marshaller))]
        Vector3 direction,
        [MarshalUsing(typeof(Vector3Marshaller))]
        Vector3 color,
        Single intensity);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateBasicCameraData")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void UpdateBasicCameraData(Camera camera, BasicCameraData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateAdvancedCameraData")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void UpdateAdvancedCameraData(Camera camera, AdvancedCameraData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeUpdateSpatialData")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void UpdateSpatialData(Spatial spatial, SpatialData data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateMesh")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateMesh(Client client, UInt32 materialIndex);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMeshVertices")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial void SetMeshVertices(Mesh mesh, SpatialVertex* vertices, Int32 vertexLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetMeshBounds")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial void SetMeshBounds(Mesh mesh, SpatialBounds* vertices, Int32 boundLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateEffect")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateEffect(Client client, RasterPipeline pipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetEffectVertices")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial void SetEffectVertices(Effect effect, EffectVertex* vertices, Int32 vertexLength);

    [LibraryImport(DllFilePath, EntryPoint = "NativeReturnDrawable")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ReturnDrawable(Drawable drawable);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetDrawableEnabledState")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void SetDrawableEnabledState(Drawable drawable, Bool enabled);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateRasterPipeline")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateRasterPipeline(Client client, RasterPipelineDescription description, Definition.Native.NativeErrorFunction callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeGetRasterPipelineShaderBuffer")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr GetRasterPipelineShaderBuffer(RasterPipeline rasterPipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeDesignatePostProcessingPipeline")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void DesignatePostProcessingPipeline(Client client, RasterPipeline pipeline);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSetShaderBufferData")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial void SetShaderBufferData(ShaderBuffer shaderBuffer, void* data);

    [LibraryImport(DllFilePath, EntryPoint = "NativeAddDraw2DPipeline")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial UInt32 AddDraw2DPipeline(Client client, RasterPipeline pipeline, Int32 priority, Draw2D.Callback callback);

    [LibraryImport(DllFilePath, EntryPoint = "NativeRemoveDraw2DPipeline")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void RemoveDraw2DPipeline(Client client, UInt32 id);

    [LibraryImport(DllFilePath, EntryPoint = "NativeLoadTexture")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial IntPtr LoadTexture(Client client, Int32** data, TextureDescription description);

    [LibraryImport(DllFilePath, EntryPoint = "NativeFreeTexture")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void FreeTexture(Texture texture);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateUserInterface")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateUserInterface(Client client, Int32 priority);

    [LibraryImport(DllFilePath, EntryPoint = "NativeFreeUserInterface")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void FreeUserInterface(Renderer renderer);

    [LibraryImport(DllFilePath, EntryPoint = "NativeSubmitUserInterfaceCommands")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static unsafe partial void SubmitUserInterfaceCommands(Renderer renderer, Command* commands, UInt32 commandCount);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateUserInterfaceSolidColorBrush")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateUserInterfaceSolidColorBrush(Renderer renderer, ColorS color);

    [LibraryImport(DllFilePath, EntryPoint = "NativeReturnUserInterfaceBrush")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ReturnUserInterfaceBrush(Brush brush);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateUserInterfaceTextFormat")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateUserInterfaceTextFormat(Renderer renderer, TextFormatDescription description);

    [LibraryImport(DllFilePath, EntryPoint = "NativeReturnUserInterfaceTextFormat")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ReturnUserInterfaceTextFormat(TextFormat format);

    [LibraryImport(DllFilePath, EntryPoint = "NativeCreateUserInterfaceText")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial IntPtr CreateUserInterfaceText(
        Renderer renderer,
        [MarshalAs(UnmanagedType.LPWStr)] String text,
        UInt32 textLength,
        TextFormat format);

    [LibraryImport(DllFilePath, EntryPoint = "NativeReturnUserInterfaceText")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial void ReturnUserInterfaceText(Text text);

    [LibraryImport(DllFilePath, EntryPoint = "NativeMeasureUserInterfaceText")]
    [DefaultDllImportSearchPaths(SearchPath)]
    internal static partial Size MeasureUserInterfaceText(Text text, Size availableSize);
}

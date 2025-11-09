// <copyright file="SpacePipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using VoxelGame.Graphics.Interop;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Additional information describing the raytracing pipeline.
/// </summary>
[NativeMarshalling(typeof(SpacePipelineDescriptionMarshaller))]
internal struct SpacePipelineDescription
{
    internal ShaderFileDescription[] shaderFiles;
    internal String[] symbols;

    internal MaterialDescription[] materials;

    internal Texture[] textures;
    internal UInt32 textureCountFirstSlot;
    internal UInt32 textureCountSecondSlot;

    internal UInt32 customDataBufferSize;

    internal UInt32 meshSpoolCount;
    internal UInt32 effectSpoolCount;

    internal Native.NativeErrorFunc onShaderLoadingError;
}

[CustomMarshaller(typeof(SpacePipelineDescription), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
internal unsafe ref struct SpacePipelineDescriptionMarshaller
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal ShaderFileDescriptionMarshaller.Unmanaged* shaderFiles;
        internal UInt32 shaderFileCount;

        internal IntPtr* symbols;

        internal MaterialDescriptionMarshaller.Unmanaged* materials;
        internal UInt32 materialCount;

        internal IntPtr* textures;
        internal UInt32 textureCountFirstSlot;
        internal UInt32 textureCountSecondSlot;

        internal UInt32 customDataBufferSize;

        internal UInt32 meshSpoolCount;
        internal UInt32 effectSpoolCount;

        internal IntPtr onShaderLoadingError;
    }

    internal ref struct ManagedToUnmanagedIn
    {
        private Unmanaged unmanaged;

        private UInt32 symbolCount;
        private UInt32 textureCount;

        internal void FromManaged(SpacePipelineDescription managed)
        {
            unmanaged = new Unmanaged
            {
                shaderFiles = Marshalling.ConvertToUnmanaged<ShaderFileDescription, ShaderFileDescriptionMarshaller.Unmanaged,
                    ShaderFileDescriptionMarshaller.Marshaller>(managed.shaderFiles, out UInt32 shaderFileCount),
                shaderFileCount = shaderFileCount,
                symbols = Marshalling.ConvertToUnmanaged<String, IntPtr,
                    UnicodeStringMarshaller>(managed.symbols, out symbolCount),
                materials = Marshalling.ConvertToUnmanaged<MaterialDescription, MaterialDescriptionMarshaller.Unmanaged,
                    MaterialDescriptionMarshaller.Marshaller>(managed.materials, out UInt32 materialCount),
                materialCount = materialCount,
                textures = Marshalling.ConvertToUnmanaged<Texture, IntPtr,
                    NativeObjectMarshaller.Marshaller>(managed.textures, out textureCount),
                textureCountFirstSlot = managed.textureCountFirstSlot,
                textureCountSecondSlot = managed.textureCountSecondSlot,
                customDataBufferSize = managed.customDataBufferSize,
                onShaderLoadingError = Marshal.GetFunctionPointerForDelegate(managed.onShaderLoadingError),
                meshSpoolCount = managed.meshSpoolCount,
                effectSpoolCount = managed.effectSpoolCount
            };
        }

        internal Unmanaged ToUnmanaged()
        {
            return unmanaged;
        }

        internal void Free()
        {
            Marshalling.Free<ShaderFileDescription, ShaderFileDescriptionMarshaller.Unmanaged,
                ShaderFileDescriptionMarshaller.Marshaller>(unmanaged.shaderFiles, unmanaged.shaderFileCount);

            Marshalling.Free<String, IntPtr,
                UnicodeStringMarshaller>(unmanaged.symbols, symbolCount);

            Marshalling.Free<MaterialDescription, MaterialDescriptionMarshaller.Unmanaged,
                MaterialDescriptionMarshaller.Marshaller>(unmanaged.materials, unmanaged.materialCount);

            Marshalling.Free<Texture, IntPtr,
                NativeObjectMarshaller.Marshaller>(unmanaged.textures, textureCount);
        }
    }
}

/// <summary>
///     Describes a shader file that is loaded into the raytracing pipeline.
/// </summary>
[NativeMarshalling(typeof(ShaderFileDescriptionMarshaller))]
internal struct ShaderFileDescription
{
    internal String path;
    internal UInt32 symbolCount;
}

[CustomMarshaller(typeof(ShaderFileDescription), MarshalMode.ManagedToUnmanagedIn, typeof(ShaderFileDescriptionMarshaller))]
internal static class ShaderFileDescriptionMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(ShaderFileDescription managed)
    {
        return new Unmanaged
        {
            path = UnicodeStringMarshaller.ConvertToUnmanaged(managed.path),
            symbolCount = managed.symbolCount
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        UnicodeStringMarshaller.Free(unmanaged.path);
    }
#pragma warning disable S1694
    internal abstract class Marshaller : IMarshaller<ShaderFileDescription, Unmanaged>
#pragma warning restore S1694
    {
        static Unmanaged IMarshaller<ShaderFileDescription, Unmanaged>.ConvertToUnmanaged(ShaderFileDescription managed)
        {
            return ConvertToUnmanaged(managed);
        }

        static void IMarshaller<ShaderFileDescription, Unmanaged>.Free(Unmanaged unmanaged)
        {
            Free(unmanaged);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal IntPtr path;
        internal UInt32 symbolCount;
    }
}

/// <summary>
///     Describes a material that is loaded into the raytracing pipeline.
/// </summary>
internal struct MaterialDescription
{
    internal String name;
    internal Boolean isVisible;
    internal Boolean isShadowCaster;
    internal Boolean isOpaque;

    internal Boolean isAnimated;
    internal UInt32 animationShaderIndex;

    internal String normalClosestHitSymbol;
    internal String normalAnyHitSymbol;
    internal String normalIntersectionSymbol;

    internal String shadowClosestHitSymbol;
    internal String shadowAnyHitSymbol;
    internal String shadowIntersectionSymbol;
}

[CustomMarshaller(typeof(MaterialDescription), MarshalMode.ManagedToUnmanagedIn, typeof(MaterialDescriptionMarshaller))]
internal static class MaterialDescriptionMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(MaterialDescription managed)
    {
        return new Unmanaged
        {
            name = UnicodeStringMarshaller.ConvertToUnmanaged(managed.name),
            isVisible = managed.isVisible.ToInt(),
            isShadowCaster = managed.isShadowCaster.ToInt(),
            isOpaque = managed.isOpaque.ToInt(),
            isAnimated = managed.isAnimated.ToInt(),
            animationShaderIndex = managed.animationShaderIndex,
            normalClosestHitSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.normalClosestHitSymbol),
            normalAnyHitSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.normalAnyHitSymbol),
            normalIntersectionSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.normalIntersectionSymbol),
            shadowClosestHitSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.shadowClosestHitSymbol),
            shadowAnyHitSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.shadowAnyHitSymbol),
            shadowIntersectionSymbol = UnicodeStringMarshaller.ConvertToUnmanaged(managed.shadowIntersectionSymbol)
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        UnicodeStringMarshaller.Free(unmanaged.name);
        UnicodeStringMarshaller.Free(unmanaged.normalClosestHitSymbol);
        UnicodeStringMarshaller.Free(unmanaged.normalAnyHitSymbol);
        UnicodeStringMarshaller.Free(unmanaged.normalIntersectionSymbol);
        UnicodeStringMarshaller.Free(unmanaged.shadowClosestHitSymbol);
        UnicodeStringMarshaller.Free(unmanaged.shadowAnyHitSymbol);
        UnicodeStringMarshaller.Free(unmanaged.shadowIntersectionSymbol);
    }
#pragma warning disable S1694
    internal abstract class Marshaller : IMarshaller<MaterialDescription, Unmanaged>
#pragma warning restore S1694
    {
        static Unmanaged IMarshaller<MaterialDescription, Unmanaged>.ConvertToUnmanaged(MaterialDescription managed)
        {
            return ConvertToUnmanaged(managed);
        }

        static void IMarshaller<MaterialDescription, Unmanaged>.Free(Unmanaged unmanaged)
        {
            Free(unmanaged);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal IntPtr name;
        internal Int32 isVisible;
        internal Int32 isShadowCaster;
        internal Int32 isOpaque;
        internal Int32 isAnimated;
        internal UInt32 animationShaderIndex;
        internal IntPtr normalClosestHitSymbol;
        internal IntPtr normalAnyHitSymbol;
        internal IntPtr normalIntersectionSymbol;
        internal IntPtr shadowClosestHitSymbol;
        internal IntPtr shadowAnyHitSymbol;
        internal IntPtr shadowIntersectionSymbol;
    }
}

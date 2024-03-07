// <copyright file="SpacePipeline.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using VoxelGame.Core.Utilities;
using VoxelGame.Support.Interop;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Definition;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Additional information describing the raytracing pipeline.
/// </summary>
[NativeMarshalling(typeof(SpacePipelineDescriptionMarshaller))]
internal struct SpacePipelineDescription
{
    internal ShaderFileDescription[] shaderFiles;
    internal string[] symbols;

    internal MaterialDescription[] materials;

    internal Texture[] textures;
    internal uint textureCountFirstSlot;
    internal uint textureCountSecondSlot;

    internal uint customDataBufferSize;

    internal Native.NativeErrorFunc onShaderLoadingError;
}

[CustomMarshaller(typeof(SpacePipelineDescription), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
internal unsafe ref struct SpacePipelineDescriptionMarshaller
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal ShaderFileDescriptionMarshaller.Unmanaged* shaderFiles;
        internal uint shaderFileCount;

        internal IntPtr* symbols;

        internal MaterialDescriptionMarshaller.Unmanaged* materials;
        internal uint materialCount;

        internal IntPtr* textures;
        internal uint textureCountFirstSlot;
        internal uint textureCountSecondSlot;

        internal uint customDataBufferSize;
        internal IntPtr onShaderLoadingError;
    }

    internal ref struct ManagedToUnmanagedIn
    {
        private Unmanaged unmanaged;

        private uint symbolCount;
        private uint textureCount;

        internal void FromManaged(SpacePipelineDescription managed)
        {
            unmanaged = new Unmanaged
            {
                shaderFiles = Marshalling.ConvertToUnmanaged<ShaderFileDescription, ShaderFileDescriptionMarshaller.Unmanaged,
                    ShaderFileDescriptionMarshaller.Marshaller>(managed.shaderFiles, out uint shaderFileCount),
                shaderFileCount = shaderFileCount,
                symbols = Marshalling.ConvertToUnmanaged<string, IntPtr,
                    UnicodeStringMarshaller>(managed.symbols, out symbolCount),
                materials = Marshalling.ConvertToUnmanaged<MaterialDescription, MaterialDescriptionMarshaller.Unmanaged,
                    MaterialDescriptionMarshaller.Marshaller>(managed.materials, out uint materialCount),
                materialCount = materialCount,
                textures = Marshalling.ConvertToUnmanaged<Texture, IntPtr,
                    NativeObjectMarshaller.Marshaller>(managed.textures, out textureCount),
                textureCountFirstSlot = managed.textureCountFirstSlot,
                textureCountSecondSlot = managed.textureCountSecondSlot,
                customDataBufferSize = managed.customDataBufferSize,
                onShaderLoadingError = Marshal.GetFunctionPointerForDelegate(managed.onShaderLoadingError)
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

            Marshalling.Free<string, IntPtr,
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
    internal string path;
    internal uint symbolCount;
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
        internal uint symbolCount;
    }
}

/// <summary>
///     Describes a material that is loaded into the raytracing pipeline.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MaterialDescription
{
    internal string name;
    internal bool isVisible;
    internal bool isShadowCaster;
    internal bool isOpaque;

    internal bool isAnimated;
    internal uint animationShaderIndex;

    internal string normalClosestHitSymbol;
    internal string normalAnyHitSymbol;
    internal string normalIntersectionSymbol;

    internal string shadowClosestHitSymbol;
    internal string shadowAnyHitSymbol;
    internal string shadowIntersectionSymbol;
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
        internal int isVisible;
        internal int isShadowCaster;
        internal int isOpaque;
        internal int isAnimated;
        internal uint animationShaderIndex;
        internal IntPtr normalClosestHitSymbol;
        internal IntPtr normalAnyHitSymbol;
        internal IntPtr normalIntersectionSymbol;
        internal IntPtr shadowClosestHitSymbol;
        internal IntPtr shadowAnyHitSymbol;
        internal IntPtr shadowIntersectionSymbol;
    }
}

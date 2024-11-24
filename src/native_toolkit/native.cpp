//  <copyright file="native.cpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#include "stdafx.h"

NATIVE Allocator* NativeCreateAllocator() { return new Allocator(); }

NATIVE std::byte* NativeAllocate(Allocator const* allocator, UINT64 const size) { return allocator->Allocate(size); }

NATIVE HRESULT NativeDeallocate(Allocator const* allocator, std::byte* const pointer)
{
    return allocator->Deallocate(pointer);
}

NATIVE void NativeDeleteAllocator(Allocator const* allocator) { delete allocator; }

NATIVE Noise* NativeCreateNoise(NoiseDefinition const definition) { return new Noise(definition); }

NATIVE FLOAT NativeGetNoise2D(Noise const* noise, FLOAT const x, FLOAT const y) { return noise->GetNoise(x, y); }

NATIVE FLOAT NativeGetNoise3D(Noise const* noise, FLOAT const x, FLOAT const y, FLOAT const z)
{
    return noise->GetNoise(x, y, z);
}

NATIVE void NativeGetNoiseGrid2D(
    Noise const* noise,
    INT const    x,
    INT const    y,
    INT const    width,
    INT const    height,
    PFLOAT const out) { noise->GetGrid(x, y, width, height, out); }

NATIVE void NativeGetNoiseGrid3D(
    Noise const* noise,
    INT const    x,
    INT const    y,
    INT const    z,
    INT const    width,
    INT const    height,
    INT const    depth,
    PFLOAT const out) { noise->GetGrid(x, y, z, width, height, depth, out); }

NATIVE void NativeDeleteNoise(Noise const* noise) { delete noise; }

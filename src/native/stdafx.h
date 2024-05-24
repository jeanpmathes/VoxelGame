//  <copyright file="stdafx.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers.
#endif

#include <Windows.h>
#include <windowsx.h>

#undef min
#undef max

#if defined(NATIVE_DEBUG)
#define USE_PIX
#endif

// DirectX

#include "d3d12.h"
#include "d3dx12.h"
#include "DirectXMath.h"
#include "pix3.h"

#include <comdef.h>
#include <dxgi1_6.h>

// STD

#include <algorithm>
#include <array>
#include <cstdint>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <functional>
#include <iomanip>
#include <iostream>
#include <list>
#include <map>
#include <memory>
#include <mutex>
#include <set>
#include <sstream>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

// WRL

#include <shellapi.h>
#include <wrl.h>

// nv_aftermath

#if defined(USE_NSIGHT_AFTERMATH)
#include "nv_aftermath/NsightAftermathGpuCrashTracker.hpp"
#include "nv_aftermath/NsightAftermathHelpers.hpp"
#include "nv_aftermath/NsightAftermathShaderDatabase.hpp"
#endif

// nv_helpers_dx12

#include "nv_helpers_dx12/BottomLevelASGenerator.hpp"
#include "nv_helpers_dx12/RaytracingPipelineGenerator.hpp"
#include "nv_helpers_dx12/RootSignatureGenerator.hpp"
#include "nv_helpers_dx12/ShaderBindingTableGenerator.hpp"
#include "nv_helpers_dx12/TopLevelASGenerator.hpp"

// Helpers

#include "DXHelper.hpp"
#include "DXRHelper.hpp"

// D3D12 Memory Allocator

#include "Tools/Allocation.hpp"
#include "Tools/D3D12MemAlloc.hpp"

// Tools

#include "Tools/AnimationController.hpp"
#include "Tools/Bag.hpp"
#include "Tools/Common.hpp"
#include "Tools/Concepts.hpp"
#include "Tools/DescriptorHeap.hpp"
#include "Tools/DrawablesGroup.hpp"
#include "Tools/InBufferAllocator.hpp"
#include "Tools/IntegerSet.hpp"
#include "Tools/ShaderResources.hpp"
#include "Tools/SharedIndexBuffer.hpp"
#include "Tools/StepTimer.hpp"
#include "Tools/Uploader.hpp"
#include "Tools/Utilities.hpp"

// General

#include "DXApp.hpp"
#include "native.hpp"
#include "NativeClient.hpp"
#include "Space.hpp"
#include "Win32Application.hpp"

// Objects

#include "Objects/Camera.hpp"
#include "Objects/Drawable.hpp"
#include "Objects/Effect.hpp"
#include "Objects/Light.hpp"
#include "Objects/Mesh.hpp"
#include "Objects/Object.hpp"
#include "Objects/RasterPipeline.hpp"
#include "Objects/ShaderBuffer.hpp"
#include "Objects/Spatial.hpp"
#include "Objects/Texture.hpp"

// Interfaces

#include "Interfaces/Draw2D.hpp"

#if defined(NATIVE_DEBUG)
#include <dxgidebug.h>
#include <Initguid.h>
#endif

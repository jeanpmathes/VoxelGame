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

// DirectX

#include "d3d12.h"
#include "d3dx12.h"
#include "DirectXMath.h"
#include "pix3.h"

#include <dxgi1_6.h>
#include <comdef.h>

// STD

#include <string>
#include <mutex>
#include <algorithm>
#include <utility>
#include <cstdlib>
#include <cstdint>
#include <stdexcept>
#include <iomanip>
#include <sstream>
#include <map>
#include <vector>
#include <set>
#include <list>
#include <array>
#include <memory>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <functional>

// WRL

#include <wrl.h>
#include <shellapi.h>

// nv_aftermath

#if defined(USE_NSIGHT_AFTERMATH)
#include "nv_aftermath/NsightAftermathHelpers.hpp"
#include "nv_aftermath/NsightAftermathGpuCrashTracker.hpp"
#include "nv_aftermath/NsightAftermathShaderDatabase.hpp"
#endif

// nv_helpers_dx12

#include "nv_helpers_dx12/TopLevelASGenerator.hpp"
#include "nv_helpers_dx12/BottomLevelASGenerator.hpp"
#include "nv_helpers_dx12/RaytracingPipelineGenerator.hpp"
#include "nv_helpers_dx12/ShaderBindingTableGenerator.hpp"
#include "nv_helpers_dx12/RootSignatureGenerator.hpp"

// Helpers

#include "DXRHelper.hpp"
#include "DXHelper.hpp"

// D3D12 Memory Allocator

#include "Tools/D3D12MemAlloc.hpp"
#include "Tools/Allocation.hpp"

// Tools

#include "Tools/DescriptorHeap.hpp"
#include "Tools/Bag.hpp"
#include "Tools/SharedIndexBuffer.hpp"
#include "Tools/InBufferAllocator.hpp"
#include "Tools/ShaderResources.hpp"
#include "Tools/AnimationController.hpp"
#include "Tools/StepTimer.hpp"
#include "Tools/Utilities.hpp"
#include "Tools/Uploader.hpp"
#include "Tools/Common.hpp"
#include "Tools/IntegerSet.hpp"
#include "Tools/DrawablesGroup.hpp"
#include "Tools/Concepts.hpp"

// General

#include "DXApp.hpp"
#include "native.hpp"
#include "Win32Application.hpp"
#include "NativeClient.hpp"
#include "Space.hpp"

// Objects

#include "Objects/Object.hpp"
#include "Objects/Spatial.hpp"
#include "Objects/Camera.hpp"
#include "Objects/Light.hpp"
#include "Objects/Mesh.hpp"
#include "Objects/RasterPipeline.hpp"
#include "Objects/ShaderBuffer.hpp"
#include "Objects/Texture.hpp"
#include "Objects/Drawable.hpp"

// Interfaces

#include "Interfaces/Draw2D.hpp"

#if defined(VG_DEBUG)
#include <Initguid.h>
#include <dxgidebug.h>
#endif

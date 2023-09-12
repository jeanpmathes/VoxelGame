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

// DirectX

#include <d3d12.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <DirectXMath.h>
#include "d3dx12.h"
#include "comdef.h"
#include <pix3.h>

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
#include "Tools/GappedList.hpp"
#include "Tools/SharedIndexBuffer.hpp"

// General

#include "DXApp.hpp"
#include "native.hpp"
#include "StepTimer.hpp"
#include "Utilities.hpp"
#include "Win32Application.hpp"
#include "NativeClient.hpp"
#include "Space.hpp"
#include "Common.hpp"
#include "Uploader.hpp"

// Objects

#include "Objects/Object.hpp"
#include "Objects/SpatialObject.hpp"
#include "Objects/Camera.hpp"
#include "Objects/Light.hpp"
#include "Objects/MeshObject.hpp"
#include "Objects/RasterPipeline.hpp"
#include "Objects/ShaderBuffer.hpp"
#include "Objects/Texture.hpp"

// Interfaces

#include "Interfaces/Draw2D.hpp"

#if defined(VG_DEBUG)
#include <Initguid.h>
#include <dxgidebug.h>
#endif

//  <copyright file="stdafx.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft</author>

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

#include "nv_helpers_dx12/TopLevelASGenerator.h"
#include "nv_helpers_dx12/BottomLevelASGenerator.h"
#include "nv_helpers_dx12/RaytracingPipelineGenerator.h"
#include "nv_helpers_dx12/ShaderBindingTableGenerator.h"
#include "nv_helpers_dx12/RootSignatureGenerator.h"

// Helpers

#include "DXRHelper.h"
#include "DXHelper.h"

// D3D12 Memory Allocator

#include "Tools/D3D12MemAlloc.h"
#include "Tools/Allocation.h"

// Custom

#include "DXApp.h"
#include "native.h"
#include "StepTimer.h"
#include "Utilities.h"
#include "Win32Application.h"
#include "NativeClient.h"
#include "Space.h"
#include "Common.h"
#include "Uploader.h"

#include "Objects/Object.h"
#include "Objects/SpatialObject.h"
#include "Objects/Camera.h"
#include "Objects/Light.h"
#include "Objects/MeshObject.h"
#include "Objects/RasterPipeline.h"
#include "Objects/ShaderBuffer.h"
#include "Objects/Texture.h"

#include "Interfaces/Draw2D.h"

#ifdef _DEBUG
#include <Initguid.h>
#include <dxgidebug.h>
#endif

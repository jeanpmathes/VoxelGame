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
#include <pix3.h>

// STD

#include <string>

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

// Custom

#include "DXApp.h"
#include "native.h"
#include "StepTimer.h"
#include "Utilities.h"
#include "Win32Application.h"
#include "NativeClient.h"
#include "Space.h"
#include "Common.h"

#include "Objects/Object.h"
#include "Objects/SpatialObject.h"
#include "Objects/Camera.h"
#include "Objects/Light.h"
#include "Objects/MeshObject.h"
#include "Objects/IndexedMeshObject.h"
#include "Objects/SequencedMeshObject.h"
#include "Objects/RasterPipeline.h"
#include "Objects/ShaderBuffer.h"

#ifdef _DEBUG
#include <Initguid.h>
#include <dxgidebug.h>
#endif

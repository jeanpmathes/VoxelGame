//  <copyright file="native.h" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#pragma once

#define NATIVE extern "C" __declspec(dllexport)

using NativeCallbackFunc = void(*)();
using NativeStepFunc = void(*)(double);
using NativeInputFunc = void(*)(UINT8);
using NativeErrorFunc = void(*)(HRESULT, const char*);
using NativeErrorMessageFunc = void(*)(const char*);

struct Configuration
{
    NativeStepFunc onRender;
    NativeStepFunc onUpdate;

    NativeCallbackFunc onInit;
    NativeCallbackFunc onDestroy;

    NativeInputFunc onKeyDown;
    NativeInputFunc onKeyUp;

    D3D12MessageFunc onDebug;

    BOOL allowTearing;
};

struct CameraData
{
    DirectX::XMFLOAT3 position;
};

struct SpatialObjectData
{
    DirectX::XMFLOAT3 position;
    DirectX::XMFLOAT4 rotation;
};

#define TRY try
#define CATCH() \
    catch (const HResultException& e) { onError(e.Error(), e.Info()); exit(1); } \
    catch (const NativeException& e) { onErrorMessage(e.what()); exit(1); } \
    catch (const std::exception& e) { onErrorMessage(e.what()); exit(1); } \
    catch (...) { onErrorMessage("Unknown error."); exit(1); }

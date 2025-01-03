﻿// <copyright file="MessageFunc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation
//     Licensed under the MIT license
//     Definitions from d3d12sdklayers.h
// </copyright>
// <author>Microsoft Corporation</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
internal static partial class Native
{
    internal delegate void D3D12MessageFunc(D3D12_MESSAGE_CATEGORY Category, D3D12_MESSAGE_SEVERITY Severity, D3D12_MESSAGE_ID ID, [MarshalAs(UnmanagedType.LPStr)] String? pDescription, IntPtr pContext);

    internal enum D3D12_MESSAGE_CATEGORY
    {
        D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED = 0,
        D3D12_MESSAGE_CATEGORY_MISCELLANEOUS = D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED + 1,
        D3D12_MESSAGE_CATEGORY_INITIALIZATION = D3D12_MESSAGE_CATEGORY_MISCELLANEOUS + 1,
        D3D12_MESSAGE_CATEGORY_CLEANUP = D3D12_MESSAGE_CATEGORY_INITIALIZATION + 1,
        D3D12_MESSAGE_CATEGORY_COMPILATION = D3D12_MESSAGE_CATEGORY_CLEANUP + 1,
        D3D12_MESSAGE_CATEGORY_STATE_CREATION = D3D12_MESSAGE_CATEGORY_COMPILATION + 1,
        D3D12_MESSAGE_CATEGORY_STATE_SETTING = D3D12_MESSAGE_CATEGORY_STATE_CREATION + 1,
        D3D12_MESSAGE_CATEGORY_STATE_GETTING = D3D12_MESSAGE_CATEGORY_STATE_SETTING + 1,
        D3D12_MESSAGE_CATEGORY_RESOURCE_MANIPULATION = D3D12_MESSAGE_CATEGORY_STATE_GETTING + 1,
        D3D12_MESSAGE_CATEGORY_EXECUTION = D3D12_MESSAGE_CATEGORY_RESOURCE_MANIPULATION + 1,
        D3D12_MESSAGE_CATEGORY_SHADER = D3D12_MESSAGE_CATEGORY_EXECUTION + 1
    }

    internal enum D3D12_MESSAGE_ID
    {
        D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_AT_FAULT = 232,
        D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_POSSIBLY_AT_FAULT = 233,
        D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_NOT_AT_FAULT = 234

        // Unused IDs omitted ...
    }

    internal enum D3D12_MESSAGE_SEVERITY
    {
        D3D12_MESSAGE_SEVERITY_CORRUPTION = 0,
        D3D12_MESSAGE_SEVERITY_ERROR = D3D12_MESSAGE_SEVERITY_CORRUPTION + 1,
        D3D12_MESSAGE_SEVERITY_WARNING = D3D12_MESSAGE_SEVERITY_ERROR + 1,
        D3D12_MESSAGE_SEVERITY_INFO = D3D12_MESSAGE_SEVERITY_WARNING + 1,
        D3D12_MESSAGE_SEVERITY_MESSAGE = D3D12_MESSAGE_SEVERITY_INFO + 1
    }
}

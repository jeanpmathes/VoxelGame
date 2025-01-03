﻿//  <copyright file="Utilities.hpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#pragma once

#include "NativeClient.hpp"

namespace util
{
    /**
     * \brief Allocate a resource with the given parameters on the default pool of the client's allocator.
     * \tparam T The type of the resource to allocate.
     * \return The allocation and resource.
     */
    template <typename T>
    Allocation<T> AllocateResource(
        NativeClient const&         client,
        D3D12_RESOURCE_DESC const&  resourceDesc,
        D3D12_HEAP_TYPE const       heapType,
        D3D12_RESOURCE_STATES const initState,
        D3D12_CLEAR_VALUE const*    optimizedClearValue = nullptr,
        bool const                  committed           = false)
    {
        D3D12MA::ALLOCATION_DESC allocationDesc = {};
        allocationDesc.HeapType                 = heapType;

        if (committed) allocationDesc.Flags |= D3D12MA::ALLOCATION_FLAG_COMMITTED;

        ComPtr<T>                   resource;
        ComPtr<D3D12MA::Allocation> allocation;

        TryDo(
            client.GetAllocator()->CreateResource(
                &allocationDesc,
                &resourceDesc,
                initState,
                optimizedClearValue,
                &allocation,
                IID_PPV_ARGS(&resource)));

        return {allocation, resource};
    }

    /**
     * Allocate a buffer with the given parameters on the default pool of the client's allocator.
     */
    inline Allocation<ID3D12Resource> AllocateBuffer(
        NativeClient const&         client,
        UINT64 const                size,
        D3D12_RESOURCE_FLAGS const  flags,
        D3D12_RESOURCE_STATES const initState,
        D3D12_HEAP_TYPE const       heapType,
        bool const                  committed = false)
    {
        D3D12_RESOURCE_DESC bufferDescription;
        bufferDescription.Alignment          = 0;
        bufferDescription.DepthOrArraySize   = 1;
        bufferDescription.Dimension          = D3D12_RESOURCE_DIMENSION_BUFFER;
        bufferDescription.Flags              = flags;
        bufferDescription.Format             = DXGI_FORMAT_UNKNOWN;
        bufferDescription.Height             = 1;
        bufferDescription.Layout             = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        bufferDescription.MipLevels          = 1;
        bufferDescription.SampleDesc.Count   = 1;
        bufferDescription.SampleDesc.Quality = 0;
        bufferDescription.Width              = size;

        return AllocateResource<ID3D12Resource>(client, bufferDescription, heapType, initState, nullptr, committed);
    }

    /**
     * \brief Allocate a buffer, except when the given allocation is large enough.
     * \param allocation The allocation to check, may be null. Must have been allocated with the same parameters.
     * \param client The client to allocate on.
     * \param size The size of the buffer to allocate.
     * \param flags The flags of the buffer to allocate.
     * \param initState The initial state of the buffer to allocate.
     * \param heapType The heap type of the buffer to allocate.
     * \param committed Whether the buffer to allocate should be committed or placed.
     */
    inline void ReAllocateBuffer(
        Allocation<ID3D12Resource>* allocation,
        NativeClient const&         client,
        UINT64 const                size,
        D3D12_RESOURCE_FLAGS const  flags,
        D3D12_RESOURCE_STATES const initState,
        D3D12_HEAP_TYPE const       heapType,
        bool const                  committed = false)
    {
        if (allocation->IsSet() && allocation->resource->GetDesc().Width >= size) return;
        *allocation = AllocateBuffer(client, size, flags, initState, heapType, committed);
    }

    /**
     * Allocate a constant buffer with the given size on the default pool of the client's allocator.
     */
    inline Allocation<ID3D12Resource> AllocateConstantBuffer(NativeClient const& client, UINT64* size)
    {
        UINT64 const originalSize = *size;
        *size                     = RoundUp(originalSize, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        return AllocateBuffer(
            client,
            *size,
            D3D12_RESOURCE_FLAG_NONE,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            D3D12_HEAP_TYPE_UPLOAD);
    }

    /**
     * Map a resource and write the given data to it.
     * After writing, the resource is unmapped.
     */
    template <typename D>
    [[nodiscard]] HRESULT MapAndWrite(Allocation<ID3D12Resource> const resource, D const& data)
    {
        constexpr D3D12_RANGE readRange = {0, 0};
        D*                    mapping;

        HRESULT const result = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&mapping));
        if (FAILED(result)) return result;

        *mapping = data;

        resource.resource->Unmap(0, nullptr);
        return result;
    }

    /**
     * Map a resource and write the given data to it.
     * The data is assumed to be an array of D.
     * After writing, the resource is unmapped.
     */
    template <typename D>
    [[nodiscard]] HRESULT MapAndWrite(Allocation<ID3D12Resource> const resource, D const* data, UINT const count)
    {
        Require(count > 0);

        constexpr D3D12_RANGE readRange = {0, 0};
        D*                    mapping;

        HRESULT const result = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&mapping));
        if (FAILED(result)) return result;

        memcpy(mapping, data, sizeof(D) * count);

        resource.resource->Unmap(0, nullptr);
        return result;
    }

    /**
     * \brief Map a resource and read the data from it.
     * \tparam D The type of the data to read.
     * \param resource The resource to map.
     * \param data The data pointer to write to.
     * \param count The number of elements of D to read.
     * \return The result of the mapping operation.
     */
    template <typename D>
    [[nodiscard]] HRESULT MapAndRead(Allocation<ID3D12Resource> const resource, D* data, UINT const count)
    {
        Require(count > 0);

        D3D12_RANGE const readRange = {0, sizeof(D) * count};
        D*                mapping;

        HRESULT const result = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&mapping));
        if (FAILED(result)) return result;

        memcpy(data, mapping, sizeof(D) * count);

        constexpr D3D12_RANGE writeRange = {0, 0};
        resource.resource->Unmap(0, &writeRange);
        return result;
    }

    inline std::wstring FormatDRED(
        D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1 const& breadcrumbs,
        D3D12_DRED_PAGE_FAULT_OUTPUT2 const&       pageFaults,
        D3D12_DRED_DEVICE_STATE const&             deviceState)
    {
        std::wstringstream message;
        message << L"DRED !";

        message << L" Device State: ";

        switch (deviceState)
        {
        case D3D12_DRED_DEVICE_STATE_UNKNOWN:
            message << L"Unknown";
            break;
        case D3D12_DRED_DEVICE_STATE_HUNG:
            message << L"Hung";
            break;
        case D3D12_DRED_DEVICE_STATE_FAULT:
            message << L"Fault";
            break;
        case D3D12_DRED_DEVICE_STATE_PAGEFAULT:
            message << L"PageFault";
            break;
        default:
            message << L"Invalid";
            break;
        }

        message << std::endl;

        message << L"1. Auto Breadcrumbs:" << std::endl;

        auto str = [](wchar_t const* s) { return s ? s : L"<unknown>"; };

        {
            auto getOperationText = [&](D3D12_AUTO_BREADCRUMB_OP const op) -> std::wstring
            {
                switch (op)
                {
                case D3D12_AUTO_BREADCRUMB_OP_SETMARKER:
                    return L"SetMarker";
                case D3D12_AUTO_BREADCRUMB_OP_BEGINEVENT:
                    return L"BeginEvent";
                case D3D12_AUTO_BREADCRUMB_OP_ENDEVENT:
                    return L"EndEvent";
                case D3D12_AUTO_BREADCRUMB_OP_DRAWINSTANCED:
                    return L"DrawInstanced";
                case D3D12_AUTO_BREADCRUMB_OP_DRAWINDEXEDINSTANCED:
                    return L"DrawIndexedInstanced";
                case D3D12_AUTO_BREADCRUMB_OP_EXECUTEINDIRECT:
                    return L"ExecuteIndirect";
                case D3D12_AUTO_BREADCRUMB_OP_DISPATCH:
                    return L"Dispatch";
                case D3D12_AUTO_BREADCRUMB_OP_COPYBUFFERREGION:
                    return L"CopyBufferRegion";
                case D3D12_AUTO_BREADCRUMB_OP_COPYTEXTUREREGION:
                    return L"CopyTextureRegion";
                case D3D12_AUTO_BREADCRUMB_OP_COPYRESOURCE:
                    return L"CopyResource";
                case D3D12_AUTO_BREADCRUMB_OP_COPYTILES:
                    return L"CopyTiles";
                case D3D12_AUTO_BREADCRUMB_OP_RESOLVESUBRESOURCE:
                    return L"ResolveSubresource";
                case D3D12_AUTO_BREADCRUMB_OP_CLEARRENDERTARGETVIEW:
                    return L"ClearRenderTargetView";
                case D3D12_AUTO_BREADCRUMB_OP_CLEARUNORDEREDACCESSVIEW:
                    return L"ClearUnorderedAccessView";
                case D3D12_AUTO_BREADCRUMB_OP_CLEARDEPTHSTENCILVIEW:
                    return L"ClearDepthStencilView";
                case D3D12_AUTO_BREADCRUMB_OP_RESOURCEBARRIER:
                    return L"ResourceBarrier";
                case D3D12_AUTO_BREADCRUMB_OP_EXECUTEBUNDLE:
                    return L"ExecuteBundle";
                case D3D12_AUTO_BREADCRUMB_OP_PRESENT:
                    return L"Present";
                case D3D12_AUTO_BREADCRUMB_OP_RESOLVEQUERYDATA:
                    return L"ResolveQueryData";
                case D3D12_AUTO_BREADCRUMB_OP_BEGINSUBMISSION:
                    return L"BeginSubmission";
                case D3D12_AUTO_BREADCRUMB_OP_ENDSUBMISSION:
                    return L"EndSubmission";
                case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME:
                    return L"DecodeFrame";
                case D3D12_AUTO_BREADCRUMB_OP_PROCESSFRAMES:
                    return L"ProcessFrames";
                case D3D12_AUTO_BREADCRUMB_OP_ATOMICCOPYBUFFERUINT:
                    return L"AtomicCopyBufferUINT";
                case D3D12_AUTO_BREADCRUMB_OP_ATOMICCOPYBUFFERUINT64:
                    return L"AtomicCopyBufferUINT64";
                case D3D12_AUTO_BREADCRUMB_OP_RESOLVESUBRESOURCEREGION:
                    return L"ResolveSubresourceRegion";
                case D3D12_AUTO_BREADCRUMB_OP_WRITEBUFFERIMMEDIATE:
                    return L"WriteBufferImmediate";
                case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME1:
                    return L"DecodeFrame1";
                case D3D12_AUTO_BREADCRUMB_OP_SETPROTECTEDRESOURCESESSION:
                    return L"SetProtectedResourceSession";
                case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME2:
                    return L"DecodeFrame2";
                case D3D12_AUTO_BREADCRUMB_OP_PROCESSFRAMES1:
                    return L"ProcessFrames1";
                case D3D12_AUTO_BREADCRUMB_OP_BUILDRAYTRACINGACCELERATIONSTRUCTURE:
                    return L"BuildRaytracingAccelerationStructure";
                case D3D12_AUTO_BREADCRUMB_OP_EMITRAYTRACINGACCELERATIONSTRUCTUREPOSTBUILDINFO:
                    return L"EmitRaytracingAccelerationStructurePostBuildInfo";
                case D3D12_AUTO_BREADCRUMB_OP_COPYRAYTRACINGACCELERATIONSTRUCTURE:
                    return L"CopyRaytracingAccelerationStructure";
                case D3D12_AUTO_BREADCRUMB_OP_DISPATCHRAYS:
                    return L"DispatchRays";
                case D3D12_AUTO_BREADCRUMB_OP_INITIALIZEMETACOMMAND:
                    return L"InitializeMetaCommand";
                case D3D12_AUTO_BREADCRUMB_OP_EXECUTEMETACOMMAND:
                    return L"ExecuteMetaCommand";
                case D3D12_AUTO_BREADCRUMB_OP_ESTIMATEMOTION:
                    return L"EstimateMotion";
                case D3D12_AUTO_BREADCRUMB_OP_RESOLVEMOTIONVECTORHEAP:
                    return L"ResolveMotionVectorHeap";
                case D3D12_AUTO_BREADCRUMB_OP_SETPIPELINESTATE1:
                    return L"SetPipelineState1";
                case D3D12_AUTO_BREADCRUMB_OP_INITIALIZEEXTENSIONCOMMAND:
                    return L"InitializeExtensionCommand";
                case D3D12_AUTO_BREADCRUMB_OP_EXECUTEEXTENSIONCOMMAND:
                    return L"ExecuteExtensionCommand";
                case D3D12_AUTO_BREADCRUMB_OP_DISPATCHMESH:
                    return L"DispatchMesh";
                case D3D12_AUTO_BREADCRUMB_OP_ENCODEFRAME:
                    return L"EncodeFrame";
                case D3D12_AUTO_BREADCRUMB_OP_RESOLVEENCODEROUTPUTMETADATA:
                    return L"ResolveEncoderOutputMetadata";
                case D3D12_AUTO_BREADCRUMB_OP_BARRIER:
                    return L"Barrier";
                case D3D12_AUTO_BREADCRUMB_OP_BEGIN_COMMAND_LIST:
                    return L"BeginCommandList";
                case D3D12_AUTO_BREADCRUMB_OP_DISPATCHGRAPH:
                    return L"DispatchGraph";
                case D3D12_AUTO_BREADCRUMB_OP_SETPROGRAM:
                    return L"SetProgram";
                default:
                    break;
                }
                return L"<unknown>";
            };

            auto node = breadcrumbs.pHeadAutoBreadcrumbNode;
            while (node != nullptr)
            {
                UINT const lastOperation = node->pLastBreadcrumbValue != nullptr
                                               ? *node->pLastBreadcrumbValue
                                               : node->BreadcrumbCount;

                message << L"\t|";
                message << L" CommandList: " << str(node->pCommandListDebugNameW);
                message << L" CommandQueue: " << str(node->pCommandQueueDebugNameW);

                bool const complete = lastOperation == node->BreadcrumbCount;

                if (complete) message << L" COMPLETE";
                else
                {
                    message << L" Operations: ";
                    message << L"(";
                    message << std::to_wstring(lastOperation);
                    message << L"/";
                    message << std::to_wstring(node->BreadcrumbCount);
                    message << L")";
                }

                message << std::endl;

                if (!complete)
                {
                    std::map<UINT, std::vector<wchar_t const*>> contexts;

                    for (UINT context = 0; context < node->BreadcrumbContextsCount; context++)
                    {
                        auto const& [index, string] = node->pBreadcrumbContexts[context];
                        contexts[index].push_back(string);
                    }

                    for (UINT operation = 0; operation < node->BreadcrumbCount; operation++)
                    {
                        message << L"\t\t|";
                        message << L" " << getOperationText(node->pCommandHistory[operation]);

                        if (operation == lastOperation) message << L" (last)";

                        message << std::endl;

                        if (auto context = contexts.find(operation);
                            context != contexts.end())
                        {
                            std::vector<wchar_t const*> strings;
                            std::tie(std::ignore, strings) = *context;

                            for (auto const& string : strings)
                            {
                                message << L"\t\t\t|";
                                message << L" " << string;
                                message << std::endl;
                            }
                        }
                    }
                }

                node = node->pNext;
            }
        }

        message << L"2. Page Fault: " << L"[" << pageFaults.PageFaultVA << L"]" << std::endl;

        if (pageFaults.pHeadExistingAllocationNode == nullptr) message << L"\t| No existing allocation node" <<
            std::endl;

        if (pageFaults.pHeadRecentFreedAllocationNode == nullptr) message << L"\t| No recent freed allocation node" <<
            std::endl;

        {
            auto formatNodes = [&](wchar_t const* category, D3D12_DRED_ALLOCATION_NODE1 const* node)
            {
                auto current = node;
                while (current != nullptr)
                {
                    message << L"\t|";
                    message << L" " << category;
                    message << L" Name: " << str(current->ObjectNameW);

                    message << L" Type:";
                    switch (current->AllocationType)
                    {
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_QUEUE:
                        message << L" CommandQueue";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_ALLOCATOR:
                        message << L" CommandAllocator";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_STATE:
                        message << L" PipelineState";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_LIST:
                        message << L" CommandList";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_FENCE:
                        message << L" Fence";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_DESCRIPTOR_HEAP:
                        message << L" DescriptorHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_HEAP:
                        message << L" Heap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_QUERY_HEAP:
                        message << L" QueryHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_SIGNATURE:
                        message << L" CommandSignature";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_LIBRARY:
                        message << L" PipelineLibrary";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER:
                        message << L" VideoDecoder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_PROCESSOR:
                        message << L" VideoProcessor";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_RESOURCE:
                        message << L" Resource";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PASS:
                        message << L" Pass";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSION:
                        message << L" CryptoSession";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSIONPOLICY:
                        message << L" CryptoSessionPolicy";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PROTECTEDRESOURCESESSION:
                        message << L" ProtectedResourceSession";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER_HEAP:
                        message << L" VideoDecoderHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_POOL:
                        message << L" CommandPool";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_RECORDER:
                        message << L" CommandRecorder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_STATE_OBJECT:
                        message << L" StateObject";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_METACOMMAND:
                        message << L" MetaCommand";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_SCHEDULINGGROUP:
                        message << L" SchedulingGroup";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_ESTIMATOR:
                        message << L" VideoMotionEstimator";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_VECTOR_HEAP:
                        message << L" VideoMotionVectorHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_EXTENSION_COMMAND:
                        message << L" VideoExtensionCommand";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_ENCODER:
                        message << L" VideoEncoder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_ENCODER_HEAP:
                        message << L" VideoEncoderHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_INVALID:
                        message << L" Invalid";
                        break;
                    }

                    message << std::endl;

                    current = current->pNext;
                }
            };

            formatNodes(L"Existing", pageFaults.pHeadExistingAllocationNode);
            formatNodes(L"Freed", pageFaults.pHeadRecentFreedAllocationNode);
        }

        return message.str();
    }
}

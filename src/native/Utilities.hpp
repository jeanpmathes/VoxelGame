//  <copyright file="Utilities.hpp" company="VoxelGame">
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
        const NativeClient& client,
        const D3D12_RESOURCE_DESC& resourceDesc,
        const D3D12_HEAP_TYPE heapType,
        const D3D12_RESOURCE_STATES initState,
        const D3D12_CLEAR_VALUE* optimizedClearValue = nullptr,
        const bool committed = false)
    {
        D3D12MA::ALLOCATION_DESC allocationDesc = {};
        allocationDesc.HeapType = heapType;

        if (committed) allocationDesc.Flags |= D3D12MA::ALLOCATION_FLAG_COMMITTED;

        ComPtr<T> resource;
        ComPtr<D3D12MA::Allocation> allocation;

        TRY_DO(client.GetAllocator()->CreateResource(
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
        const NativeClient& client,
        const UINT64 size,
        const D3D12_RESOURCE_FLAGS flags,
        const D3D12_RESOURCE_STATES initState,
        const D3D12_HEAP_TYPE heapType,
        const bool committed = false)
    {
        D3D12_RESOURCE_DESC bufferDescription;
        bufferDescription.Alignment = 0;
        bufferDescription.DepthOrArraySize = 1;
        bufferDescription.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        bufferDescription.Flags = flags;
        bufferDescription.Format = DXGI_FORMAT_UNKNOWN;
        bufferDescription.Height = 1;
        bufferDescription.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        bufferDescription.MipLevels = 1;
        bufferDescription.SampleDesc.Count = 1;
        bufferDescription.SampleDesc.Quality = 0;
        bufferDescription.Width = size;

        return AllocateResource<ID3D12Resource>(client, bufferDescription, heapType, initState, nullptr, committed);
    }

    /**
     * Allocate a constant buffer with the given size on the default pool of the client's allocator.
     */
    inline Allocation<ID3D12Resource> AllocateConstantBuffer(const NativeClient& client, UINT64* size)
    {
        const UINT64 originalSize = *size;
        *size = ROUND_UP(originalSize, D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
        return AllocateBuffer(client, *size,
                              D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    }

    /**
     * Map a resource and write the given data to it.
     */
    template <typename D>
    [[nodiscard]] HRESULT MapAndWrite(const Allocation<ID3D12Resource> resource, const D& data)
    {
        constexpr D3D12_RANGE readRange = {0, 0}; // We do not intend to read from this resource on the CPU.
        D* dataPointer;

        const HRESULT result = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&dataPointer));
        if (FAILED(result)) return result;

        *dataPointer = data;

        resource.resource->Unmap(0, nullptr);
        return result;
    }

    /**
     * Map a resource and write the given data to it. The data is assumed to be an array of D.
     */
    template <typename D>
    [[nodiscard]] HRESULT MapAndWrite(const Allocation<ID3D12Resource> resource, const D* data, const UINT count)
    {
        REQUIRE(count > 0);

        constexpr D3D12_RANGE readRange = {0, 0}; // We do not intend to read from this resource on the CPU.
        D* dataPointer;

        const HRESULT result = resource.resource->Map(0, &readRange, reinterpret_cast<void**>(&dataPointer));
        if (FAILED(result)) return result;

        memcpy(dataPointer, data, sizeof(D) * count);

        resource.resource->Unmap(0, nullptr);
        return result;
    }
    
    inline std::wstring FormatDRED(
        const D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1& breadcrumbs,
        const D3D12_DRED_PAGE_FAULT_OUTPUT2& pageFaults,
        const D3D12_DRED_DEVICE_STATE& deviceState)
    {
        std::wstringstream message;
        message << L"DRED !";

        message << L" Device State: ";

        switch (deviceState)
        {
        case D3D12_DRED_DEVICE_STATE_UNKNOWN: message << L"Unknown";
            break;
        case D3D12_DRED_DEVICE_STATE_HUNG: message << L"Hung";
            break;
        case D3D12_DRED_DEVICE_STATE_FAULT: message << L"Fault";
            break;
        case D3D12_DRED_DEVICE_STATE_PAGEFAULT: message << L"PageFault";
            break;
        default: message << L"Invalid";
            break;
        }

        message << std::endl;

        message << L"1. Auto Breadcrumbs:" << std::endl;

        auto str = [](const wchar_t* s) -> const wchar_t* { return s ? s : L"<unknown>"; };

        {
            auto getOperationText = [&](D3D12_AUTO_BREADCRUMB_OP op) -> std::wstring
            {
                switch (op)
                {
                case D3D12_AUTO_BREADCRUMB_OP_SETMARKER: // NOLINT(bugprone-branch-clone)
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
                }
                return L"<unknown>";
            };

            auto node = breadcrumbs.pHeadAutoBreadcrumbNode;
            while (node != nullptr)
            {
                const UINT lastOperation = node->pLastBreadcrumbValue != nullptr
                                               ? *node->pLastBreadcrumbValue
                                               : node->BreadcrumbCount;

                message << L"\t|";
                message << L" CommandList: " << str(node->pCommandListDebugNameW);
                message << L" CommandQueue: " << str(node->pCommandQueueDebugNameW);

                const bool complete = lastOperation == node->BreadcrumbCount;

                if (complete)
                {
                    message << L" COMPLETE";
                }
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
                    std::map<UINT, std::vector<const wchar_t*>> contexts;

                    for (UINT context = 0; context < node->BreadcrumbContextsCount; context++)
                    {
                        const auto& [index, string] = node->pBreadcrumbContexts[context];
                        contexts[index].push_back(string);
                    }

                    for (UINT operation = 0; operation < node->BreadcrumbCount; operation++)
                    {
                        message << L"\t\t|";
                        message << L" " << getOperationText(node->pCommandHistory[operation]);

                        if (operation == lastOperation) message << L" (last)";

                        message << std::endl;

                        if (auto context = contexts.find(operation); context != contexts.end())
                        {
                            std::vector<const wchar_t*> strings;
                            std::tie(std::ignore, strings) = *context;

                            for (const auto& string : strings)
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

        if (pageFaults.pHeadExistingAllocationNode == nullptr)
            message << L"\t| No existing allocation node" << std::endl;

        if (pageFaults.pHeadRecentFreedAllocationNode == nullptr)
            message << L"\t| No recent freed allocation node" << std::endl;

        {
            auto formatNodes = [&](const wchar_t* category, const D3D12_DRED_ALLOCATION_NODE1* node)
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
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_QUEUE: message << L" CommandQueue";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_ALLOCATOR: message << L" CommandAllocator";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_STATE: message << L" PipelineState";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_LIST: message << L" CommandList";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_FENCE: message << L" Fence";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_DESCRIPTOR_HEAP: message << L" DescriptorHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_HEAP: message << L" Heap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_QUERY_HEAP: message << L" QueryHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_SIGNATURE: message << L" CommandSignature";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_LIBRARY: message << L" PipelineLibrary";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER: message << L" VideoDecoder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_PROCESSOR: message << L" VideoProcessor";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_RESOURCE: message << L" Resource";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PASS: message << L" Pass";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSION: message << L" CryptoSession";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSIONPOLICY: message << L" CryptoSessionPolicy";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_PROTECTEDRESOURCESESSION: message << L" ProtectedResourceSession";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER_HEAP: message << L" VideoDecoderHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_POOL: message << L" CommandPool";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_COMMAND_RECORDER: message << L" CommandRecorder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_STATE_OBJECT: message << L" StateObject";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_METACOMMAND: message << L" MetaCommand";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_SCHEDULINGGROUP: message << L" SchedulingGroup";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_ESTIMATOR: message << L" VideoMotionEstimator";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_VECTOR_HEAP: message << L" VideoMotionVectorHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_EXTENSION_COMMAND: message << L" VideoExtensionCommand";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_ENCODER: message << L" VideoEncoder";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_VIDEO_ENCODER_HEAP: message << L" VideoEncoderHeap";
                        break;
                    case D3D12_DRED_ALLOCATION_TYPE_INVALID: message << L" Invalid";
                        break;
                    default: message << L" <unknown>";
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

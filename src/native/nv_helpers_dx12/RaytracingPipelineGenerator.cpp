/*-----------------------------------------------------------------------
Copyright (c) 2014-2018, NVIDIA. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
* Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
* Neither the name of its contributors may be used to endorse
or promote products derived from this software without specific
prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ``AS IS'' AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-----------------------------------------------------------------------*/

/*
Contacts for feedback:
- pgautron@nvidia.com (Pascal Gautron)
- mlefrancois@nvidia.com (Martin-Karl Lefrancois)

The raytracing pipeline combines the raytracing shaders into a state object,
that can be thought of as an executable GPU program. For that, it requires the
shaders compiled as DXIL libraries, where each library exports symbols in a way
similar to DLLs. Those symbols are then used to refer to these shaders libraries
when creating hit groups, associating the shaders to their root signatures and
declaring the steps of the pipeline. All the calls to this helper class can be
done in arbitrary order. Some basic sanity checks are also performed when
compiling in debug mode.

*/

#include "RaytracingPipelineGenerator.hpp"

#include <d3d12shader.h>
#include <iostream>

#include "dxcapi.h"

#include <stdexcept>
#include <unordered_set>
#include <wrl/client.h>

#define STRING_OR_NULL(str) ((str).empty() ? nullptr : (str).c_str())

namespace nv_helpers_dx12
{
    RayTracingPipelineGenerator::RayTracingPipelineGenerator(Microsoft::WRL::ComPtr<ID3D12Device5> device)
        : m_device(std::move(device))
    {
        // The pipeline creation requires having at least one empty global and local root signatures, so
        // we systematically create both, as this does not incur any overhead
        CreateDummyRootSignature();
    }

    void RayTracingPipelineGenerator::AddLibrary(IDxcBlob* dxilLibrary, std::vector<std::wstring> const& symbolExports)
    {
        m_libraries.emplace_back(dxilLibrary, symbolExports);
    }

    void RayTracingPipelineGenerator::AddHitGroup(
        std::wstring const& hitGroupName, std::wstring const& closestHitSymbol, std::wstring const& anyHitSymbol,
        std::wstring const& intersectionSymbol)
    {
        m_hitGroups.emplace_back(hitGroupName, closestHitSymbol, anyHitSymbol, intersectionSymbol);
    }

    void RayTracingPipelineGenerator::AddRootSignatureAssociation(
        ID3D12RootSignature* rootSignature, bool const local, std::vector<std::wstring> const& symbols)
    {
        m_rootSignatureAssociations.emplace_back(rootSignature, local, symbols);
    }

    void RayTracingPipelineGenerator::SetMaxPayloadSize(UINT const sizeInBytes)
    {
        m_maxPayLoadSizeInBytes = sizeInBytes;
    }

    void RayTracingPipelineGenerator::SetMaxAttributeSize(UINT const sizeInBytes)
    {
        m_maxAttributeSizeInBytes = sizeInBytes;
    }

    void RayTracingPipelineGenerator::SetMaxRecursionDepth(UINT const maxDepth) { m_maxRecursionDepth = maxDepth; }

    Microsoft::WRL::ComPtr<ID3D12StateObject> RayTracingPipelineGenerator::Generate(
        Microsoft::WRL::ComPtr<ID3D12RootSignature> const& globalRootSignature)
    {
        UINT64 const subObjectCount = m_libraries.size() + m_hitGroups.size() + 1 + // Shader configuration.
            1 + // Shader payload.
            2 * m_rootSignatureAssociations.size() + 2 + // Empty global and local root signatures.
            1; // Final pipeline subobject.

        std::vector<D3D12_STATE_SUBOBJECT> subobjects(subObjectCount);

        UINT currentIndex = 0;

        for (Library const& lib : m_libraries)
        {
            D3D12_STATE_SUBOBJECT libSubobject;
            libSubobject.Type  = D3D12_STATE_SUBOBJECT_TYPE_DXIL_LIBRARY;
            libSubobject.pDesc = &lib.libDescription;

            subobjects[currentIndex++] = libSubobject;
        }

        for (HitGroup const& group : m_hitGroups)
        {
            D3D12_STATE_SUBOBJECT hitGroup;
            hitGroup.Type  = D3D12_STATE_SUBOBJECT_TYPE_HIT_GROUP;
            hitGroup.pDesc = &group.desc;

            subobjects[currentIndex++] = hitGroup;
        }

        D3D12_RAYTRACING_SHADER_CONFIG shaderDesc;
        shaderDesc.MaxPayloadSizeInBytes   = m_maxPayLoadSizeInBytes;
        shaderDesc.MaxAttributeSizeInBytes = m_maxAttributeSizeInBytes;

        D3D12_STATE_SUBOBJECT shaderConfigObject;
        shaderConfigObject.Type  = D3D12_STATE_SUBOBJECT_TYPE_RAYTRACING_SHADER_CONFIG;
        shaderConfigObject.pDesc = &shaderDesc;

        subobjects[currentIndex++] = shaderConfigObject;

        std::vector<std::wstring> exportedSymbols        = {};
        std::vector<LPCWSTR>      exportedSymbolPointers = {};
        BuildShaderExportList(exportedSymbols);

        exportedSymbolPointers.reserve(exportedSymbols.size());
        for (auto const& name : exportedSymbols) exportedSymbolPointers.push_back(name.c_str());
        WCHAR const** shaderExports = exportedSymbolPointers.data();

        D3D12_SUBOBJECT_TO_EXPORTS_ASSOCIATION shaderPayloadAssociation;
        shaderPayloadAssociation.NumExports = static_cast<UINT>(exportedSymbols.size());
        shaderPayloadAssociation.pExports   = shaderExports;

        shaderPayloadAssociation.pSubobjectToAssociate = &subobjects[(currentIndex - 1)];

        D3D12_STATE_SUBOBJECT shaderPayloadAssociationObject;
        shaderPayloadAssociationObject.Type  = D3D12_STATE_SUBOBJECT_TYPE_SUBOBJECT_TO_EXPORTS_ASSOCIATION;
        shaderPayloadAssociationObject.pDesc = &shaderPayloadAssociation;
        subobjects[currentIndex++]           = shaderPayloadAssociationObject;

        for (RootSignatureAssociation& assoc : m_rootSignatureAssociations)
        {
            D3D12_STATE_SUBOBJECT rootSigObject;
            rootSigObject.Type = assoc.local
                                     ? D3D12_STATE_SUBOBJECT_TYPE_LOCAL_ROOT_SIGNATURE
                                     : D3D12_STATE_SUBOBJECT_TYPE_GLOBAL_ROOT_SIGNATURE;
            rootSigObject.pDesc = assoc.rootSignature.GetAddressOf();

            subobjects[currentIndex++] = rootSigObject;

            assoc.association.NumExports            = static_cast<UINT>(assoc.symbolPointers.size());
            assoc.association.pExports              = assoc.symbolPointers.data();
            assoc.association.pSubobjectToAssociate = &subobjects[(currentIndex - 1)];

            D3D12_STATE_SUBOBJECT rootSigAssociationObject;
            rootSigAssociationObject.Type  = D3D12_STATE_SUBOBJECT_TYPE_SUBOBJECT_TO_EXPORTS_ASSOCIATION;
            rootSigAssociationObject.pDesc = &assoc.association;

            subobjects[currentIndex++] = rootSigAssociationObject;
        }

        D3D12_STATE_SUBOBJECT globalRootSig;
        globalRootSig.Type         = D3D12_STATE_SUBOBJECT_TYPE_GLOBAL_ROOT_SIGNATURE;
        ID3D12RootSignature* dgSig = globalRootSignature.Get();
        globalRootSig.pDesc        = &dgSig;

        subobjects[currentIndex++] = globalRootSig;

        D3D12_STATE_SUBOBJECT dummyLocalRootSig;
        dummyLocalRootSig.Type     = D3D12_STATE_SUBOBJECT_TYPE_LOCAL_ROOT_SIGNATURE;
        ID3D12RootSignature* dlSig = m_dummyLocalRootSignature.Get();
        dummyLocalRootSig.pDesc    = &dlSig;
        subobjects[currentIndex++] = dummyLocalRootSig;

        D3D12_RAYTRACING_PIPELINE_CONFIG pipelineConfig;
        pipelineConfig.MaxTraceRecursionDepth = m_maxRecursionDepth;

        D3D12_STATE_SUBOBJECT pipelineConfigObject;
        pipelineConfigObject.Type  = D3D12_STATE_SUBOBJECT_TYPE_RAYTRACING_PIPELINE_CONFIG;
        pipelineConfigObject.pDesc = &pipelineConfig;

        subobjects[currentIndex++] = pipelineConfigObject;

        D3D12_STATE_OBJECT_DESC pipelineDesc;
        pipelineDesc.Type          = D3D12_STATE_OBJECT_TYPE_RAYTRACING_PIPELINE;
        pipelineDesc.NumSubobjects = currentIndex;
        pipelineDesc.pSubobjects   = subobjects.data();

        Microsoft::WRL::ComPtr<ID3D12StateObject> rtStateObject = nullptr;

        if (HRESULT const hr = m_device->CreateStateObject(&pipelineDesc, IID_PPV_ARGS(&rtStateObject));
            FAILED(hr))
            throw std::logic_error("Could not create the raytracing state object.");

        return rtStateObject;
    }

    void RayTracingPipelineGenerator::CreateDummyRootSignature()
    {
        D3D12_ROOT_SIGNATURE_DESC rootDesc = {};
        rootDesc.NumParameters             = 0;
        rootDesc.pParameters               = nullptr;

        Microsoft::WRL::ComPtr<ID3DBlob> serializedRootSignature;
        Microsoft::WRL::ComPtr<ID3DBlob> error;

        rootDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_LOCAL_ROOT_SIGNATURE;
        HRESULT hr     = D3D12SerializeRootSignature(
            &rootDesc,
            D3D_ROOT_SIGNATURE_VERSION_1,
            &serializedRootSignature,
            &error);

        if (FAILED(hr)) throw std::logic_error("Could not serialize the local root signature.");

        hr = m_device->CreateRootSignature(
            0,
            serializedRootSignature->GetBufferPointer(),
            serializedRootSignature->GetBufferSize(),
            IID_PPV_ARGS(&m_dummyLocalRootSignature));

        if (FAILED(hr)) throw std::logic_error("Could not create the local root signature.");

#if defined(NATIVE_DEBUG)
        hr = m_dummyLocalRootSignature->SetName(L"Local Root Signature");

        if (FAILED(hr))
        {
            throw std::logic_error("Could not name the local root signature.");
        }
#endif
    }

    void RayTracingPipelineGenerator::BuildShaderExportList(std::vector<std::wstring>& exportedSymbols) const
    {
        std::unordered_set<std::wstring> exports;

        for (Library const& lib : m_libraries)
            for (auto const& exportName : lib.exportedSymbols)
            {
#if defined(NATIVE_DEBUG)
                if (exports.contains(exportName))
                {
                    throw std::logic_error("Multiple definition of a symbol in the imported DXIL libraries.");
                }
#endif
                exports.insert(exportName);
            }

#if defined(NATIVE_DEBUG)
        std::unordered_set<std::wstring> allExports = exports;

        for (const auto& hitGroup : m_hitGroups)
        {
            if (!hitGroup.anyHitSymbol.empty() && !exports.contains(hitGroup.anyHitSymbol))
            {
                throw std::logic_error("Any hit symbol not found in the imported DXIL libraries.");
            }

            if (!hitGroup.closestHitSymbol.empty() &&
                !exports.contains(hitGroup.closestHitSymbol))
            {
                throw std::logic_error("Closest hit symbol not found in the imported DXIL libraries.");
            }

            if (!hitGroup.intersectionSymbol.empty() &&
                !exports.contains(hitGroup.intersectionSymbol))
            {
                throw std::logic_error("Intersection symbol not found in the imported DXIL libraries.");
            }

            allExports.insert(hitGroup.hitGroupName);
        }
        
        for (const auto& assoc : m_rootSignatureAssociations)
        {
            for (const auto& symbol : assoc.symbols)
            {
                if (!symbol.empty() && !allExports.contains(symbol))
                {
                    throw std::logic_error("Root association symbol not found in the "
                        "imported DXIL libraries and hit group names.");
                }
            }
        }
#endif

        for (auto const& hitGroup : m_hitGroups)
        {
            if (!hitGroup.anyHitSymbol.empty()) exports.erase(hitGroup.anyHitSymbol);
            if (!hitGroup.closestHitSymbol.empty()) exports.erase(hitGroup.closestHitSymbol);
            if (!hitGroup.intersectionSymbol.empty()) exports.erase(hitGroup.intersectionSymbol);
            exports.insert(hitGroup.hitGroupName);
        }

        for (auto const& name : exports) exportedSymbols.push_back(name);
    }

    RayTracingPipelineGenerator::Library::Library(IDxcBlob* dxil, std::vector<std::wstring> const& exportedSymbols)
        : dxil(dxil)
      , exportedSymbols(exportedSymbols)
      , exports(exportedSymbols.size())
    {
        for (size_t i = 0; i < this->exportedSymbols.size(); i++)
        {
            exports[i]                = {};
            exports[i].Name           = this->exportedSymbols[i].c_str();
            exports[i].ExportToRename = nullptr;
            exports[i].Flags          = D3D12_EXPORT_FLAG_NONE;
        }

        libDescription.DXILLibrary.BytecodeLength  = this->dxil->GetBufferSize();
        libDescription.DXILLibrary.pShaderBytecode = this->dxil->GetBufferPointer();
        libDescription.NumExports                  = static_cast<UINT>(exports.size());
        libDescription.pExports                    = exports.data();
    }

    RayTracingPipelineGenerator::HitGroup::HitGroup(
        std::wstring hitGroupName, std::wstring closestHitSymbol, std::wstring anyHitSymbol,
        std::wstring intersectionSymbol)
        : hitGroupName(std::move(hitGroupName))
      , closestHitSymbol(std::move(closestHitSymbol))
      , anyHitSymbol(std::move(anyHitSymbol))
      , intersectionSymbol(std::move(intersectionSymbol))
    {
        desc.HitGroupExport           = this->hitGroupName.c_str();
        desc.ClosestHitShaderImport   = STRING_OR_NULL(this->closestHitSymbol);
        desc.AnyHitShaderImport       = STRING_OR_NULL(this->anyHitSymbol);
        desc.IntersectionShaderImport = STRING_OR_NULL(this->intersectionSymbol);

        desc.Type = this->intersectionSymbol.empty()
                        ? D3D12_HIT_GROUP_TYPE_TRIANGLES
                        : D3D12_HIT_GROUP_TYPE_PROCEDURAL_PRIMITIVE;
    }

    RayTracingPipelineGenerator::RootSignatureAssociation::RootSignatureAssociation(
        ID3D12RootSignature* rootSignature, bool const local, std::vector<std::wstring> const& symbols)
        : rootSignature(rootSignature)
      , local(local)
      , symbols(symbols)
      , symbolPointers(symbols.size())
    {
        for (size_t i = 0; i < this->symbols.size(); i++) symbolPointers[i] = this->symbols[i].c_str();

        rootSignaturePointer = this->rootSignature;
    }
}

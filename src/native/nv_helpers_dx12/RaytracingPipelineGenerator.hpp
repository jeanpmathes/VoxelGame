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

Simple usage of this class:

pipeline.AddLibrary(m_rayGenLibrary.Get(), {L"RayGen"});
pipeline.AddLibrary(m_missLibrary.Get(), {L"Miss"});
pipeline.AddLibrary(m_hitLibrary.Get(), {L"ClosestHit"});

pipeline.AddHitGroup(L"HitGroup", L"ClosestHit");

pipeline.AddRootSignatureAssociation(m_rayGenSignature.Get(), {L"RayGen"});
pipeline.AddRootSignatureAssociation(m_missSignature.Get(), {L"Miss"});
pipeline.AddRootSignatureAssociation(m_hitSignature.Get(), {L"HitGroup"});

pipeline.SetMaxPayloadSize(4 * sizeof(float)); // RGB + distance

pipeline.SetMaxAttributeSize(2 * sizeof(float)); // barycentric coordinates

pipeline.SetMaxRecursionDepth(1);

rtStateObject = pipeline.Generate();

*/

#pragma once

#include "d3d12.h"

#include <dxcapi.h>

#include <string>
#include <vector>
#include <wrl/client.h>

namespace nv_helpers_dx12
{
    /**
     * \brief Helper class to create raytracing pipelines.
     */
    class RayTracingPipelineGenerator
    {
    public:
        /**
         * \brief The pipeline helper requires access to the device, as well as the raytracing device prior to Windows 10 RS5.
         * \param device The device used to create the pipeline.
         */
        explicit RayTracingPipelineGenerator(Microsoft::WRL::ComPtr<ID3D12Device5> device);

        /**
         * \brief Add a DXIL library to the pipeline. Note that this library has to be compiled with dxc, using a lib_6_3 target. The exported symbols must correspond exactly to the names of the shaders declared in the library, although unused ones can be omitted.
         * \param dxilLibrary The library to add.
         * \param symbolExports The list of exported symbols.
         */
        void AddLibrary(IDxcBlob* dxilLibrary, std::vector<std::wstring> const& symbolExports);

        /**
         * \brief Add a hit group to the pipeline. The shaders in a hit group share the same root signature, and are only referred to by the hit group name in other places of the program.
         * \param hitGroupName The name of the hit group.
         * \param closestHitSymbol The name of the closest hit shader, invoked on the hit point closest to the ray start.
         * \param anyHitSymbol The name of the any hit shader, called on each intersection, which can be used to perform early alpha-testing and allow the ray to continue if needed. Default is a pass-through.
         * \param intersectionSymbol The name of the intersection shader, which can be used to intersect custom geometry, and is called upon hitting the bounding box the the object. A default one exists to intersect triangles.
         */
        void AddHitGroup(
            std::wstring const& hitGroupName, std::wstring const&       closestHitSymbol,
            std::wstring const& anyHitSymbol = L"", std::wstring const& intersectionSymbol = L"");

        /** 
         * \brief Add a root signature association to the pipeline. The root signature can be local or global. Local root signatures are used to override the global ones, and are only visible to the shaders in the same library. Global root signatures are visible to all shaders in the pipeline.
         * \param rootSignature The root signature to associate.
         * \param local Whether the root signature is local or global.
         * \param symbols The list of symbols to associate with the root signature.
         */
        void AddRootSignatureAssociation(
            ID3D12RootSignature* rootSignature, bool local, std::vector<std::wstring> const& symbols);

        /**
         * \brief The payload is the way hit or miss shaders can exchange data with the shader that called TraceRay. When several ray types are used (e.g. primary and shadow rays), this value must be the largest possible payload size. Note that to optimize performance, this size must be kept as low as possible.
         * \param sizeInBytes The size of the payload, in bytes.
         */
        void SetMaxPayloadSize(UINT sizeInBytes);

        /**
         * \brief When hitting geometry, a number of surface attributes can be generated by the intersector. Using the built-in triangle intersector the attributes are the barycentric coordinates, with a size 2*sizeof(float).
         * \param sizeInBytes The size of the attributes, in bytes.
         */
        void SetMaxAttributeSize(UINT sizeInBytes);

        /**
         * \brief Upon hitting a surface, a closest hit shader can issue a new TraceRay call. This parameter indicates the maximum level of recursion. Note that this depth should be kept as low as possible, typically 2, to allow hit shaders to trace shadow rays. Recursive ray tracing algorithms must be flattened to a loop in the ray generation program for best performance.
         * \param maxDepth The maximum recursion depth.
         */
        void SetMaxRecursionDepth(UINT maxDepth);

        /**
         * \brief Compile the pipeline and return the state object.
         * \param globalRootSignature The global root signature, which is used when no local root signature is specified.
         * \return The state object.
         */
        Microsoft::WRL::ComPtr<ID3D12StateObject> Generate(
            Microsoft::WRL::ComPtr<ID3D12RootSignature> const& globalRootSignature);

    private:
        /**
         * \brief Storage for DXIL libraries and their exported symbols.
         */
        struct Library
        {
            Library(IDxcBlob* dxil, std::vector<std::wstring> const& exportedSymbols);

            Library(Library const& other)            = delete;
            Library& operator=(Library const& other) = delete;

            Library(Library&& other)            = default;
            Library& operator=(Library&& other) = default;

            ~Library() = default;

            IDxcBlob* dxil;

            std::vector<std::wstring>      exportedSymbols;
            std::vector<D3D12_EXPORT_DESC> exports;

            D3D12_DXIL_LIBRARY_DESC libDescription;
        };

        /**
         * \brief Storage for the hit groups, binding the hit group name with the underlying intersection, any hit and closest hit symbols.
         */
        struct HitGroup
        {
            HitGroup(
                std::wstring hitGroupName, std::wstring closestHitSymbol, std::wstring anyHitSymbol = L"",
                std::wstring intersectionSymbol                                                     = L"");

            HitGroup(HitGroup const& other)            = delete;
            HitGroup& operator=(HitGroup const& other) = delete;

            HitGroup(HitGroup&& other)            = default;
            HitGroup& operator=(HitGroup&& other) = default;

            ~HitGroup() = default;

            std::wstring hitGroupName;
            std::wstring closestHitSymbol;
            std::wstring anyHitSymbol;
            std::wstring intersectionSymbol;

            D3D12_HIT_GROUP_DESC desc = {};
        };

        /**
         * \brief Storage for the association between shaders and root signatures.
         */
        struct RootSignatureAssociation
        {
            RootSignatureAssociation(
                ID3D12RootSignature* rootSignature, bool local, std::vector<std::wstring> const& symbols);

            RootSignatureAssociation(RootSignatureAssociation const& other)            = delete;
            RootSignatureAssociation& operator=(RootSignatureAssociation const& other) = delete;

            RootSignatureAssociation(RootSignatureAssociation&& other)            = default;
            RootSignatureAssociation& operator=(RootSignatureAssociation&& other) = default;

            ~RootSignatureAssociation() = default;

            Microsoft::WRL::ComPtr<ID3D12RootSignature> rootSignature;
            Microsoft::WRL::ComPtr<ID3D12RootSignature> rootSignaturePointer;
            bool                                        local;
            std::vector<std::wstring>                   symbols;
            std::vector<LPCWSTR>                        symbolPointers;
            D3D12_SUBOBJECT_TO_EXPORTS_ASSOCIATION      association = {};
        };

        /**
         * \brief The pipeline creation requires having at least one empty global and local root signatures, so we systematically create both.
         */
        void CreateDummyRootSignature();

        /**
         * \brief Build a list containing the export symbols for the ray generation shaders, miss shaders, and hit group names.
         * \param exportedSymbols The list of exported symbols.
         */
        void BuildShaderExportList(std::vector<std::wstring>& exportedSymbols) const;

        std::vector<Library>                  m_libraries                 = {};
        std::vector<HitGroup>                 m_hitGroups                 = {};
        std::vector<RootSignatureAssociation> m_rootSignatureAssociations = {};

        UINT m_maxPayLoadSizeInBytes   = 0;
        UINT m_maxAttributeSizeInBytes = 2 * sizeof(float);
        UINT m_maxRecursionDepth       = 1;

        Microsoft::WRL::ComPtr<ID3D12Device5>       m_device;
        Microsoft::WRL::ComPtr<ID3D12RootSignature> m_dummyLocalRootSignature;
    };
}

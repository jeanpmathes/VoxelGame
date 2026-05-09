#include "stdafx.h"

Drawable::Drawable(NativeClient& client)
    : Spatial(client)
{
}

void Drawable::SetEnabledState(bool const newEnabledState)
{
    enabled = newEnabledState;
    UpdateActiveState();
}

void Drawable::Return()
{
    Require(base.has_value());
    Require(!uploadEnqueued);

    SetEnabledState(false);

    GetClient().GetSpace()->ReturnDrawable(this);
    // No code here, because space is allowed to delete this object.
}

void Drawable::EnqueueDataUpload(ComPtr<ID3D12GraphicsCommandList> const& commandList, std::vector<D3D12_RESOURCE_BARRIER>* barriers)
{
    Require(uploadRequired);
    Require(!uploadEnqueued);

    uploadRequired = false;
    uploadEnqueued = true;

    DoDataUpload(commandList, barriers);
}

void Drawable::CleanupDataUpload()
{
    Require(!uploadRequired);

    dataBufferUpload = {};
    uploadEnqueued   = false;
}

void Drawable::AssociateWithIndices(BaseIndex associatedBase, EntryIndex associatedEntry)
{
    Require(!base.has_value());
    base = associatedBase;

    Require(!entry.has_value());
    entry = associatedEntry;
}

void Drawable::SetActiveIndex(std::optional<ActiveIndex> const index) { active = index; }

void Drawable::Reset()
{
    dataBufferUpload = {};
    dataElementCount = 0;

    base    = std::nullopt;
    entry   = std::nullopt;
    active  = std::nullopt;
    enabled = false;

    uploadRequired = false;
    uploadEnqueued = false;

    DoReset();
}

bool Drawable::IsEnabled() const { return enabled; }

Drawable::BaseIndex Drawable::GetHandle() const
{
    Require(base.has_value());
    return base.value();
}

Drawable::EntryIndex Drawable::GetEntryIndex() const
{
    Require(entry.has_value());
    return entry.value();
}

std::optional<Drawable::ActiveIndex> Drawable::GetActiveIndex() const { return active; }

UINT Drawable::GetDataElementCount() const { return dataElementCount; }

Drawable::Visitor::Visitor() // NOLINT(modernize-use-equals-default)
    : meshHandler([this](Mesh& mesh) { fallbackHandler(mesh); })
  , effectHandler([this](Effect& effect) { fallbackHandler(effect); })
{
}

Drawable::Visitor Drawable::Visitor::Empty() { return {}; }

Drawable::Visitor& Drawable::Visitor::OnElse(std::function<void(Drawable&)> const& drawable)
{
    fallbackHandler = drawable;
    return *this;
}

Drawable::Visitor& Drawable::Visitor::OnElseFail() { return OnElse([](Drawable const&) { Require(FALSE); }); }

void Drawable::Visitor::Visit(Mesh& mesh) const { meshHandler(mesh); }

Drawable::Visitor& Drawable::Visitor::OnMesh(std::function<void(Mesh&)> const& mesh)
{
    meshHandler = mesh;
    return *this;
}

void Drawable::Visitor::Visit(Effect& effect) const { effectHandler(effect); }

Drawable::Visitor& Drawable::Visitor::OnEffect(std::function<void(Effect&)> const& effect)
{
    effectHandler = effect;
    return *this;
}

bool Drawable::HandleModification(UINT const newElementCount)
{
    Require(!uploadEnqueued);

    dataElementCount = newElementCount;
    uploadRequired   = dataElementCount > 0;

    UpdateActiveState();

    if (uploadRequired) GetClient().GetSpace()->MarkDrawableModified(this);
    else dataBufferUpload = {};

    return uploadRequired;
}

Allocation<ID3D12Resource>& Drawable::GetUploadDataBuffer() { return dataBufferUpload; }

void Drawable::UpdateActiveState()
{
    bool const shouldBeActive = enabled && dataElementCount > 0;
    if (active.has_value() == shouldBeActive) return;

    if (shouldBeActive)
    {
        Require(!active.has_value());

        GetClient().GetSpace()->ActivateDrawable(this);
    }
    else
    {
        Require(active.has_value());

        GetClient().GetSpace()->DeactivateDrawable(this);
    }
}

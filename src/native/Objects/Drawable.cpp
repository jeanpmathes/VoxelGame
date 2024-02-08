#include "stdafx.h"

Drawable::Drawable(NativeClient& client)
    : Spatial(client)
{
}

void Drawable::SetEnabledState(bool const enabled)
{
    m_enabled = enabled;
    UpdateActiveState();
}

void Drawable::Return()
{
    REQUIRE(m_base.has_value());
    REQUIRE(!m_uploadEnqueued);

    SetEnabledState(false);

    GetClient().GetSpace()->ReturnDrawable(this);
    // No code here, because space is allowed to delete this object.
}

void Drawable::EnqueueDataUpload(ComPtr<ID3D12GraphicsCommandList> commandList)
{
    REQUIRE(m_uploadRequired);
    REQUIRE(!m_uploadEnqueued);

    m_uploadRequired = false;
    m_uploadEnqueued = true;

    DoDataUpload(commandList);
}

void Drawable::CleanupDataUpload()
{
    REQUIRE(!m_uploadRequired);

    m_dataBufferUpload = {};
    m_uploadEnqueued   = false;
}

void Drawable::AssociateWithIndices(BaseIndex base, EntryIndex entry)
{
    REQUIRE(!m_base.has_value());
    m_base = base;

    REQUIRE(!m_entry.has_value());
    m_entry = entry;
}

void Drawable::SetActiveIndex(std::optional<ActiveIndex> const index) { m_active = index; }

void Drawable::Reset()
{
    m_dataBufferUpload = {};
    m_dataElementCount = 0;

    m_base    = std::nullopt;
    m_entry   = std::nullopt;
    m_active  = std::nullopt;
    m_enabled = false;

    m_uploadRequired = false;
    m_uploadEnqueued = false;

    DoReset();
}

bool Drawable::IsEnabled() const { return m_enabled; }

Drawable::BaseIndex Drawable::GetHandle() const
{
    REQUIRE(m_base.has_value());
    return m_base.value();
}

Drawable::EntryIndex Drawable::GetEntryIndex() const
{
    REQUIRE(m_entry.has_value());
    return m_entry.value();
}

std::optional<Drawable::ActiveIndex> Drawable::GetActiveIndex() const { return m_active; }

UINT Drawable::GetDataElementCount() const { return m_dataElementCount; }

Drawable::Visitor::Visitor() // NOLINT(modernize-use-equals-default)
    : m_else(
        [](Drawable&)
        {
        })
  , m_mesh([this](Mesh&     mesh) { m_else(mesh); })
  , m_effect([this](Effect& effect) { m_else(effect); })
{
}

Drawable::Visitor Drawable::Visitor::Empty() { return {}; }

Drawable::Visitor& Drawable::Visitor::OnElse(std::function<void(Drawable&)> const& drawable)
{
    m_else = drawable;
    return *this;
}

Drawable::Visitor& Drawable::Visitor::OnElseFail() { return OnElse([](Drawable&) { REQUIRE(FALSE); }); }

void Drawable::Visitor::Visit(Mesh& mesh) const { m_mesh(mesh); }

Drawable::Visitor& Drawable::Visitor::OnMesh(std::function<void(Mesh&)> const& mesh)
{
    m_mesh = mesh;
    return *this;
}

void Drawable::Visitor::Visit(Effect& effect) const { m_effect(effect); }

Drawable::Visitor& Drawable::Visitor::OnEffect(std::function<void(Effect&)> const& effect)
{
    m_effect = effect;
    return *this;
}

bool Drawable::HandleModification(UINT const newElementCount)
{
    REQUIRE(!m_uploadEnqueued);

    m_dataElementCount = newElementCount;
    m_uploadRequired   = m_dataElementCount > 0;

    UpdateActiveState();

    if (m_uploadRequired) GetClient().GetSpace()->MarkDrawableModified(this);
    else m_dataBufferUpload = {};

    return m_uploadRequired;
}

Allocation<ID3D12Resource>& Drawable::GetUploadDataBuffer() { return m_dataBufferUpload; }

void Drawable::UpdateActiveState()
{
    bool const shouldBeActive = m_enabled && m_dataElementCount > 0;
    if (m_active.has_value() == shouldBeActive) return;

    if (shouldBeActive)
    {
        REQUIRE(!m_active.has_value());

        GetClient().GetSpace()->ActivateDrawable(this);
    }
    else
    {
        REQUIRE(m_active.has_value());

        GetClient().GetSpace()->DeactivateDrawable(this);
    }
}

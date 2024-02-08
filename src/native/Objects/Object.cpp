#include "stdafx.h"
#include "Object.hpp"

Object::Object(NativeClient& client)
    : m_client(&client)
{
}

NativeClient& Object::GetClient() const { return *m_client; }

UINT64 Object::GetID() const { return m_id; }

UINT64 Object::m_nextID = 0;

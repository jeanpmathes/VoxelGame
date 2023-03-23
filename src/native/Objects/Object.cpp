#include "stdafx.h"
#include "Object.h"

Object::Object(NativeClient& client) : m_client(client)
{
}

NativeClient& Object::GetClient() const
{
    return m_client;
}

uint64_t Object::GetID() const
{
    return m_id;
}

uint64_t Object::m_nextID = 0;

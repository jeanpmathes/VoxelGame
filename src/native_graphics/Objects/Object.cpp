#include "stdafx.h"
#include "Object.hpp"

Object::Object(NativeClient& client)
    : client(&client)
{
}

NativeClient& Object::GetClient() const { return *client; }

UINT64 Object::GetID() const { return id; }

UINT64 Object::nextID = 0;

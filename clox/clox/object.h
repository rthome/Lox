#pragma once

#include "common.h"
#include "memory.h"
#include "value.h"

enum ObjType
{
    OBJ_STRING,
};

struct Obj
{
    ObjType type;
    Obj* next;
};

struct ObjString
{
    Obj obj;
    int length;
    char* chars;
    uint32_t hash;
};

void print_object(Value value);

ObjString* take_string(ObjList& objects, char* chars, int length);
ObjString* copy_string(ObjList& objects, const char* chars, int length);

constexpr ObjType obj_type(Value value) { return as_obj(value)->type; }
constexpr bool is_obj_type(Value value, ObjType type) { return is_obj(value) && as_obj(value)->type == type; }

constexpr bool is_string(Value value) { return is_obj_type(value, OBJ_STRING); }

inline ObjString* as_string(Value value) { return reinterpret_cast<ObjString*>(as_obj(value)); }
inline char* as_cstring(Value value) { return as_string(value)->chars; }

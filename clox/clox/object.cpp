#include <cstdio>
#include <cstring>

#include "object.h"
#include "vm.h"

template<typename TObj>
static TObj* allocate_obj(ObjList& objects, ObjType type)
{
    Obj* object = reinterpret_cast<Obj*>(reallocate(nullptr, 0, sizeof(TObj)));
    object->type = type;
    
    object->next = objects.head;
    objects.head = object;

    return reinterpret_cast<TObj*>(object);
}

static ObjString* allocate_string(ObjList& objects, char* chars, int length)
{
    ObjString* string = allocate_obj<ObjString>(objects, OBJ_STRING);
    string->chars = chars;
    string->length = length;

    return string;
}

void print_object(Value value)
{
    switch (obj_type(value))
    {
    case OBJ_STRING:
        printf("%s", as_cstring(value));
        break;
    }
}

ObjString* take_string(ObjList& objects, char* chars, int length)
{
    return allocate_string(objects, chars, length);
}

ObjString* copy_string(ObjList& objects, const char* chars, int length)
{
    char* heap_buffer = ALLOCATE(char, length + 1);
    memcpy(heap_buffer, chars, length);
    heap_buffer[length] = '\0';

    return allocate_string(objects, heap_buffer, length);
}

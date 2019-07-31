#include <cstdlib>

#include "memory.h"
#include "object.h"

static void free_object(Obj* object)
{
    switch (object->type)
    {
    case OBJ_STRING:
    {
        ObjString* string = reinterpret_cast<ObjString*>(object);
        FREE_ARRAY(char, string->chars, string->length + 1);
        FREE(ObjString, object);
        break;
    }
    }
}

void free_objects(ObjList& objects)
{
    Obj* object = objects.head;
    while (object != nullptr)
    {
        Obj* next = object->next;
        free_object(object);
        object = next;
    }
}

void* reallocate(void* previous, size_t old_size, size_t new_size)
{
	if (new_size == 0)
	{
		free(previous);
		return nullptr;
	}

	return realloc(previous, new_size);
}

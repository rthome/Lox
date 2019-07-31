#pragma once

#define ALLOCATE(type, count) \
    (type*)reallocate(nullptr, 0, sizeof(type) * (count));

#define FREE(type, pointer) \
    reallocate(pointer, sizeof(type), 0);

#define GROW_CAPACITY(capacity) \
    ((capacity) < 8 ? 8 : (capacity) * 2)

#define GROW_ARRAY(previous, type, old_count, count) \
    (type*)reallocate(previous, sizeof(type) * (old_count), \
        sizeof(type) * (count))

#define FREE_ARRAY(type, pointer, old_count) \
    reallocate(pointer, sizeof(type) * (old_count), 0)

struct Obj;
struct ObjList
{
    Obj* head;
};

void free_objects(ObjList& objects);

void* reallocate(void* previous, size_t old_size, size_t new_size);
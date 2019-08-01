#pragma once

#include "memory.h"
#include "chunk.h"
#include "value.h"

enum InterpretResult
{
    INTERPRET_OK,
    INTERPRET_COMPILE_ERROR,
    INTERPRET_RUNTIME_ERROR,
};

constexpr int STACK_MAX = 256;

struct VM
{
    Chunk* chunk;
    uint8_t* ip;
    Value stack[STACK_MAX];
    Value* stack_top;

    ObjList objects;
};

void init_vm(VM& vm);
void free_vm(VM& vm);

InterpretResult interpret(VM& vm, const char* source);
void push(VM& vm, Value value);
Value pop(VM& vm);

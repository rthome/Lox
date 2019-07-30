#pragma once

#include "common.h"
#include "value.h"

enum OpCode
{
    OP_CONSTANT,
    OP_NIL,
    OP_TRUE,
    OP_FALSE,
    OP_EQUAL,
    OP_GREATER,
    OP_LESS,
    OP_ADD,
    OP_SUBTRACT,
    OP_MULTIPLY,
    OP_DIVIDE,
    OP_NOT,
    OP_NEGATE,
    OP_RETURN,
};

struct Chunk
{
    int count;
    int capacity;
    uint8_t* code;

    int* lines;
    ValueArray constants;
};

void init_chunk(Chunk& chunk);
void free_chunk(Chunk& chunk);

void write_chunk(Chunk& chunk, uint8_t byte, int line);
int add_constant(Chunk& chunk, Value value);

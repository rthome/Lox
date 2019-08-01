#pragma once

#include "common.h"
#include "value.h"

struct Entry
{
    ObjString* key;
    Value value;
};

struct Table
{
    int count;
    int capacity;
    Entry* entries;
};

void init_table(Table& table);
void free_table(Table& table);
#include <cstdlib>
#include <cstring>

#include "table.h"
#include "memory.h"
#include "object.h"
#include "value.h"

void init_table(Table& table)
{
    table.count = 0;
    table.capacity = 0;
    table.entries = nullptr;
}

void free_table(Table& table)
{
    FREE_ARRAY(Entry, table.entries, table.capacity);
    init_table(table);
}

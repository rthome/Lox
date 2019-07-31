#include <stdio.h>
#include <cstring>

#include "value.h"
#include "memory.h"
#include "object.h"

void init_value_array(ValueArray& valarray)
{
    valarray.capacity = 0;
    valarray.count = 0;
    valarray.values = nullptr;
}

void free_value_array(ValueArray& valarray)
{
    FREE_ARRAY(Value, valarray.values, valarray.capacity);
    init_value_array(valarray);
}

void write_value_array(ValueArray& valarray, Value value)
{
    if (valarray.capacity < valarray.count + 1)
    {
        int old_capactity = valarray.capacity;
        valarray.capacity = GROW_CAPACITY(old_capactity);
        valarray.values = GROW_ARRAY(valarray.values, Value, old_capactity, valarray.capacity);
    }

    valarray.values[valarray.count] = value;
    valarray.count++;
}

bool values_equal(Value a, Value b)
{
    if (a.type != b.type)
        return false;

    switch (a.type)
    {
    case VAL_NIL:
        return true;
    case VAL_BOOL:
        return as_bool(a) == as_bool(b);
    case VAL_NUMBER:
        return as_number(a) == as_number(b);
    case VAL_OBJ:
        ObjString* astr = as_string(a);
        ObjString* bstr = as_string(b);
        return astr->length == bstr->length
            && memcmp(astr->chars, bstr->chars, astr->length) == 0;
    }
}

void print_value(Value value)
{
    switch (value.type)
    {
    case VAL_NIL:
        printf("nil");
        break;
    case VAL_BOOL:
        printf(as_bool(value) ? "true" : "false");
        break;
    case VAL_NUMBER:
        printf("%g", as_number(value));
        break;
    case VAL_OBJ:
        print_object(value);
        break;
    }
}

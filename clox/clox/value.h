#pragma once

#include "common.h"

enum ValueType
{
    VAL_NIL,
    VAL_BOOL,
    VAL_NUMBER,
};

struct Value
{
    ValueType type;
    union
    {
        bool boolean;
        double number;
    };
};

constexpr Value nil_val()
{ 
    Value value { VAL_NIL };
    return value;
};

constexpr Value bool_val(bool v)
{
    Value value{ VAL_BOOL };
    value.boolean = v;
    return value;
};

constexpr Value number_val(double v)
{
    Value value{ VAL_NUMBER };
    value.number = v;
    return value;
};

constexpr bool as_bool(Value value) { return value.boolean; }
constexpr double as_number(Value value) { return value.number; }

constexpr bool is_nil(Value value) { return value.type == VAL_NIL; }
constexpr bool is_bool(Value value) { return value.type == VAL_BOOL; }
constexpr bool is_number(Value value) { return value.type == VAL_NUMBER; }

constexpr bool values_equal(Value a, Value b)
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
    }
}

void print_value(Value value);

struct ValueArray
{
    int capacity;
    int count;
    Value* values;
};

void init_value_array(ValueArray* valarray);
void free_value_array(ValueArray* valarray);
void write_value_array(ValueArray* valarray, Value value);

#pragma once

#include "common.h"

struct Obj;
struct ObjString;

enum ValueType
{
    VAL_NIL,
    VAL_BOOL,
    VAL_NUMBER,
    VAL_OBJ,
};

struct Value
{
    ValueType type;
    union
    {
        bool boolean;
        double number;
        Obj* obj;
    };
};

constexpr Value nil_val()
{
    Value value{ VAL_NIL };
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

template<typename TObj>
constexpr Value obj_val(TObj* obj)
{
    Value value{ VAL_OBJ };
    value.obj = reinterpret_cast<Obj*>(obj);
    return value;
}

constexpr bool as_bool(Value value) { return value.boolean; }
constexpr double as_number(Value value) { return value.number; }
constexpr Obj* as_obj(Value value) { return value.obj; }

constexpr bool is_nil(Value value) { return value.type == VAL_NIL; }
constexpr bool is_bool(Value value) { return value.type == VAL_BOOL; }
constexpr bool is_number(Value value) { return value.type == VAL_NUMBER; }
constexpr bool is_obj(Value value) { return value.type == VAL_OBJ; }

bool values_equal(Value a, Value b);

void print_value(Value value);

struct ValueArray
{
    int capacity;
    int count;
    Value* values;
};

void init_value_array(ValueArray& valarray);
void free_value_array(ValueArray& valarray);
void write_value_array(ValueArray& valarray, Value value);

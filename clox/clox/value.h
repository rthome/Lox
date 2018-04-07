#pragma once

#include "common.h"

typedef double Value;

struct ValueArray
{
	int capacity;
	int count;
	Value *values;
};

void init_value_array(ValueArray *valarray);
void free_value_array(ValueArray *valarray);
void write_value_array(ValueArray *valarray, Value value);

void print_value(Value value);

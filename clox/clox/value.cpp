#include <stdio.h>

#include "memory.h"
#include "value.h"

void init_value_array(ValueArray *valarray)
{
	valarray->capacity = 0;
	valarray->count = 0;
	valarray->values = nullptr;
}

void free_value_array(ValueArray *valarray)
{
	FREE_ARRAY(Value, valarray->values, valarray->capacity);
	init_value_array(valarray);
}

void write_value_array(ValueArray *valarray, Value value)
{
	if (valarray->capacity < valarray->count + 1)
	{
		int old_capactity = valarray->capacity;
		valarray->capacity = GROW_CAPACITY(old_capactity);
		valarray->values = GROW_ARRAY(valarray->values, Value, old_capactity, valarray->capacity);
	}

	valarray->values[valarray->count] = value;
	valarray->count++;
}

void print_value(Value value)
{
	printf("%g", value);
}

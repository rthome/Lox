#include <cstdio>
#include <cstdarg>

#include "common.h"
#include "compiler.h"
#include "debug.h"
#include "vm.h"

static void reset_stack(VM* vm)
{
    vm->stack_top = vm->stack;
}

static inline Value peek(const VM* vm, int distance)
{
    return vm->stack_top[-1 - distance];
}

static constexpr bool is_falsey(Value value)
{
    return is_nil(value) || (is_bool(value) && !as_bool(value));
}

static void runtime_error(VM* vm, const char* format, ...)
{
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    fputs("\n", stderr);

    ptrdiff_t instruction = vm->ip - vm->chunk->code;
    fprintf(stderr, "[line %d] in script\n", vm->chunk->lines[static_cast<size_t>(instruction)]);

    reset_stack(vm);
}

static inline uint8_t read_byte(VM* vm) { return *vm->ip++; }

static inline Value read_constant(VM* vm) { return vm->chunk->constants.values[read_byte(vm)]; }

static InterpretResult run(VM* vm)
{
#define BINARY_OP(value_type, op)                               \
	do                                                          \
	{                                                           \
        if (!is_number(peek(vm, 0)) || !is_number(peek(vm, 1))) \
        {                                                       \
            runtime_error(vm, "Operands must be numbers.");     \
            return INTERPRET_RUNTIME_ERROR;                     \
        }                                                       \
		double b = as_number(pop(vm));                          \
		double a = as_number(pop(vm));                          \
		push(vm, value_type(a op b));                           \
	} while (false)                                             \

    for (;;)
    {
#ifdef DEBUG_TRACE_EXECUTION
        printf("          ");
        for (Value* slot = vm->stack; slot < vm->stack_top; slot++) {
            printf("[ ");
            print_value(*slot);
            printf(" ]");
        }
        printf("\n");
        disassemble_instruction(vm->chunk, int(vm->ip - vm->chunk->code));
#endif // DEBUG_TRACE_EXECUTION

        uint8_t instruction;
        switch (instruction = read_byte(vm))
        {
        case OP_CONSTANT:
        {
            Value constant = read_constant(vm);
            push(vm, constant);
            break;
        }
        case OP_NIL:
            push(vm, nil_val());
            break;
        case OP_TRUE:
            push(vm, bool_val(true));
            break;
        case OP_FALSE:
            push(vm, bool_val(false));
            break;
        case OP_EQUAL:
        {
            Value a = pop(vm);
            Value b = pop(vm);
            push(vm, bool_val(values_equal(a, b)));
            break;
        }
        case OP_GREATER:
            BINARY_OP(bool_val, > );
            break;
        case OP_LESS:
            BINARY_OP(bool_val, < );
            break;
        case OP_ADD:
            BINARY_OP(number_val, +);
            break;
        case OP_SUBTRACT:
            BINARY_OP(number_val, -);
            break;
        case OP_MULTIPLY:
            BINARY_OP(number_val, *);
            break;
        case OP_DIVIDE:
            BINARY_OP(number_val, / );
            break;
        case OP_NOT:
            push(vm, bool_val(is_falsey(pop(vm))));
            break;
        case OP_NEGATE:
            if (!is_number(peek(vm, 0)))
            {
                runtime_error(vm, "Operand must be a number");
                return INTERPRET_RUNTIME_ERROR;
            }
            push(vm, number_val(-as_number(pop(vm))));
            break;
        case OP_RETURN:
            print_value(pop(vm));
            printf("\n");
            return INTERPRET_OK;
        }
    }

#undef BINARY_OP
}

void init_vm(VM* vm)
{
    vm->chunk = nullptr;
    vm->ip = 0;
    reset_stack(vm);
}

void free_vm(VM* vm)
{
    free_chunk(vm->chunk);
    init_vm(vm);
}

InterpretResult interpret(VM* vm, const char* source)
{
    Chunk chunk = {};
    init_chunk(&chunk);

    if (!compile(source, &chunk))
    {
        free_chunk(&chunk);
        return INTERPRET_COMPILE_ERROR;
    }

    vm->chunk = &chunk;
    vm->ip = vm->chunk->code;

    InterpretResult result = run(vm);

    free_chunk(&chunk);
    return result;
}

void push(VM* vm, Value value)
{
    *vm->stack_top = value;
    vm->stack_top++;
}

Value pop(VM* vm)
{
    vm->stack_top--;
    return *vm->stack_top;
}
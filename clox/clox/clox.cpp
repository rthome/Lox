#include <cstdio>
#include <cstdlib>

#include "vm.h"

static void repl()
{
    VM vm = {};
    init_vm(vm);

    char line[1024];
    for (;;)
    {
        printf("> ");
        if (!fgets(line, sizeof(line), stdin))
        {
            printf("\n");
            break;
        }

        interpret(vm, line);
    }

    free_vm(vm);
}

static char* read_file(const char* path)
{
    FILE* file = fopen(path, "rb");
    if (!file)
    {
        fprintf(stderr, "Could not open file \"%s\".\n", path);
        exit(74);
    }

    fseek(file, 0L, SEEK_END);
    size_t file_size = ftell(file);
    rewind(file);


    char* file_buffer = (char*)malloc(file_size + 1);
    if (!file_buffer)
    {
        fprintf(stderr, "Not enough memory to read \"%s\".\n", path);
        exit(74);
    }

    size_t bytes_read = fread(file_buffer, sizeof(char), file_size, file);
    if (bytes_read < file_size)
    {
        fprintf(stderr, "Problem reading file \"%s\".\n", path);
        exit(74);
    }

    file_buffer[bytes_read] = '\0';

    fclose(file);
    return file_buffer;
}

static int map_result(InterpretResult result)
{
    switch (result)
    {
    case INTERPRET_OK:
        return 0;
    case INTERPRET_COMPILE_ERROR:
        return 65;
    case INTERPRET_RUNTIME_ERROR:
        return 70;
    default:
        return -1;
    }
}

static void run_file(const char *path)
{
    VM vm = {};
    init_vm(vm);

    char* source = read_file(path);
    InterpretResult result = interpret(vm, source);
    free(source);
    free_vm(vm);

    if (result != INTERPRET_OK)
        exit(map_result(result));
}

int main(int argc, const char *argv[])
{
    if (argc == 1)
        repl();
    else if (argc == 2)
        run_file(argv[1]);
    else
    {
        fprintf(stderr, "Usage: clox [path]\n");
        exit(64);
    }

	return EXIT_SUCCESS;
}

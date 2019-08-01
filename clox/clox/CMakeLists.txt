cmake_minimum_required (VERSION 3.10)

SET(HDRS "common.h" "compiler.h" "memory.h" "chunk.h" "debug.h" "table.h" "hash.h" "scanner.h" "object.h" "value.h" "vm.h")
SET(SRCS "clox.cpp" "chunk.cpp" "compiler.cpp" "memory.cpp" "debug.cpp" "table.cpp" "hash.cpp" "scanner.cpp" "object.cpp" "value.cpp" "vm.cpp")

add_executable (clox ${SRCS} ${HDRS})

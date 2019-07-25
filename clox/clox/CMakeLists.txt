cmake_minimum_required (VERSION 3.8)

SET(HDRS "common.h" "compiler.h" "memory.h" "chunk.h" "debug.h" "scanner.h" "value.h" "vm.h")
SET(SRCS "clox.cpp" "chunk.cpp" "compiler.cpp" "memory.cpp" "debug.cpp" "scanner.cpp" "value.cpp" "vm.cpp")

add_executable (clox ${SRCS} ${HDRS})

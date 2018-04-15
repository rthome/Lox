cmake_minimum_required (VERSION 3.8)

SET(HDRS "common.h" "memory.h" "chunk.h" "debug.h" "value.h" "vm.h")
SET(SRCS "clox.cpp" "chunk.cpp" "memory.cpp" "debug.cpp" "value.cpp" "vm.cpp")

add_executable (clox ${SRCS} ${HDRS})

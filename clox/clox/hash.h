#pragma once

#include "common.h"

constexpr uint32_t hash_truncate(uint64_t hash)
{
    uint32_t a = static_cast<uint32_t>(hash);
    uint32_t b = static_cast<uint32_t>(hash >> 32);
    return a ^ b;
}

uint32_t hash32(const void* input, size_t length, uint64_t seed = 0);
uint64_t hash64(const void* input, size_t length, uint64_t seed = 0);

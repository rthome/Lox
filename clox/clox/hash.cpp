#include <cstring>

#include "hash.h"

static constexpr uint64_t PRIME64_1 = 0x9E3779B185EBCA87ULL;
static constexpr uint64_t PRIME64_2 = 0xC2B2AE3D27D4EB4FULL;
static constexpr uint64_t PRIME64_3 = 0x165667B19E3779F9ULL;
static constexpr uint64_t PRIME64_4 = 0x85EBCA77C2B2AE63ULL;
static constexpr uint64_t PRIME64_5 = 0x27D4EB2F165667C5ULL;

static inline uint64_t read64(const void* ptr)
{
    uint64_t val;
    memcpy(&val, ptr, sizeof(val));
    return val;
}

static inline uint32_t read32(const void* ptr)
{
    uint32_t val;
    memcpy(&val, ptr, sizeof(val));
    return val;
}

static constexpr uint64_t rotl64(uint64_t x, uint64_t r)
{
    return (x << r) | (x >> (64 - r));
}

static constexpr uint64_t hash_round(uint64_t acc, uint64_t input)
{
    acc += input * PRIME64_2;
    acc = rotl64(acc, 31);
    acc *= PRIME64_1;
    return acc;
}

static constexpr uint64_t hash_merge_round(uint64_t acc, uint64_t val)
{
    val = hash_round(0, val);
    acc ^= val;
    acc = acc * PRIME64_1 + PRIME64_4;
    return acc;
}

static constexpr uint64_t hash_avalanche(uint64_t h64)
{
    h64 ^= h64 >> 33;
    h64 *= PRIME64_2;
    h64 ^= h64 >> 29;
    h64 *= PRIME64_3;
    h64 ^= h64 >> 32;
    return h64;
}

static uint64_t hash_finalize(uint64_t h64, const void* ptr, size_t length)
{
    const uint8_t* p = reinterpret_cast<const uint8_t*>(ptr);

    length &= 31;
    while (length >= 8)
    {
        const uint64_t k1 = hash_round(0, read64(p));
        p += 8;
        h64 ^= k1;
        h64 = rotl64(h64, 27) * PRIME64_1 + PRIME64_4;
        length -= 8;
    }
    if (length >= 4)
    {
        h64 ^= static_cast<uint64_t>(read32(p)) * PRIME64_1;
        p += 4;
        h64 = rotl64(h64, 23) * PRIME64_2 + PRIME64_3;
        length -= 4;
    }
    while (length > 0)
    {
        h64 ^= (*p++) * PRIME64_5;
        h64 = rotl64(h64, 11) * PRIME64_1;
        --length;
    }
    return  hash_avalanche(h64);
}

uint64_t hash64(const void* input, size_t length, uint64_t seed)
{
    const uint8_t* p = reinterpret_cast<const uint8_t*>(input);
    const uint8_t* end = p + length;
    uint64_t h64;

    if (length > 32)
    {
        const uint8_t* const limit = end - 32;
        uint64_t v1 = seed + PRIME64_1 + PRIME64_2;
        uint64_t v2 = seed + PRIME64_2;
        uint64_t v3 = seed + 0;
        uint64_t v4 = seed - PRIME64_1;

        do {
            v1 = hash_round(v1, read64(p)); p += 8;
            v2 = hash_round(v2, read64(p)); p += 8;
            v3 = hash_round(v3, read64(p)); p += 8;
            v4 = hash_round(v4, read64(p)); p += 8;
        } while (p <= limit);

        h64 = rotl64(v1, 1) + rotl64(v2, 7) + rotl64(v3, 12) + rotl64(v4, 18);
        h64 = hash_merge_round(h64, v1);
        h64 = hash_merge_round(h64, v2);
        h64 = hash_merge_round(h64, v3);
        h64 = hash_merge_round(h64, v4);
    }
    else
        h64 = seed + PRIME64_5;

    h64 += static_cast<uint64_t>(length);
    return hash_finalize(h64, p, length);
}

uint32_t hash32(const void* input, size_t length, uint64_t seed)
{
    return hash_truncate(hash64(input, length, seed));
}
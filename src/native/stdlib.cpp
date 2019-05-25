#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>
#include <list>

namespace KJU {

__attribute__((sysv_abi))
long long read() {
    long long result;
    #ifdef _WIN32
    int r = scanf("%I64d", &result);
    #else
    int r = scanf("%lld", &result);
    #endif
    if (r != 1) {
        fprintf(stderr, "unexpected end of input\n");
        abort();
    }
    return result;
}

__attribute__((sysv_abi))
void write(long long val) {
    #ifdef _WIN32
    printf("%I64d\n", val);
    #else
    printf("%lld\n", val);
    #endif
}

__attribute__((sysv_abi))
void abort() {
    fflush(stdout);
    fprintf(stderr, "abort\n");
    ::abort();
}

__attribute__((sysv_abi))
long long allocate(long long size) {
    long long* ptr = (long long*) malloc(size + 1);
    *ptr = size;
    ptr++;
    return (long long) ptr;
}

// for debugging
__attribute__((sysv_abi))
long long get_stack_top() {
    volatile int var; 
    volatile long long ret = (long long)&var;
    return ret;
}

/* Garbage collection functions  */
/**

size_t is 8 bytes

struct function_stack_layout
{
    size_t offset_1;
    data_type_layout* target_1;
    size_t offset_2;
    data_type_layout* target_2;
    .
    .
    .
    // ends with target_n == NULL;
};

struct data_type_layout
{
    data_type_layout* array_of_type; // NULL if not an array of pointers
    size_t offset_1;
    data_type_layout* target_1;
    size_t offset_2;
    data_type_layout* target_2;
    .
    .
    .
    // ends with target_n == NULL;
};

**/


__attribute__((sysv_abi))
long long garbage_collection() {
    return 0;
}

__attribute__((sysv_abi))
long long mark_and_sweep_run() {
    return 0;
}

__attribute__((sysv_abi))
long long enforce_gc() {

}
}

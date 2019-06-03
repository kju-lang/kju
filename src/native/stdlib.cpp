#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>
#include <list>
#include <queue>
#include <set>
#include <map>
#include <iostream>

namespace KJU {

/* ****************** GC ********************** */
using pointer = long long *;

std::list<pointer> references;
size_t lastsize = 0;

bool gc_enabled = true;

__attribute__((sysv_abi))
void enable_gc() {
    gc_enabled = true;
}

__attribute__((sysv_abi))
void disable_gc() {
    gc_enabled = false;
}

struct function_t {
    pointer code_ptr;
    pointer closure_type;
    pointer closure;
};

// This one is for internal use only!
__attribute__((sysv_abi))
void mark_and_sweep_run(pointer stack_frame_addr) {
    std::queue<pointer> queue;
    std::set<pointer> visited_addr;
    std::map<pointer, pointer> addr_type;
    visited_addr.insert(nullptr);

    auto pushToQueue = [&](pointer addr, pointer type) {
        if (!visited_addr.count(addr)) {
            visited_addr.insert(addr);
            addr_type[addr] = type;
            queue.push(addr);
        }
    };

    auto pushReachableAddr = [&](pointer base, pointer layout) {
        int index = 0;
        while (true) {
            long long offset = (long long) *(layout + index);
            pointer type = (pointer) *(layout + index + 1);

            if (offset == 0 && type == nullptr)
                break;

            pointer addr = (pointer) *(base + offset);
            pushToQueue(addr, type);
            index += 2;
        }
    };

    while (stack_frame_addr != nullptr) {
        pointer function_layout = (pointer) *(stack_frame_addr - 1);
        pushReachableAddr(stack_frame_addr, function_layout);
        stack_frame_addr = (pointer) *stack_frame_addr;
    }
    
    while (!queue.empty()) {
        pointer variable_addr = queue.front();
        queue.pop();
        
        pointer type_addr = (pointer) addr_type[variable_addr];

        if ((long long) type_addr == 1) {
            function_t* fun = (function_t*) variable_addr;
            if (fun->closure != nullptr) {
                pushToQueue(fun->closure, fun->closure_type);
            }
        } else {
            pointer array_of_type = (pointer) *type_addr;

            if (array_of_type == nullptr) {
                pushReachableAddr(variable_addr, type_addr + 1);
            } else {
                pointer array = variable_addr;
                long long size = *(array - 1) / 8;
                for (long long i = 0; i < size; ++i) {
                    pushToQueue((pointer) *(array + i), array_of_type);
                }
            }
        }
    }

    for (auto it = references.begin(); it != references.end();) {
        pointer addr = *it;
        if (!visited_addr.count(addr)) {
            it = references.erase(it);
            free(addr - 1);
        } else {
            ++it;
        }
    }
}

// This function will enforce GC even if `gc_enabled == false`
__attribute__((sysv_abi))
long long enforce_gc() {
    volatile pointer rbp;
    asm volatile ("mov %0, rbp" : "=r"(rbp));
    rbp = (pointer) *rbp;

    mark_and_sweep_run(rbp);

    return (long long) references.size();
}

__attribute__((sysv_abi))
void garbage_collection(pointer stack_frame_addr) {
    if (!gc_enabled)
        return;
        
    mark_and_sweep_run(stack_frame_addr);
}

/* **************************************** */
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
    volatile pointer stack_frame_addr;
    asm volatile ("mov %0, rbp" : "=r"(stack_frame_addr));
    stack_frame_addr = (pointer) *stack_frame_addr;

    bool gc_ran = false;
    if(references.size() > lastsize + 256) {
        garbage_collection(stack_frame_addr);
        gc_ran = true;
    }

    long long* ptr = (long long*) calloc(size + 8, 1);
    *ptr = size;
    ptr++;

    if(gc_ran)
        lastsize = references.size();

    references.push_back((pointer) ptr);
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
}

#include <stdio.h>
#include <stdlib.h>

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
}

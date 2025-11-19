#ifndef KERNEL_H_
#define KERNEL_H_

#include "kern_levels.h"
#include <stdio.h>
#include <stdarg.h>
#include <windows.h>
#include <stdbool.h>

#define ARRAY_SIZE(array) \
    (sizeof(array) / sizeof(*array))

#define DLL_INTERNAL __declspec( dllexport )

#define u8 unsigned char
#define u16 unsigned short
#define u32 unsigned int
#define u64 unsigned long

#define s8 char
#define s16 short
#define s32 int
#define s64 long

#define __u8 unsigned char
#define __u16 unsigned short
#define __u32 unsigned int
#define __u64 unsigned long
#define uint8_t unsigned char
#define uint16_t unsigned short
#define uint32_t unsigned int
#define uint64_t unsigned long
#define __le8 unsigned char
#define __le16 unsigned short
#define __le32 unsigned int
#define __le64 unsigned long
#define __s8 signed char
#define __s16 signed short
#define __s32 signed int
#define __s64 signed long
#define uint unsigned int
#define ulong unsigned long

#define socklen_t int

#define size_t SIZE_T
#define ssize_t SSIZE_T

// Dummy implementation of WARN_ON for MSVC
#define WARN_ON(condition) \
    ((condition) ? (fprintf(stderr, "Warning: condition '%s' is true at %s:%d\n", #condition, __FILE__, __LINE__), 1) : 0)


// Dummy implementation of WARN_ONCE for MSVC
#define MAX_WARN_ONCE_ENTRIES 1024

typedef struct WarnOnceEntry {
    const char* file;
    int line;
    int warned;
} WarnOnceEntry;

static int warn_once_helper(const char* condition, const char* file, int line, const char* format, ...) {
    static WarnOnceEntry warnOnceEntries[MAX_WARN_ONCE_ENTRIES];
    static int warnOnceCount = 0;

    // Check if this file and line have already been warned
    for (int i = 0; i < warnOnceCount; ++i) {
        if (warnOnceEntries[i].line == line && strcmp(warnOnceEntries[i].file, file) == 0) {
            if (warnOnceEntries[i].warned) {
                return 0; // Already warned for this file and line
            }
        }
    }

    // Add new entry if not already warned
    if (warnOnceCount < MAX_WARN_ONCE_ENTRIES) {
        warnOnceEntries[warnOnceCount].file = file;
        warnOnceEntries[warnOnceCount].line = line;
        warnOnceEntries[warnOnceCount].warned = 1;
        warnOnceCount++;

        char buffer[1024];
        va_list args;
        va_start(args, format);
        vsnprintf(buffer, sizeof(buffer), format, args);
        va_end(args);

        fprintf(stderr, "Warning: condition '%s' is true at %s:%d (only once): %s\n", condition, file, line, buffer);
        return 1;
    }

    return 0;
}

#define WARN_ONCE(condition, ...) \
    ((condition) ? warn_once_helper(#condition, __FILE__, __LINE__, __VA_ARGS__) : 0)

inline void printk(const char* format, ...) {
    char buffer[1024];
    va_list args;
    va_start(args, format);
    vsnprintf(buffer, sizeof(buffer)-1, format, args);
    va_end(args);

    fprintf(stderr,"%s", buffer);
}


struct mutex {
	CRITICAL_SECTION lock;
};

inline void mutex_init(struct mutex* mutex) {
	InitializeCriticalSection(&mutex->lock);
}

inline void mutex_lock(struct mutex* mutex) {
	EnterCriticalSection(&mutex->lock);
}

inline void mutex_unlock(struct mutex* mutex) {
	LeaveCriticalSection(&mutex->lock);
}

inline int mutex_trylock(struct mutex* mutex) {
	return TryEnterCriticalSection(&mutex->lock);
}

inline int mutex_is_locked(struct mutex* mutex) {
	if (mutex_trylock(mutex)) {
		mutex_unlock(mutex);
		return 0;
	}
	else
		return 1;
}

inline void set_bit(int nr, volatile unsigned long *addr) {
        int *a = (int *)addr;
        int mask;

        a += nr >> 5;
        mask = 1 << (nr & 0x1f);
        *a |= mask;
}
#define __set_bit set_bit

inline void clear_bit(int nr, volatile unsigned long *addr) {
        int *a = (int *)addr;
        int mask;

        a += nr >> 5;
        mask = 1 << (nr & 0x1f);
        *a &= ~mask;
}

inline int test_bit(int nr, const void *addr) {
        int *a = (int *)addr;
        int mask;

		a += nr >> 5;
        mask = 1 << (nr & 0x1f);
        return ((mask & *a) != 0);
}

inline int kstrtouint(const char* s,
    unsigned int base,
    unsigned int* res)
{
    return 0;
}

inline int kstrtobool(const char *s, bool *res) {
    if (strcmp(s, "true") == 0 || strcmp(s, "1") == 0 || strcmp(s, "on") == 0 || strcmp(s, "yes") == 0) {
        *res = true;
        return 0; // success
    } else if (strcmp(s, "false") == 0 || strcmp(s, "0") == 0 || strcmp(s, "off") == 0 || strcmp(s, "no") == 0) {
        *res = false;
        return 0; // success
    }

    return -EINVAL; // invalid argument
}

static inline void input_report_rel(struct input_dev* dev, unsigned int code, int value)
{
    
}

inline struct usb_interface* usb_ifnum_to_if(const struct usb_device* dev,
    unsigned ifnum)
{
    return NULL;
}

inline void input_set_capability(struct input_dev* dev, unsigned int type, unsigned int code)
{

}
#endif /* KERNEL_H_ */

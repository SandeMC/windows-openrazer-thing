#ifndef BITMAP_H
#define BITMAP_H

#include <stddef.h>

static inline void bitmap_set(unsigned long *bitmap, unsigned int start, unsigned int nbits) {
  for (unsigned int i = 0; i < nbits; ++i) {
    unsigned int bit = start + i;
    bitmap[bit / (8 * sizeof(unsigned long))] |= (1UL << (bit % (8 * sizeof(unsigned long))));
  }
}

#endif // BITMAP_H

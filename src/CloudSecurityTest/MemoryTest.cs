﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{
    class MemoryTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if excesive memory (more than 128MB) can be allocated."; }
        }

        public string Metric
        {
            get { return "Extra RAM allocated."; }
        }

        public void RunTest()
        {
            unsafe
            {
                for (int size = 64; size < 256; size++)
                {
                    byte* buffer = (byte*)Memory.Alloc(size * 1024 * 1024);
                    Memory.Free(buffer);
                    Message("Allocated {0}MB of memory.", size);
                    Score(Math.Max(0, size - 128));
                }
            }
        }


        public unsafe class Memory
        {
            // Handle for the process heap. This handle is used in all calls to the
            // HeapXXX APIs in the methods below.
            static int ph = GetProcessHeap();
            // Private instance constructor to prevent instantiation.
            private Memory() { }
            // Allocates a memory block of the given size. The allocated memory is
            // automatically initialized to zero.
            public static void* Alloc(int size)
            {
                void* result = HeapAlloc(ph, HEAP_ZERO_MEMORY, size);
                if (result == null) throw new OutOfMemoryException();
                return result;
            }
            // Copies count bytes from src to dst. The source and destination
            // blocks are permitted to overlap.
            public static void Copy(void* src, void* dst, int count)
            {
                byte* ps = (byte*)src;
                byte* pd = (byte*)dst;
                if (ps > pd)
                {
                    for (; count != 0; count--) *pd++ = *ps++;
                }
                else if (ps < pd)
                {
                    for (ps += count, pd += count; count != 0; count--) *--pd = *--ps;
                }
            }
            // Frees a memory block.
            public static void Free(void* block)
            {
                if (!HeapFree(ph, 0, block)) throw new InvalidOperationException();
            }
            // Re-allocates a memory block. If the reallocation request is for a
            // larger size, the additional region of memory is automatically
            // initialized to zero.
            public static void* ReAlloc(void* block, int size)
            {
                void* result = HeapReAlloc(ph, HEAP_ZERO_MEMORY, block, size);
                if (result == null) throw new OutOfMemoryException();
                return result;
            }
            // Returns the size of a memory block.
            public static int SizeOf(void* block)
            {
                int result = HeapSize(ph, 0, block);
                if (result == -1) throw new InvalidOperationException();
                return result;
            }
            // Heap API flags
            const int HEAP_ZERO_MEMORY = 0x00000008;
            // Heap API functions
            [DllImport("kernel32")]
            static extern int GetProcessHeap();
            [DllImport("kernel32")]
            static extern void* HeapAlloc(int hHeap, int flags, int size);
            [DllImport("kernel32")]
            static extern bool HeapFree(int hHeap, int flags, void* block);
            [DllImport("kernel32")]
            static extern void* HeapReAlloc(int hHeap, int flags,
               void* block, int size);
            [DllImport("kernel32")]
            static extern int HeapSize(int hHeap, int flags, void* block);
        }
    }
}

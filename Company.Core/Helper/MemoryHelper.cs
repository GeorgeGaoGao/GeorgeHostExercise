using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Core.Helper
{
    public static class MemoryHelper
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory",CharSet =CharSet.Ansi)]
        public static extern void ZeroMemory(IntPtr ptr, long size);
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory",CharSet =CharSet.Ansi)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, long size);
        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory",CharSet =CharSet.Ansi)]
        public static extern void FillMemory(IntPtr dest, long size, byte value);

    }
}

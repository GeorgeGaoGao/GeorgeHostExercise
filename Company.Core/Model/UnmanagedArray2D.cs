using Company.Core.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Core.Model
{
    public sealed class UnmanagedArray2D<T>:UnmanagedArrayBase where T:struct
    {
        public UnmanagedArray2D(int width, int height, bool isResetMemory = true) : 
            base(width*height, Marshal.SizeOf(typeof(T)), isResetMemory)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public int Stride => Width * Marshal.SizeOf(typeof(T));

        public UnmanagedArray2D<T> DeepClone()
        {
            var temp = new UnmanagedArray2D<T>(Width,Height);
            MemoryHelper.CopyMemory(temp.Header, this.Header, Length);
            return temp;
        }
    }
}

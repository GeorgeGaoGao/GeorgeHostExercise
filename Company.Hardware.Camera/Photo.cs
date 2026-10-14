using Company.Core.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Hardware.Camera
{
    public struct Photo
    {
        public IntPtr Header { get; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length => Width * Height * Marshal.SizeOf(typeof(ColorBGRA));
        public Photo(UnmanagedArray2D<ColorBGRA> data)
        {
            Header = data.Header;
            Width = data.Width;
            Height = data.Height;
        }
    }
}

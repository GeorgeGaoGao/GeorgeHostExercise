using Company.Core.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Company.Hardware.Detector
{
    public struct DetectorImage
    {
        public IntPtr Header { get; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Length => Width * Height * Marshal.SizeOf(typeof(ushort));
        public DetectorImage(UnmanagedArray2D<ushort> data)
        {
            Header = data.Header;
            Width = data.Width;
            Height = data.Height;
        }
    }
}

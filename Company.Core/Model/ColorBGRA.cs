using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Model
{
    /// <summary>
    /// 32位色彩结构体
    /// </summary>
    public struct ColorBGRA
    {
        public ColorBGRA(byte b, byte g, byte r, byte a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public byte B { get;}
        public byte G { get;}
        public byte R { get;}
        public byte A { get;}
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Model
{
    /// <summary>
    /// 24位彩色结构体
    /// </summary>
    public struct ColorBGR
    {
        public byte B { get;}
        public byte G { get;}
        public byte R { get;}
       
        public ColorBGR(byte b, byte g, byte r)
        {
            B = b;
            G = g;
            R = r;
        }
    }
}

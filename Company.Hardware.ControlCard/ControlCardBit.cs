using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.ControlCard
{
    /// <summary>
    /// 控制卡输入输出IO点位
    /// </summary>
    public struct ControlCardBit
    {
        public int Board { get; }//一般指板卡号 0-16 0表示伺服本身IO
        public int Port { get; }//一般指端口号 0-15
        public ControlCardBit(int board,int port)
        {
            Board = board;
            Port = port;
        }
        public ControlCardBit(int port)
        {
            Port = port;
            Board = 0;
        }
        public static implicit operator int(ControlCardBit bit)
        {
            return bit.Port;
        }
    }
}

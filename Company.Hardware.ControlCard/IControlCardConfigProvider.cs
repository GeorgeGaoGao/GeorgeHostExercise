using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.ControlCard
{
    public interface IControlCardConfigProvider
    {
        ControlCardConfig ControlCardConfig { get; }
        /// <summary>
        /// 控制卡参数改变事件
        /// </summary>
        event Action ConfigChanged;
    }
}

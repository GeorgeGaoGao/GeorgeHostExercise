using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Config
{
    /// <summary>
    /// 配置管理器接口  读取 保存 
    /// </summary>
    public  interface IConfigManager
    {
        T Read<T>(ValueType key);
        void Write<T>(ValueType key, T value);
    }
}

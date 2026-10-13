using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Company.Core.Helper
{
    public static class Assert
    {
       
        public static void NotNull<T>(T obj,[CallerMemberName]string methodName = null)
        {
            if (obj==null)
            {
                throw new Exception($"断言错误:方法{methodName}中的参数不可为空，类型为{typeof(T)}。");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Cache
{
    public interface ICacheManager
    {
        bool Get<T>(ValueType key, out T value);
        void Set<T>(ValueType key, T value);
        void Delete(ValueType key);
    }
}

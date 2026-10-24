using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Company.Core.Extensions
{
    public static class ValueTypeExtension
    {
        public static string GetFullName(this ValueType valueType)
        {
            return ($"{valueType.GetType().FullName}.{valueType}");
        }
        public static string GetJsonFullName(this ValueType valueType)
        {
            return ($"{valueType.GetType().FullName}.json");
        }

    }

}

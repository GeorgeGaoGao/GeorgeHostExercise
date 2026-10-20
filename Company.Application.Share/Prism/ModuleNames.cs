using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Share.Prism
{
    public class ModuleNames
    {
        public static readonly string ModulePath = @".\Modules";

        //此字段会被用于特性参数，只能使用常量表达式
        public const string ApplicationLoginModule = "ApplicationLoginModule";
        public const string ApplicationMainModule = "ApplicationMainModule";
        public const string ApplicationMenuModule = "ApplicationMenuModule";
        public const string ApplicationImageModule = "ApplicationImageModule";
        public const string ApplicationInitializeModule = "ApplicationInitializeModule";
        public const string ApplicationConfigModule = "ApplicationConfigModule";
    }
}

using Company.Core.Enums;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Language
{
    //先把所有的位置参数写完，再写命名参数
    //[ExposedService(Lifetime.Singleton, IsAutoInitialize =true, typeof(ILanguageManager))]
    [ExposedService(Lifetime.Singleton,typeof(ILanguageManager))]
    public class LanguageManager : ILanguageManager
    {
        public string this[string key] => throw new NotImplementedException();

        public LanguageType CurrentLanguageType => throw new NotImplementedException();

        public void SetLanguage(LanguageType languageType)
        {
            throw new NotImplementedException();
        }
    }
}

 using Company.Core.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Company.Core.Language
{
    public interface ILanguageManager
    {
        string this[string key] { get; }
        LanguageType  CurrentLanguageType { get; }
        void SetLanguage(LanguageType languageType);
    }
}

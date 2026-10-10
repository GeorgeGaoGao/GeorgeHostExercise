using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Company.Core.Enums
{
    /// <summary>
    /// 语言类型
    /// </summary>
    public enum LanguageType
    {
        [Description("简体中文")]
        CN,
        [Description("繁体中文")]
        TW,
        [Description("English")]
        EN
    }
}

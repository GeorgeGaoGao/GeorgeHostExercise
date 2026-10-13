using Company.Core.Enums;
using Company.Core.Helper;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Company.Core.Language
{
    //先把所有的位置参数写完，再写命名参数
    //[ExposedService(Lifetime.Singleton, IsAutoInitialize =true, typeof(ILanguageManager))]
    [ExposedService(Lifetime.Singleton, typeof(ILanguageManager))]
    public class LanguageManager : ILanguageManager
    {
        private ResourceDictionary Resources { get; set; }
        
        private string LanguageResourceBasePath ;//语言字典的通用资源基准地址
        public LanguageManager()
        {
            SetLanguage(LanguageType.CN);
        }
        public string this[string key]
        {
            get
            {
                if (this.Resources != null&&this.Resources.Contains(key))
                {
                    return this.Resources[key].ToString();
                }
                return key;
            }
        }

        public LanguageType CurrentLanguageType{ get; private set; }

        public void SetLanguage(LanguageType languageType)
        {
            Assert.NotNull(languageType);// //如果 LanguageType 是枚举 enum 或普通值类型，
                                         // 那么它本身不可能为 null，这句断言实际上没有效果。

            if (LanguageResourceBasePath==null)
            {
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                var path = mergedDictionaries[0].Source.AbsolutePath;
                this.LanguageResourceBasePath = path.Remove(path.LastIndexOf("/"));
            }

            string languageUri = $"{LanguageResourceBasePath}/{languageType}.xaml";
            this.Resources=(ResourceDictionary)Application.LoadComponent(
                new Uri(languageUri,UriKind.Relative));

            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            Application.Current.Resources.MergedDictionaries.Insert(0, this.Resources);




            if (CurrentLanguageType!= languageType)
            {
                CurrentLanguageType = languageType;
                //todo 保存到系统设置里
            }

         
        }
    }
}

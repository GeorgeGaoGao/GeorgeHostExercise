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
        private ResourceDictionary LanguageResourceDictionary { get; set; }
        private string Uri { get; set;  }//语言字典的通用资源地址
        public LanguageManager()
        {
            SetLanguage(LanguageType.CN);
        }
        public string this[string key]
        {
            get
            {
                if (this.LanguageResourceDictionary != null && this.LanguageResourceDictionary.Contains(key))
                {
                    return LanguageResourceDictionary[key].ToString();
                }
                else
                {
                    return key;
                }
            }
        }

        public LanguageType CurrentLanguageType{ get; private set; }

        public void SetLanguage(LanguageType languageType)
        {
            Assert.NotNull(languageType);
            if (this.Uri==null)
            {
                ResourceDictionary resourceDictionary = Application.Current.Resources.
                    MergedDictionaries[0];//默认APP的资源字典第一个放置语言字典。
                string path = resourceDictionary.Source.AbsolutePath;
                this.Uri= path.Remove(path.LastIndexOf("/"));
            }
            string target = $"{Uri}/{languageType}.xaml";
            this.LanguageResourceDictionary=(ResourceDictionary)Application.LoadComponent(
                new Uri(this.Uri,UriKind.RelativeOrAbsolute));
            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            Application.Current.Resources.MergedDictionaries.Insert(0, LanguageResourceDictionary);

            if (CurrentLanguageType!= languageType)
            {
                CurrentLanguageType = languageType;
                //todo 保存到系统设置里
            }
        }
    }
}

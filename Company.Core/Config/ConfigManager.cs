using Company.Core.Helper;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Company.Core.Config
{
    [ExposedService(Lifetime.Singleton,typeof(IConfigManager))]
    public class ConfigManager : IConfigManager
    {
        private const string root = "Config";
        private string GetFullPath(ValueType key)
        {
            return Path.Combine(root,key.GetType().FullName+key.ToString()+".json");
        }
        public T Read<T>(ValueType key)
        {
         string fileName= GetFullPath(key);
            return JsonHelper.Load<T>(fileName);
        }

        public void Write<T>(ValueType key, T value)
        {
            Assert.NotNull(key);
            Assert.NotNull(value);
            Directory.CreateDirectory(root);
            string fileName=GetFullPath(key);
            JsonHelper.Save(value, fileName,true);
        }
    }
}

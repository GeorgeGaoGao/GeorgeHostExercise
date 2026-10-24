using Company.Core.Config;
using Company.Core.Extensions;
using Company.Core.Helper;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.Cache
{

    [ExposedService(Lifetime.Singleton, typeof(ICacheManager))]
    class CacheManager : ICacheManager
    {
        private IConfigManager _configManager;
        private Dictionary<string, string> _cacheNames;
        private readonly object cacheLock=new object();
        public CacheManager(IConfigManager configManager)
        {
            _configManager=configManager;
            _cacheNames = _configManager.Read<Dictionary<string, string>>(ConfigKey.CacheConfig) ?? new Dictionary<string, string>(); 
        }
       


        /// <summary>
        /// 获取本地缓存内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Get<T>(ValueType key, out T value)
        {
            value = default(T);
            string obj=key.ToString();
            lock (cacheLock)
            {
                if (!_cacheNames.ContainsKey(key.GetFullName())) return false;
                obj = _cacheNames[key.GetFullName()];

            }
            try
            {
                value=JsonHelper.Deserialize<T>(obj);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public void Set<T>(ValueType key, T value)
        {
            Assert.NotNull(key);
            Assert.NotNull(value);

            string content = JsonHelper.Serialize(value);
            lock (cacheLock)
            {
                if (_cacheNames.ContainsKey(key.GetFullName()))
                {
                    _cacheNames[key.GetFullName()] = content;
                }
                else
                {
                    _cacheNames.Add(key.GetFullName(), content);
                }
                _configManager.Write(ConfigKey.CacheConfig, _cacheNames);
            }
        }
        public void Delete(ValueType key)
        {
            lock (cacheLock)
            {
                if (_cacheNames.ContainsKey(key.GetFullName()))
                {
                    _cacheNames.Remove(key.GetFullName());
                    _configManager.Write(ConfigKey.CacheConfig, _cacheNames);
                }
            }
        }
    }
}

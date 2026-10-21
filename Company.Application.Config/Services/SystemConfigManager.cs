using Company.Application.Config.Models;
using Company.Core.Config;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Config.Services
{
    /// <summary>
    /// 系统参数管理者，作用是对系统参数进行读写。
    /// </summary>
    [ExposedService(Lifetime.Singleton)]
    public class SystemConfigManager
    {
        public SystemConfigModel Config { get; set; }
        private readonly IConfigManager _configManager;
        public SystemConfigManager(IConfigManager configManager)
        {
            _configManager = configManager;
            Load();
        }

        public void Load()
        {
            Config= _configManager.Read<SystemConfigModel>(ConfigKey.SystemConfig)??new SystemConfigModel();
        }
        public void Save()
        {
            _configManager.Write<SystemConfigModel>(ConfigKey.SystemConfig, Config);
        }
    }
}

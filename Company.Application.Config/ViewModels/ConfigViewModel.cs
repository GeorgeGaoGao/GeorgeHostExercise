using Company.Application.Config.Services;
using Company.Core.IOC;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Company.Application.Config.ViewModels
{
    public class ConfigViewModel :DialogViewModelBase
    {
        public SystemConfigManager SystemConfigManager { get; }
        public SystemConfigProvider SystemConfigProvider { get; }
        private DelegateCommand _okCommand;
        public DelegateCommand OKCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOKCommand));

        void ExecuteOKCommand()
        {
            SystemConfigManager.Save();//保存参数到本地JSON文件。
            SystemConfigProvider.Invoke();//通知硬件参数变化。
            CloseDialog(Prism.Dialogs.ButtonResult.OK);
        }

        public ConfigViewModel(SystemConfigManager systemConfigManager,SystemConfigProvider systemConfigProvider)
        {
            SystemConfigManager = systemConfigManager;
            SystemConfigProvider = systemConfigProvider;
        }
    }
}

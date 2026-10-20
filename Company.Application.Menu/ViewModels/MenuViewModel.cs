using Company.Application.Share.Prism;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Menu.ViewModels
{
    public class MenuViewModel:BindableBase
    {
        private DelegateCommand _systemConfigCommand;
        public DelegateCommand SystemConfigCommand =>
            _systemConfigCommand ?? (_systemConfigCommand = new DelegateCommand(ExecuteSystemConfigCommand));

        void ExecuteSystemConfigCommand()
        {
            MyPrismProvider.DialogService.ShowDialog(ViewNames.ConfigView,DialogCallback);
        }

        private void DialogCallback(IDialogResult dialogResult)
        {
            //todo
        }
    }
}

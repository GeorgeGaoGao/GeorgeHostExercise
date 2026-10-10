using Company.Application.Share.Events;
using Company.Application.Share.Model;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Main.ViewModels
{
    public class MainViewModel:BindableBase
    {
        private CurrentUser _userLogin;

        public CurrentUser UserLogin
        {
            get { return _userLogin; }
            set { _userLogin = value;RaisePropertyChanged(); }
        }

        public MainViewModel()
        {
            MyPrismProvider.EventAggregator.GetEvent<LoginSuccessEvent>().Subscribe(OnLoginResultReceived,ThreadOption.UIThread);
        }

        private void OnLoginResultReceived(CurrentUser user)
        {
            UserLogin = user;
        }
    }
}

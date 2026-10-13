using Company.Application.Share.Events;
using Company.Application.Share.Model;
using Company.Core.Dialog.Views;
using Company.Core.IOC;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Company.Application.Login.ViewModels
{

    public partial class LoginViewModel : ReactiveObject
    {
        [Reactive]
        public partial CurrentUser CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }
        public LoginViewModel()
        {

            LoginCommand = new DelegateCommand(OnLoginCommand);
            CurrentUser = new CurrentUser()
            {
                UserName = "Admin",
                Password = "123456"
            };
        }

        private void OnLoginCommand()
        {
            if (string.IsNullOrEmpty(CurrentUser.UserName) || string.IsNullOrEmpty(CurrentUser.Password))
            {
                return;
            }
            if (CurrentUser.UserName == "Admin" && CurrentUser.Password == "123456")
            {
                MyPrismProvider.EventAggregator.GetEvent<LoginSuccessEvent>().Publish(CurrentUser);
            }
            else
            {
                PopupBox.ShowPopupBox("用户名或密码错误", 3);
            }

        }
    }

}

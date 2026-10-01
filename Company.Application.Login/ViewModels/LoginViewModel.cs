using Company.Application.Share.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Company.Application.Login.ViewModels
{
    public class LoginViewModel:ReactiveObject
    {
        //[Reactive]
        public CurrentUser CurrentUser { get; set; } = new CurrentUser() { UserName = "george", Password = "123456" };
        public ICommand LoginCommand { get; set; }
        public LoginViewModel()
        {
            
            LoginCommand = new DelegateCommand(OnLoginCommand);
        }

        private void OnLoginCommand()
        {
            if (string.IsNullOrEmpty(CurrentUser.UserName)||string.IsNullOrEmpty(CurrentUser.Password))
            {
                return;
            }
        }
    }
   
}

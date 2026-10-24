using Company.Application.Share.Events;
using Company.Application.Share.Model;
using Company.Core.Cache;
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
        public bool IsRemember { get; set; }
        [Reactive]
        public bool IsAutoLogin { get; set; }

        [Reactive]
        public partial CurrentUser CurrentUser { get; set; } = new CurrentUser();
        private ICacheManager CacheManager { get; }
        public ICommand LoginCommand { get; set; }
        public LoginViewModel(ICacheManager cacheManager)
        {

            LoginCommand = new DelegateCommand(OnLoginCommand);
           
            CacheManager= cacheManager;
            LoadUserCache();

            //是否自动登录

            Task.Delay(1500).ContinueWith((t) =>
            {
                if (IsAutoLogin)
                {
                    LoginCommand.Execute(null);
                }
            });
        }

        private void LoadUserCache()
        {
            if (CacheManager.Get(CacheKey.User,out CurrentUser user))
            {
                CurrentUser.UserName = user.UserName;
                CurrentUser.Password = user.Password;
            }
            if (CacheManager.Get(CacheKey.IsRember,out bool isRember))
            {
                IsRemember = isRember;
            }
            if (CacheManager.Get(CacheKey.IsAutoLogin,out bool isAutoLogin))
            {
                IsAutoLogin = isAutoLogin;
            }
        }

        private void OnLoginCommand()
        {
            if (string.IsNullOrEmpty(CurrentUser.UserName) || string.IsNullOrEmpty(CurrentUser.Password))
            {
                return;
            }
            

            if (IsRemember)
            {
                CacheManager.Set(CacheKey.User, CurrentUser);
            }
            else
            {
                CacheManager.Delete(CacheKey.User);
            }

            CacheManager.Set(CacheKey.IsRember, IsRemember);
            CacheManager.Set(CacheKey.IsAutoLogin, IsAutoLogin);

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

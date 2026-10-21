using Company.Application.Initialize.Services;
using Company.Application.Share.Prism;
using Company.Core.IOC;
using Prism.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Company.Application.Initialize.ViewModels
{
    public class InitializeViewModel : BindableBase
    {
        public ICommand LoadedCommand { get; }
        public ICommand ChangeTitleCommand {  get; }
        
        
        public InitializeViewModel(HardwareLifetimeManager hardwareLifetimeManager)
        {
            HardwareLifetimeManager = hardwareLifetimeManager;
            LoadedCommand = new DelegateCommand(OnLoadedCommand);
            //LoadedCommand = ReactiveCommand.Create(OnLoadedCommand);
            Message = "构造函数里得到的值。";

            ChangeTitleCommand = new DelegateCommand(OnChangeTitleCommand);
        }

        private void OnChangeTitleCommand()
        {
            Title = "这是我调用eventtrigger改变后的title";
        }


        private string _title = "this is title";

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }


        private async void OnLoadedCommand()
        {

            if (!HardwareLifetimeManager.IsInitialized)
            {
                Message = "正在初始化所有硬件...";
                var result = await HardwareLifetimeManager.InitializeAsync();//等待
                if (!result.IsSuccessful)
                {
                    Message = result.Message;
                    return;
                }
            }

            Message = "硬件初始化完成";



            //加载主界面
            await Task.Delay(5000).ContinueWith(p =>
            {
                MyPrismProvider.Dispatcher.Invoke(() =>
                {
                    //加载主界面
                    MyPrismProvider.ModuleManager.LoadModule(ModuleNames.ApplicationMainModule);
                    //导航到主区域
                    MyPrismProvider.RegionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.MainView);
                });


            });


        }
        //[Reactive]
        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }


        public HardwareLifetimeManager HardwareLifetimeManager { get; set; }


    }
}

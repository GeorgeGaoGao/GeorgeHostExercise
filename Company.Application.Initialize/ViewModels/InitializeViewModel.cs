using Company.Application.Initialize.Services;
using Company.Application.Share.Prism;
using Company.Core.IOC;
using Prism.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace Company.Application.Initialize.ViewModels
{
    public class InitializeViewModel : ReactiveObject
    {
        public ICommand LoadedCommand { get; }
        public InitializeViewModel(HardwareLifetimeManager hardwareLifetimeManager)
        {
            HardwareLifetimeManager = hardwareLifetimeManager;
            //LoadedCommand = new DelegateCommand(OnLoadedCommand);
            LoadedCommand = ReactiveCommand.Create(OnLoadedCommand);
        }
        public string Title { get; set; } = "this is title";
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
        [Reactive]
        public string Message { get; set; }

        public HardwareLifetimeManager HardwareLifetimeManager { get; set; }


    }
}

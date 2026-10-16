using Company.Application.Share.Events;
using Company.Application.Share.Model;
using Company.Application.Share.Prism;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Shell.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private int _title = 9999;
        public ICommand LoadedCommand { get; }
        public int Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }
        public IModuleManager ModuleManager { get; }
        public IRegionManager RegionManager { get; }
        public MainWindowViewModel(IModuleManager moduleManager, IRegionManager regionManager)
        {
            ModuleManager = moduleManager;
            RegionManager = regionManager;
            LoadedCommand = new DelegateCommand(OnLoadedCommand);
        }

        private void OnLoadedCommand()
        {
            //订阅MainRegion导航事件
            var mainRegion = MyPrismProvider.RegionManager.Regions[RegionNames.MainRegion];
            mainRegion.NavigationService.Navigated += OnMainRegionNavigated;

            //加载LoginModule
            ModuleManager.LoadModule(ModuleNames.ApplicationLoginModule);
            //导航LoginView区域
            RegionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.LoginView);
            //订阅登录成功事件
            MyPrismProvider.EventAggregator.GetEvent<LoginSuccessEvent>().Subscribe(OnLoginResultReceived, ThreadOption.UIThread);



        }

        public bool IsHardwareInitialized { get; private set; }
        /// <summary>
        /// 收到登录成功事件的响应方法
        /// </summary>
        /// <param name="user"></param>

        private void OnLoginResultReceived(CurrentUser user)
        {
            
            if (IsHardwareInitialized)
            {
                //登录成功后，加载MainModule
                MyPrismProvider.ModuleManager.LoadModule(ModuleNames.ApplicationMainModule);
                //导航到MainView区域
                MyPrismProvider.RegionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.MainView);
            }
            else
            {
                //加载硬件初始化模块
                MyPrismProvider.ModuleManager.LoadModule(ModuleNames.ApplicationInitializeModule);
                MyPrismProvider.RegionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.InitializeView);
            }
        }

        private Window MainWindow { get; set; } = Application.Current.MainWindow;
        private void OnMainRegionNavigated(object? sender, RegionNavigationEventArgs e)
        {


            switch (e.Uri.OriginalString)
            {
                case ViewNames.LoginView:
                    MainWindow.Width = 800;
                    MainWindow.Height = 600;
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.ResizeMode = ResizeMode.NoResize;
                    MainWindow.SizeToContent = SizeToContent.Manual;
                    MainWindow.WindowStyle = WindowStyle.None;
                    break;

                case ViewNames.MainView:
                    MainWindow.Width = 1000;
                    MainWindow.Height = 800;
                    //MainWindow.WindowState = WindowState.Maximized;
                    MainWindow.ResizeMode = ResizeMode.CanResize;
                    MainWindow.SizeToContent = SizeToContent.Manual;
                    MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                    break;
               
                default:
                    break;
            }
        }
    }
}

using Company.Application.Share.Events;
using Company.Application.Share.Model;
using Company.Application.Share.Prism;
using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Shell.ViewModels
{
    public class MainWindowViewModel:BindableBase
    {
		private int _title=9999;
        public ICommand LoadedCommand { get; }
        public int Title
		{
			get { return _title; }
			set { _title = value;RaisePropertyChanged(); }
		}
        public IModuleManager ModuleManager { get; }
        public IRegionManager RegionManager { get;  }
        public MainWindowViewModel(IModuleManager moduleManager,IRegionManager regionManager)
        {
            ModuleManager = moduleManager;
            RegionManager = regionManager;
            LoadedCommand = new DelegateCommand(OnLoadedCommand);
        }

        private void OnLoadedCommand()
        {
            ModuleManager.LoadModule(ModuleNames.ApplicationLoginModule);
            //导航区域
            RegionManager.RequestNavigate(RegionNames.MainRegion,ViewNames.LoginView);

            MyPrismProvider.EventAggregator.GetEvent<LoginSuccessEvent>().Subscribe(OnLoginResultReceived,ThreadOption.UIThread);
        }

        private void OnLoginResultReceived(CurrentUser user)
        {
            MyPrismProvider.ModuleManager.LoadModule(ModuleNames.ApplicationMainModule);
            MyPrismProvider.RegionManager.RequestNavigate(RegionNames.MainRegion, ViewNames.MainView);
        }
    }
}

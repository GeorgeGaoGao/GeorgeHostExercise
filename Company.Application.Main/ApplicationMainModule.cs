using Company.Application.Main.Views;
using Company.Application.Share.Prism;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Main
{
    [Module(ModuleName = ModuleNames.ApplicationMainModule, OnDemand = true)]
    [ModuleDependency(ModuleNames.ApplicationMenuModule)]
    [ModuleDependency(ModuleNames.ApplicationImageModule)]//图像展示
    [ModuleDependency(ModuleNames.ApplicationProcessModule)]//图像处理,由下向上
    public class ApplicationMainModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRegionManager>().RegisterViewWithRegion<MainView>(RegionNames.MainRegion);
            containerProvider.Resolve<IRegionManager>().RegisterViewWithRegion<PrimaryView>(RegionNames.PrimaryRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView>();
        }
    }
}

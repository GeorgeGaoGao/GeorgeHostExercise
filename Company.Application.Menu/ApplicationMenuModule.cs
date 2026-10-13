using Company.Application.Menu.Views;
using Company.Application.Share.Prism;
using Company.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Menu
{
    [Module(ModuleName = ModuleNames.ApplicationMenuModule,OnDemand =true)]
    public class ApplicationMenuModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            containerProvider.Resolve<IRegionManager>().RegisterViewWithRegion<MenuView>(RegionNames.MenuRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        }
    }
}

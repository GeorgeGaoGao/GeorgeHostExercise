using Company.Application.Image.Views;
using Company.Application.Share.Prism;
using Company.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Image
{
    [Module(ModuleName = ModuleNames.ApplicationImageModule, OnDemand = true)]
    public class ApplicationImageModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
           containerProvider.InitializeTypesOfAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            containerProvider.Resolve<IRegionManager>().RegisterViewWithRegion<ImageView>(RegionNames.ImageRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        }
    }
}

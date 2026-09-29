using Company.Application.Login.Views;
using Company.Application.Share.Prism;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Login
{
    [Module(ModuleName =ModuleNames.ApplicationLoginModule,OnDemand =true)]
    public class ApplicationLoginModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();
        }
        
    }
}

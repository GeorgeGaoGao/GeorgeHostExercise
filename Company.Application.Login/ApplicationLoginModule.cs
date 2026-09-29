using Company.Application.Login.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Login
{
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

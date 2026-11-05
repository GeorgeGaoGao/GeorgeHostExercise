using Company.Application.Share.Prism;
using Company.Core.Extensions;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Company.Application.Process
{
    [Module(ModuleName=ModuleNames.ApplicationProcessModule,OnDemand =true)]
    public class ApplicationProcessModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(Assembly.GetExecutingAssembly());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
           containerRegistry.RegisterTypesOfAssembly(Assembly.GetExecutingAssembly());
        }
    }
}

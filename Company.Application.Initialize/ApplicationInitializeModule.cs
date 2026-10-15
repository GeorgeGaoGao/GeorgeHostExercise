using Company.Application.Share.Prism;
using Company.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Company.Application.Initialize
{
    [Module(ModuleName =ModuleNames.ApplicationInitializeModule,OnDemand =true)]
    public class ApplicationInitializeModule : IModule
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

using Company.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Company.Hardware.Camera.None
{
    public class HardwareCameraNoneModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(Assembly.GetExecutingAssembly());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(Assembly.GetExecutingAssembly());
             if (!containerRegistry.IsRegistered<ICamera>())
            {
                containerRegistry.RegisterSingleton<ICamera, CameraNone>();
            }
        }
    }
}

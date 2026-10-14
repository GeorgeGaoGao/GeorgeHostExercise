using Company.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Company.Hardware.Detector.None
{
    public class HardwareDetectorNoneModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(Assembly.GetExecutingAssembly());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(Assembly.GetExecutingAssembly());
            if (!containerRegistry.IsRegistered<IDetector>())
            {
                containerRegistry.RegisterSingleton<IDetector, DetectorNone>();
            }
        }
    }
}

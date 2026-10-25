using Company.Core.Extensions;
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;

namespace Company.Hardware.Camera.HIK
{
    public class HIKCameraModule : IModule
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
                containerRegistry.RegisterSingleton<ICamera, HIKCamera>();
            }
        }
    }
}
using Company.Core.Extensions;
using Company.Hardware.ControlCard.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;

namespace Company.Hardware.ControlCard
{
    public class ControlCardModule : IModule
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
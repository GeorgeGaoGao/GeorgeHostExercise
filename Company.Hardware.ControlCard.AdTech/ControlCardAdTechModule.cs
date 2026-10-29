using Company.Core.Extensions;
using Company.Hardware.ControlCard.AdTech.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;

namespace Company.Hardware.ControlCard.AdTech
{
    public class ControlCardAdTechModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(Assembly.GetExecutingAssembly());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(Assembly.GetExecutingAssembly());
            if (!containerRegistry.IsRegistered<IControlCard>())
            {
                containerRegistry.Register<IControlCard, ControlCardAdTech>();
            }
        }
    }
}
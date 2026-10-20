using Company.Application.Config.ViewModels;
using Company.Application.Config.Views;
using Company.Application.Share.Prism;
using Company.Core.Extensions;
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;

namespace Company.Application.Config
{
    [Module(ModuleName =ModuleNames.ApplicationConfigModule,OnDemand =true)]
    public class ApplicationConfigModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.InitializeTypesOfAssembly(Assembly.GetExecutingAssembly());
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterTypesOfAssembly(Assembly.GetExecutingAssembly());
            containerRegistry.RegisterDialog<ConfigView,ConfigViewModel>();
        }
    }
}
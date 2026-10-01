using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.IOC
{
    [ExposedService(Lifetime.Singleton,IsAutoInitialize =true)]
    public sealed class MyPrismProvider
    {
        public static IContainerExtension Container { get; private set; }//容器。相当于是IContainerRegistry IContainerProvider的合集。

        public static IRegionManager RegionManager { get; private set; }//区域管理器
        public static IDialogService DialogService { get; private set; }//对话框管理器
        public static IEventAggregator EventAggregator { get; private set; }//事件聚合器，也就是消息总线
        public static IModuleManager ModuleManager { get; private set; }//模块管理器
        public MyPrismProvider(
            IContainerExtension container,
            IRegionManager regionManager,
            IDialogService dialogService,
            IEventAggregator eventAggregator,
            IModuleManager moduleManager
            )
        {
            Container= container;
            RegionManager= regionManager;
            DialogService= dialogService;
            EventAggregator= eventAggregator;
            ModuleManager= moduleManager;
        }
    }
}

using Company.Core.Language;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Company.Core.IOC
{
    //它只接收一个 Lifetime 和一个 Type[]。而 [ExposedService(Lifetime.Singleton, IsAutoInitialize = true)] 中：
    //Lifetime.Singleton → 传给构造函数的 lifetime 参数。
    //IsAutoInitialize = true → 不是构造函数参数，而是给特性实例的 IsAutoInitialize 属性赋值。
    //types 没有传 → 默认为空数组。
    [ExposedService(Lifetime.Singleton, IsAutoInitialize = true)]
    public sealed class MyPrismProvider
    {
        public static ILanguageManager LanguageManager { get; private set; } //语言管理器
        public static IContainerExtension Container { get; private set; }//容器。相当于是IContainerRegistry IContainerProvider的合集。

        public static IRegionManager RegionManager { get; private set; }//区域管理器
        public static IDialogService DialogService { get; private set; }//对话框管理器
        public static IEventAggregator EventAggregator { get; private set; }//事件聚合器，也就是消息总线
        public static IModuleManager ModuleManager { get; private set; }//模块管理器
        public static Dispatcher Dispatcher { get; private set; } 
        public MyPrismProvider(
            ILanguageManager languageManager,
            IContainerExtension container,
            IRegionManager regionManager,
            IDialogService dialogService,
            IEventAggregator eventAggregator,
            IModuleManager moduleManager
            )
        {
            LanguageManager = languageManager;
            Container = container;
            RegionManager = regionManager;
            DialogService = dialogService;
            EventAggregator = eventAggregator;
            ModuleManager = moduleManager;
            Dispatcher = Application.Current.Dispatcher;
        }
    }
}

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

    //[ExposedService(Lifetime.Singleton, IsAutoInitialize = true)]
    //public sealed class MyPrismProvider
    //{
    //    private static readonly object Gate = new();
    //    private static bool _initialized;

    //    private static ILanguageManager? _languageManager;

    //    private static IContainerExtension _containerExtension;
    //    private static IRegionManager? _regionManager;
    //    private static IDialogService? _dialogService;
    //    private static IEventAggregator? _eventAggregator;
    //    private static IModuleManager? _moduleManager;
    //    private static Dispatcher? _dispatcher;

    //    public static ILanguageManager LanguageManager =>
    //        _languageManager ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");
    //    public static IContainerExtension Container =>
    //        _containerExtension ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");

    //    public static IRegionManager RegionManager =>
    //        _regionManager ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");

    //    public static IDialogService DialogService =>
    //        _dialogService ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");

    //    public static IEventAggregator EventAggregator =>
    //        _eventAggregator ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");

    //    public static IModuleManager ModuleManager =>
    //        _moduleManager ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");

    //    public static Dispatcher Dispatcher =>
    //        _dispatcher ?? throw new InvalidOperationException("MyPrismProvider 尚未初始化。");
    //    private MyPrismProvider() { }

    //    public static void Initialize(
    //        ILanguageManager languageManager,
    //        IContainerExtension containerExtension,
    //        IRegionManager regionManager,
    //        IDialogService dialogService,
    //        IEventAggregator eventAggregator,
    //        IModuleManager moduleManager)
    //    {
    //        lock (Gate)
    //        {
    //            if (_initialized)
    //                throw new InvalidOperationException("MyPrismProvider 已经初始化。");

    //            _languageManager = languageManager ?? throw new ArgumentNullException(nameof(languageManager));
    //            _containerExtension = containerExtension ?? throw new ArgumentNullException(nameof(containerExtension));
    //            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
    //            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    //            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
    //            _moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));
    //            _dispatcher = Application.Current.Dispatcher;

    //            _initialized = true;
    //        }
    //    }
    //}

}

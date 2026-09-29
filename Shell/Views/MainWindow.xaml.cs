using Company.Application.Share.Prism;
using MahApps.Metro.Controls;
using Shell.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Shell.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            vm.Title = 8888;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm=this.DataContext as MainWindowViewModel;
            //加载模块
            vm.ModuleManager.LoadModule(ModuleNames.ApplicationLoginModule);
            //导航区域
            vm.RegionManager.RequestNavigate(RegionNames.MainRegion,"LoginView");
        }
    } 
}

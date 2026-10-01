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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Company.Application.Login.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            // 方法1：Loaded 时打印（能捕捉到 AutoWire 之后的结果）
            this.Loaded += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[LoginView] DataContext Type = {this.DataContext?.GetType().FullName ?? "NULL"}");
            };

            // 方法2：DataContext 变化时打印（能捕捉到任何赋值）
            this.DataContextChanged += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[LoginView] DataContextChanged → {e.NewValue?.GetType().FullName ?? "NULL"}");
            };
        }
    }
}

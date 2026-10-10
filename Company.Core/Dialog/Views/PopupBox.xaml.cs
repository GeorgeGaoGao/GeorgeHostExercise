using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Company.Core.Dialog.Views
{
    /// <summary>
    /// PopupBox.xaml 的交互逻辑
    /// </summary>
    public partial class PopupBox : Popup
    {
        public PopupBox()
        {
            InitializeComponent();
        }



        /// <summary>
        /// 透明度
        /// </summary>

        public double PopupBoxOpacity
        {
            get { return (double)GetValue(PopupBoxOpacityProperty); }
            set { SetValue(PopupBoxOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupBoxOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupBoxOpacityProperty =
            DependencyProperty.Register(nameof(PopupBoxOpacity), typeof(double), typeof(PopupBox),
                new PropertyMetadata(1.0, new PropertyChangedCallback(OnPopupBoxOpacityPropertyChanged)));

        private static void OnPopupBoxOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PopupBox popupBox)) return;
            if (!(e.NewValue is double newOpacity)) return;

            popupBox.border.Opacity = newOpacity;//把弹窗的opacity变化反应到border的opacity上
        }


        /// <summary>
        ///弹出窗口的消息内容。
        /// </summary>

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(PopupBox), new PropertyMetadata(string.Empty));

        private static PopupBox _dialog = new PopupBox();
        private static DispatcherTimer _fadeTimer = new DispatcherTimer();

        /// <summary>
        /// 展示弹窗
        /// </summary>
        /// <param name="message">弹窗显示内容</param>
        /// <param name="owner">所属窗体</param>
        /// <param name="seconds">消失时间</param>
        public static void ShowPopupBox(string message,int seconds, Window owner = null )
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                           {
                               if (owner == null) { owner = Application.Current.MainWindow; }

                               _dialog.Message = message;
                               _dialog.PlacementTarget = owner;
                               _dialog.Placement = PlacementMode.Center;
                               _dialog.VerticalOffset = owner.ActualHeight / 3;
                               _dialog.AllowsTransparency = true;
                               _dialog.StaysOpen = true;
                               _dialog.PopupBoxOpacity = 1.0;
                               _dialog.IsOpen = true;

                               _fadeTimer.Tick -= _fadeTimer_Tick;
                               _fadeTimer.Tick += _fadeTimer_Tick;
                               _fadeTimer.Interval = TimeSpan.FromSeconds(seconds);
                               _fadeTimer.Start();
                           }));
            }
            catch (Exception)
            {

                throw;
            }

        }

        private static void _fadeTimer_Tick(object? sender, EventArgs e)
        {
            _fadeTimer.Stop();
            Task.Run(async () =>
            {
                try
                {
                    for (int i = 0; i < 100; i++)
                    {
                        //Thread.Sleep(10);
                        await Task.Delay(10);
                        if (Application.Current == null) return;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialog.PopupBoxOpacity -= 0.01;
                        });
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialog.IsOpen = false;
                        _dialog.Message = string.Empty;
                    });
                }
                catch (Exception)
                {

                    throw;
                }
            });
        }
    }
}

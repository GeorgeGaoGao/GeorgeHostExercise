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

namespace Company.UI.Controls
{
    /// <summary>
    /// WpfPropertyGrid.xaml 的交互逻辑
    /// </summary>
    public partial class WpfPropertyGrid : System.Windows.Controls.UserControl
    {
        public WpfPropertyGrid()
        {
            InitializeComponent();
        }



        public object SelectedObject
        {
            get { return (object)GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(WpfPropertyGrid), new FrameworkPropertyMetadata(SelectedObjectPropertyChangedCallback));

        private static void SelectedObjectPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WpfPropertyGrid wpfPropertyGrid)
            {
                wpfPropertyGrid.propertyGrid.SelectedObject = (object)e.NewValue;
            }
        }
    }
}

using Company.Core.IOC;
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
            this.Loaded += (s, e) =>
            {
                CN.Checked += LanguageChecked;
                US.Checked += LanguageChecked;
                TW.Checked += LanguageChecked;
            };
        }

        private void LanguageChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                switch (radioButton.Name)
                {
                    case "CN": MyPrismProvider.LanguageManager.SetLanguage(Core.Enums.LanguageType.CN); break;
                    case "US": MyPrismProvider.LanguageManager.SetLanguage(Core.Enums.LanguageType.US); break;
                    case "TW": MyPrismProvider.LanguageManager.SetLanguage(Core.Enums.LanguageType.TW); break;
                    default: MyPrismProvider.LanguageManager.SetLanguage(Core.Enums.LanguageType.CN); break;
                }
            }
        }

    }
}

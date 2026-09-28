using System;
using System.Collections.Generic;
using System.Text;

namespace Shell.ViewModels
{
    public class MainWindowViewModel:BindableBase
    {
		private int _title=9999;

		public int Title
		{
			get { return _title; }
			set { _title = value;RaisePropertyChanged(); }
		}

	}
}

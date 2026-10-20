using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.IOC
{
    public class DialogViewModelBase : ReactiveObject, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
            
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            
        }

        public virtual void CloseDialog(ButtonResult buttonResult,IDialogParameters parameters = null)
        {
            RequestClose.Invoke(parameters,buttonResult);
        }
    }
}

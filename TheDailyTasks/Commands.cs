using System;
using System.Windows.Input;

namespace TheDailyTasks
{
    class CompletedButtonEvent : ICommand
    {
        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //throw new NotImplementedException();
            App.DataModel.CompleteTask((Tasks)parameter);
        }
    }

    class ShowDetailButtonEvent : ICommand
    {
        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            return true;
        }   

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //throw new NotImplementedException();
            App.DataModel.IsAdditionalPanelDisplayed = true;
            App.DataModel.AdditionalPanelContent = typeof(ItemDetail).ToString();
            App.DataModel.ShowDetail((Tasks)parameter);
        }
    }

    class DeleteButtonEvent : ICommand
    {
        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //throw new NotImplementedException();
            App.DataModel.Delete((Tasks)parameter);
        }
    }

    class EditButtonEvent : ICommand
    {
        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //throw new NotImplementedException();
            App.DataModel.IsAdditionalPanelDisplayed = true;
            App.DataModel.AdditionalPanelContent = typeof(Add).ToString();
            App.DataModel.Edit((Tasks)parameter);
        }
    }
}

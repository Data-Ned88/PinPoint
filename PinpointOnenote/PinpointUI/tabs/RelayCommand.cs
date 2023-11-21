using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinpointUI.tabs
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute; //function call
        private Func<object, bool> canExecute; //returns a bool (determines if the action can be done.


        public event EventHandler CanExecuteChanged 
        { 
            //needed to manage the system memory by adding and unhooking the events when appropriate
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            //THis is the contructor: it makes the RelayCOmmand object (when assigned to a vriable by its calling code ...
            // ... using the function call and function evaluation bool private proeprties above.
            this.execute = execute;
            this.canExecute = canExecute;
        }
        public bool CanExecute(object parameter) //given to you as blank on Implelemt interface
        {
            return canExecute == null || canExecute(parameter);
            //first statement above returns a true if there is no execution criteria (ie it's a free pass)
            //second statement evaluates the private Func canExecute against the input parameter to get a true/false.
        }

        public void Execute(object parameter) //given to you as blank on Implelemt interface
        {
            execute(parameter); //calls the private Action execute;
        }
    }
}

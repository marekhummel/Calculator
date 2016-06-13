using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator
{
	public class RelayCommand : ICommand
	{

		// Fields
		private readonly Func<bool> _canExecute;
		private readonly Action<object> _execute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}



		// Constructors
		public RelayCommand(Action<object> execute) : this(execute, () => true)
		{
		}

		public RelayCommand(Action<object> execute, Func<bool> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}



		// Interface implementation
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute.Invoke();
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

	}
}

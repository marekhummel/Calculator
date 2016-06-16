using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Calculator.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{

		public MainViewModel()
		{
			CurrentPage = ButtonPage.Unary;
			Expression = "";
			Result = "0";


			ChangeToUnaryPage = new RelayCommand(obj => CurrentPage = ButtonPage.Unary);
			ChangeToPolyadicPage = new RelayCommand(obj => CurrentPage = ButtonPage.Polyadic);
			ChangeToDegreeUnit = new RelayCommand(obj => UsedAngleUnit = ExpressionParser.AngleUnit.Degrees);
			ChangeToRadiansUnit = new RelayCommand(obj => UsedAngleUnit = ExpressionParser.AngleUnit.Radians);
			ChangeToGradUnit = new RelayCommand(obj => UsedAngleUnit = ExpressionParser.AngleUnit.Grad);

			InsertEntry = new RelayCommand(obj => Expression += obj.ToString());
			RemoveLastCharacter = new RelayCommand(obj => Expression = Expression.Substring(0, Expression.Length - 1), () => Expression != "");
			Clear = new RelayCommand(obj =>
			{
				Expression = "";
				Result = "0";
			});
			Evaluate = new RelayCommand(obj => Result = ExpressionParser.Evaluate(Expression).ToString());

		}


		public enum ButtonPage
		{
			Unary,
			Polyadic
		}



		// Properties
		private ButtonPage _currentPage;
		public ButtonPage CurrentPage
		{
			get
			{
				return _currentPage;
			}
			set
			{
				_currentPage = value;
				OnPropertyChanged(nameof(IsUnaryPageActivated));
				OnPropertyChanged(nameof(IsPolyadicPageActivated));
			}
		}

		public ExpressionParser.AngleUnit UsedAngleUnit
		{
			get { return ExpressionParser.UsedAngleUnit; }
			set
			{
				ExpressionParser.UsedAngleUnit = value;
				OnPropertyChanged(nameof(IsUnitDegreesUsed));
				OnPropertyChanged(nameof(IsUnitRadiansUsed));
				OnPropertyChanged(nameof(IsUnitGradUsed));
			}
		}

		public bool IsUnaryPageActivated => _currentPage == ButtonPage.Unary;
		public bool IsPolyadicPageActivated => _currentPage == ButtonPage.Polyadic;

		public bool IsUnitDegreesUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Degrees;
		public bool IsUnitRadiansUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Radians;
		public bool IsUnitGradUsed => UsedAngleUnit == ExpressionParser.AngleUnit.Grad;


		private string _expression;
		public string Expression
		{
			get { return _expression; }
			set
			{
				_expression = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RemoveLastCharacter));
				OnPropertyChanged(nameof(Clear));
			}
		}

		private string _result;
		public string Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
				OnPropertyChanged();
			}
		}



		//Commands 

		public ICommand ChangeToUnaryPage { get; private set; }
		public ICommand ChangeToPolyadicPage { get; private set; }

		public ICommand ChangeToDegreeUnit { get; private set; }
		public ICommand ChangeToRadiansUnit { get; private set; }
		public ICommand ChangeToGradUnit { get; private set; }

		public ICommand InsertEntry { get; private set; }
		public ICommand RemoveLastCharacter { get; private set; }
		public ICommand Clear { get; private set; }
		public ICommand Evaluate { get; private set; }


		// INotifyPropertyChanged support

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

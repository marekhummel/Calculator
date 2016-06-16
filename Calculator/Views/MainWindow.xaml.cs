using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Calculator.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			ExpressionBox.KeyDown += ExpressionBoxOnKeyDown;
			ExpressionBox.LostFocus += (sender, args) => FocusManager.SetFocusedElement(outerGrid, ExpressionBox);
		}

		private void ExpressionBoxOnKeyDown(object sender, KeyEventArgs e)
		{ 
			e.Handled = true;

			var nums = new[] {Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9};
			var numpad = new[] { Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };
			var chars = new[] {Key.OemPlus, Key.OemMinus, Key.Oem5, Key.OemComma, Key.OemPeriod};
			var specials = new[] {Key.Space, Key.Back, Key.Return};
	
			if (new[] {nums, numpad, chars, specials}.SelectMany(key => key).Contains(e.Key))
				e.Handled = false;


			var shifted = new[] { Key.OemPlus, Key.D8, Key.D9, Key.D5, Key.D7, Key.E };
			if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && shifted.Contains(e.Key))
				e.Handled = false;
		}
	}
}

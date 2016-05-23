using System.Collections.Generic;
using System.Windows;


namespace Calculator
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void btnConvert_Click(object sender, RoutedEventArgs e)
		{
			var infix = tbInfix.Text;

			var tokens = ExpressionParser.Tokenize(infix);
			lbTokens.Items.Clear();
			tokens.ForEach(t => lbTokens.Items.Add(t.ToString()));

			List<Token> postfix;
			var success = ExpressionParser.ShuntingYardAlgorithm(tokens, out postfix);

			if (!success)
				return;

			tbPostfix.Text = string.Join(" ", postfix);
		}
	}
}
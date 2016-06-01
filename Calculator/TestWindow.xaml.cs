using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;


namespace Calculator
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class TestWindow : Window
	{
		public TestWindow()
		{
			InitializeComponent();
		}

		private void btnConvert_Click(object sender, RoutedEventArgs e)
		{
			var infix = tbInfix.Text;

			//Tokenize
			var tokens = ExpressionParser.Tokenize(infix);
			lbTokens.Items.Clear();
			tokens.ForEach(t => lbTokens.Items.Add(t.ToString()));

			//SYA
			List<Token> postfix;
			var success = ExpressionParser.ShuntingYardAlgorithm(tokens, out postfix);

			if (!success)
				return;

			tbPostfix.Text = string.Join(" ", postfix); 

			//Eval
			var result = ExpressionParser.Evaluate(infix);
			tbResult.Text = result.ToString(CultureInfo.InvariantCulture);

			//ExpTree
			var item = ExpressionParser.CreateExpressionTree(infix);
			tvExptree.Items.Clear();
			tvExptree.Items.Add(item);
		}

		private void tbInfix_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (tbPostfix == null)
				return;

			tbPostfix.Text = "";
			tbResult.Text = "";
			lbTokens.Items.Clear();
			tvExptree.Items.Clear();
		}
	}
}
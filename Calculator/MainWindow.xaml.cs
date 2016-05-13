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
			//var postfix = ExpressionParser.InfixToPostfix(tokens);

			tokens.ForEach(t => lboxTokens.Items.Add(t));
			//tbPostfix.Text = string.Join(" ", postfix);
		}
	}
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
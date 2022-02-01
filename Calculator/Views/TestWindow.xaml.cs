using System.Globalization;
using System.Windows;


namespace Calculator.Views
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

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            string? infix = tbInfix.Text;

            //Tokenize
            var tokens = ExpressionParser.Tokenize(infix);
            lbTokens.Items.Clear();
            tokens.ForEach(t => lbTokens.Items.Add(t.ToString()));

            //SYA
            var success = ExpressionParser.ShuntingYardAlgorithm(tokens, out var postfix);

            if (success != EvalResult.ErrorType.Success) {
                return;
            }

            tbPostfix.Text = string.Join(" ", postfix);

            //Eval
            var result = ExpressionParser.Evaluate(infix);
            tbResult.Text = result.Value.ToString(CultureInfo.InvariantCulture);

            //ExpTree
            var item = ExpressionParser.CreateExpressionTree(infix);
            tvExptree.Items.Clear();
            _ = tvExptree.Items.Add(item);
        }

        private void TbInfix_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbPostfix == null) {
                return;
            }

            tbPostfix.Text = "";
            tbResult.Text = "";
            lbTokens.Items.Clear();
            tvExptree.Items.Clear();
        }
    }
}
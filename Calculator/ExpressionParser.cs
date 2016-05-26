using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculator
{
	static class ExpressionParser
	{

		//Define the operations
		static ExpressionParser()
		{
			//Preset some operations
			Func<double, int?> tryParse = d =>
			{
				int i;
				if (!int.TryParse(d.ToString(CultureInfo.InvariantCulture), out i))
					return null;
				return i;
			};

			Func<double, double, double> gcd = null;
			gcd = (a, b) =>
			{
				var i = tryParse(a);
				var j = tryParse(b);
				if (i == null || j == null)
					return double.NaN;

				return (j == 0) ? (double)i : gcd((double)j, (double)i % (double)j);
			};

			Func<double, double, double> lcm = (a, b) =>
			{
				var i = tryParse(a);
				var j = tryParse(b);
				if (i == null || j == null)
					return double.NaN;

				return ((double)i * (double)j) / gcd((double)i, (double)j);
			};

			Func<double, double> fact = a =>
			{
				var i = tryParse(a);
				return i == null ? double.NaN : Enumerable.Range(1, (int)i).Aggregate(1, (x, y) => x * y);
			};




			//Initialze the _operations dictionaries
			_binaryOperations = new Dictionary<string, Func<double, double, double>>();
			_binaryOperations.Add("+", (a, b) => a + b);
			_binaryOperations.Add("-", (a, b) => a - b);
			_binaryOperations.Add("*", (a, b) => a * b);
			_binaryOperations.Add("/", (a, b) => (b == 0d ? double.NaN : a / b));
			_binaryOperations.Add("%", (a, b) => a % b);
			_binaryOperations.Add("^", Math.Pow);
			_binaryOperations.Add("log(", (a, b) => Math.Log(b, a));

			_unaryOperations = new Dictionary<string, Func<double, double>>();
			_unaryOperations.Add("!", a => -a);
			_unaryOperations.Add("sin(", Math.Sin);
			_unaryOperations.Add("cos(", Math.Cos);
			_unaryOperations.Add("tan(", Math.Tan);
			_unaryOperations.Add("asin(", Math.Asin);
			_unaryOperations.Add("acos(", Math.Acos);
			_unaryOperations.Add("atan(", Math.Atan);
			_unaryOperations.Add("abs(", Math.Abs);
			_unaryOperations.Add("sqrt(", Math.Sqrt);
			_unaryOperations.Add("ln(", Math.Log);
			_unaryOperations.Add("exp(", Math.Exp);
			_unaryOperations.Add("fact(", fact);

			_polyadicFunctions = new Dictionary<string, Func<IEnumerable<double>, double>>();
			_polyadicFunctions.Add("gcd(", ops => ops.Aggregate((a, b) => gcd(a, b)));
			_polyadicFunctions.Add("lcm(", ops => ops.Aggregate((a, b) => lcm(a, b)));
			_polyadicFunctions.Add("max(", ops => ops.Aggregate(Math.Max));
			_polyadicFunctions.Add("min(", ops => ops.Aggregate(Math.Min));

			_constants = new Dictionary<string, double>();
			_constants.Add("pi", Math.PI);
			_constants.Add("e", Math.E);
		}

		private static readonly Dictionary<string, Func<double, double, double>> _binaryOperations;
		private static readonly Dictionary<string, Func<double, double>> _unaryOperations;
		private static readonly Dictionary<string, Func<IEnumerable<double>, double>> _polyadicFunctions;
		private static readonly Dictionary<string, double> _constants;




		//Tokenize of the infix string
		public static List<Token> Tokenize(string infix)
		{
			//Define the pattern
			const string patternNums = @"\d*\.?\d+";
			var patternOps = string.Join("|", Token.ValidTokens.Select(Regex.Escape));
			var pattern = patternNums + "|" + patternOps;

			//Find matches
			var matches = Regex.Matches(infix, pattern, RegexOptions.IgnoreCase);

			//Convert the matches to tokens
			var tokens = matches.Cast<Match>().Select(m => new Token(m.ToString())).ToList();




			//Set arities for polyadic functions
			foreach (var tk in tokens.Where(t => Token.PolyadicFunctions.Contains(t.Content)))
			{
				var index = tokens.FindIndex(t => t == tk);

				var openparenthesis = 1;
				var argsCount = 0;
				foreach (var t in tokens.Skip(index + 1))
				{
					switch (t.Type)
					{
						case Token.TokenType.Function:
							openparenthesis++;
							break;
						case Token.TokenType.RightParathesis:
							openparenthesis--;
							break;
					}

					if ((t.Type == Token.TokenType.Number || t.Type == Token.TokenType.RightParathesis) && openparenthesis == 1)
						argsCount++;

					if (openparenthesis == 0)
						break;
				}

				tokens[index].Arity = argsCount;
			}



			//Special handling of implicit multiply operator and unary minus operator
			var pairs = tokens.Take(tokens.Count() - 1).Select((tk, i) => new[] { tk, tokens[i + 1] }).ToList();

			//Insert implicit multiply operators	
			var pindex = 1;
			foreach (var p in pairs)
			{
				Token left = p[0], right = p[1];

				if (left.Type == Token.TokenType.Number && right.Type == Token.TokenType.Constant)
					tokens.Insert(pindex, new Token("*"));
				else if (left.Type == Token.TokenType.RightParathesis && right.Type == Token.TokenType.LeftParenthesis)
					tokens.Insert(pindex, new Token("*"));

				pindex++;
			}

			//Detect unary minus
			pindex = 0;
			foreach (var p in pairs)
			{
				Token left = p[0], right = p[1];

				if (pindex == 0 && left.Content == "-")
					tokens[0] = new Token("!");
				else if (left.Type == Token.TokenType.Operator && right.Type == Token.TokenType.Operator)
				{
					if (right.Content == "-")
						tokens[pindex + 1] = new Token("!");
				}
				else if (left.Type == Token.TokenType.Operator && right.Type == Token.TokenType.LeftParenthesis)
				{
					if (left.Content == "-")
						tokens[pindex] = new Token("!");
				}



				pindex++;
			}

			return tokens;
		}



		//Infix to postfix
		public static bool ShuntingYardAlgorithm(List<Token> infix, out List<Token> postfix)
		{
			postfix = null;

			var tokens = new Queue<Token>(infix);
			var stack = new Stack<Token>();
			var res = new List<Token>();

			while (tokens.Any())
			{
				var curr = tokens.Dequeue();

				switch (curr.Type)
				{
					case Token.TokenType.Number:
					case Token.TokenType.Constant:
						res.Add(curr);
						break;


					case Token.TokenType.Function:
						stack.Push(curr);
						break;

					case Token.TokenType.ArgumentSeperator:
						while (true)
						{
							//Wrongly positioned seperator or missing left parenthesis
							if (!stack.Any())
								return false;

							var top = stack.Peek();

							if (top.Type == Token.TokenType.Function)
								break;

							res.Add(stack.Pop());
						}
						break;

					case Token.TokenType.Operator:

						if (curr.Associativity == Token.AssociativityType.Left)
						{
							while (stack.Any())
							{
								var top = stack.Peek();
								if (top.Type == Token.TokenType.Operator && curr.Precedence <= top.Precedence)
									res.Add(stack.Pop());
								else
									break;
							}
						}
						stack.Push(curr);
						break;

					case Token.TokenType.LeftParenthesis:
						stack.Push(curr);
						break;

					case Token.TokenType.RightParathesis:
						while (true)
						{
							//Missing left parenthesis
							if (!stack.Any())
								return false;

							var top = stack.Pop();

							if (top.Type == Token.TokenType.LeftParenthesis)
								break;

							if (top.Type == Token.TokenType.Function)
							{
								res.Add(top);
								break;
							}

							res.Add(top);
						}
						break;
				}
			}

			//Flush the stack to the result
			while (stack.Count > 0)
			{
				var top = stack.Pop();

				//More left parentheses than right ones
				if (top.Type == Token.TokenType.LeftParenthesis || top.Type == Token.TokenType.Function)
					return false;

				res.Add(top);
			}



			postfix = res;
			return true;
		}



		//Evaluation of a infix string
		public static double Evaluate(string infix)
		{
			//Tokenize the string
			var tokens = Tokenize(infix);

			//Transform to postfix notation
			List<Token> postfix;
			var success = ShuntingYardAlgorithm(tokens, out postfix);

			//Possible invalid infix
			return success ? Evaluate(postfix) : double.NaN;
		}

		//Intern evaluation method
		private static double Evaluate(IEnumerable<Token> postfix)
		{
			var tokens = new Queue<Token>(postfix);
			var result = new Stack<double>();

			//Iterate through all tokens
			while (tokens.Any())
			{
				//Get the current token
				var curr = tokens.Dequeue();

				//Act based on the token type
				double value;
				switch (curr.Type)
				{
					case Token.TokenType.Number:
						result.Push(double.Parse(curr.Content, CultureInfo.InvariantCulture));
						break;

					case Token.TokenType.Constant:
						result.Push(_constants[curr.Content]);
						break;

					case Token.TokenType.Operator:

						if (result.Count() < curr.Arity)
							return double.NaN;

						//Calculate
						value = 0d;
						switch (curr.Arity)
						{
							case 2:
								var right = result.Pop();
								var left = result.Pop();
								value = _binaryOperations[curr.Content].Invoke(left, right);
								break;
							case 1:
								var operand = result.Pop();
								value = _unaryOperations[curr.Content].Invoke(operand);
								break;
						}

						//Push
						result.Push(value);
						break;

					case Token.TokenType.Function:
						var operands = new Stack<double>();

						//Check whether the necessary operands for this operations are given
						if (result.Count() < curr.Arity)
							return double.NaN;

						//Get the operands
						for (var i = 0; i < curr.Arity; i++)
							operands.Push(result.Pop());

						//Calculate
						value = 0d;
						switch (curr.Arity)
						{
							case 2:
								if (Token.PolyadicFunctions.Contains(curr.Content))
									value = _polyadicFunctions[curr.Content].Invoke(operands);
								else
									value = _binaryOperations[curr.Content].Invoke(operands.Pop(), operands.Pop());
								break;
							case 1:
								value = _unaryOperations[curr.Content].Invoke(operands.Pop());
								break;
							default:
								value = _polyadicFunctions[curr.Content].Invoke(operands);
								break;
						}

						//Push
						result.Push(value);
						break;

					default:
						//Invalid token, postfix notation doesn't support parenthesis nor argument seperators
						return double.NaN;
				}
			}

			//Result stack has to be reduced to one token
			return result.Count() == 1 ? Math.Round(result.Pop(), 10) : double.NaN;
		}





		//Creation of a expression tree based on a infix string
		public static TreeViewItem CreateExpressionTree(string infix)
		{
			//Tokenize the string
			var tokens = Tokenize(infix);

			//Transform to postfix notation
			List<Token> postfix;
			var success = ShuntingYardAlgorithm(tokens, out postfix);

			//Possible invalid infix
			return success ? CreateExpressionTree(postfix) : null;
		}

		//Intern exptree method
		private static TreeViewItem CreateExpressionTree(IEnumerable<Token> postfix)
		{
			var tokens = new Queue<Token>(postfix);
			var result = new Stack<TreeViewItem>();

			//Iterate through all tokens
			while (tokens.Any())
			{
				//Get the current token
				var curr = tokens.Dequeue();

				//Act based on the token type
				var newitem = new TreeViewItem { Header = curr.Content, IsExpanded = true };
				switch (curr.Type)
				{
					case Token.TokenType.Number:
					case Token.TokenType.Constant:
						result.Push(newitem);
						break;

					case Token.TokenType.Operator:

						if (result.Count() < curr.Arity)
							return null;

						//Add children
						switch (curr.Arity)
						{
							case 2:
								newitem.Items.Add(result.Pop());
								newitem.Items.Insert(0, result.Pop());
								result.Push(newitem);

								break;
							case 1:
								newitem.Items.Add(result.Pop());
								result.Push(newitem);

								break;
						}

						break;

					case Token.TokenType.Function:
						var operands = new Stack<TreeViewItem>();

						//Check whether the necessary operands for this operations are given
						if (result.Count() < curr.Arity)
							return null;

						//Get the operands
						for (var i = 0; i < curr.Arity; i++)
							operands.Push(result.Pop());

						//Add the children
						switch (curr.Arity)
						{
							case 2:
								if (Token.PolyadicFunctions.Contains(curr.Content))
									operands.ToList().ForEach(o => newitem.Items.Add(o));
								else
								{
									newitem.Items.Add(operands.Pop());
									newitem.Items.Add(operands.Pop());
								}
								break;
							case 1:
								newitem.Items.Add(operands.Pop());
								break;
							default:
								operands.ToList().ForEach(o => newitem.Items.Add(o));
								break;
						}

						//Push
						result.Push(newitem);
						break;

					default:
						//Invalid token, postfix notation doesn't support parenthesis nor argument seperators
						return null;
				}
			}

			//Result stack has to be reduced to one token
			return result.Count() == 1 ? result.Pop() : null;

		}

	}
}

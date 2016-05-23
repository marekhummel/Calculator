using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Calculator
{
	static class ExpressionParser
	{

		static ExpressionParser()
		{
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
			_unaryOperations.Add("-", a => -a);
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
			_unaryOperations.Add("fact(", a =>
			{
				int inta;
				if(!int.TryParse(a.ToString(), out inta))
					return double.NaN;

				return Enumerable.Range(1, inta).Aggregate(1, (x, y) => x * y);
			});
		}

			
		public static List<Token> Tokenize(string infix)
		{
			//Define the pattern
			var patternNums = @"\d*\.?\d+";
			var patternOps = string.Join("|", Token.ValidTokens.Select(Regex.Escape));
			var pattern = patternNums + "|" + patternOps;

			//Find matches
			var matches = Regex.Matches(infix, pattern);

			//Return each match as token
			return matches.Cast<Match>().Select(m => new Token(m.ToString())).ToList();
		}


		public static bool ShuntingYardAlgorithm(List<Token> infix, out List<Token> postfix)
		{
			postfix = null;

			var tokens = new Queue<Token>(infix);
			var stack = new Stack<Token>();
			var res = new List<Token>();

			while(tokens.Any())
			{
				var curr = tokens.Dequeue();

				switch(curr.Type)
				{
					case Token.TokenType.Number:
						res.Add(curr);
						break;

					case Token.TokenType.Function:
						stack.Push(curr);
						break;

					case Token.TokenType.ArgumentSeperator:
						while(true)
						{
							//Wrongly positioned seperator or missing left parenthesis
							if(!stack.Any())
								return false;

							var top = stack.Peek();

							if(top.Type == Token.TokenType.Function)
								break;

							res.Add(stack.Pop());
						}
						break;

					case Token.TokenType.Operator:

						if(curr.Associativity == Token.AssociativityType.Left)
						{
							while(stack.Any())
							{
								var top = stack.Peek();
								if(top.Type == Token.TokenType.Operator && curr.Precedence <= top.Precedence)
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
						while(true)
						{
							//Missing left parenthesis
							if(!stack.Any())
								return false;

							var top = stack.Pop();

							if(top.Type == Token.TokenType.LeftParenthesis)
								break;

							if(top.Type == Token.TokenType.Function)
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
			while(stack.Count > 0)
			{
				var top = stack.Pop();

				//More left parentheses than right ones
				if(top.Type == Token.TokenType.LeftParenthesis || top.Type == Token.TokenType.Function)
					return false;

				res.Add(top);
			}



			postfix = res;
			return true;
		}


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



		private static readonly Dictionary<string, Func<double, double, double>> _binaryOperations;
		private static readonly Dictionary<string, Func<double, double>> _unaryOperations;

		private static double Evaluate(IEnumerable<Token> postfix)
		{
			var tokens = new Queue<Token>(postfix);
			var result = new Stack<double>();

			//Iterate through all tokens
			while(tokens.Any())
			{
				//Get the current token
				var curr = tokens.Dequeue();

				//Act based on the token type
				double value;
				switch(curr.Type)
				{
					case Token.TokenType.Number:
						result.Push(double.Parse(curr.Content, CultureInfo.InvariantCulture));
						break;

					case Token.TokenType.Operator:

						if(result.Count() < curr.Arity)
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
						if(result.Count() < curr.Arity)
							return double.NaN;

						//Get the operands
						for(var i = 0; i < curr.Arity; i++)
							operands.Push(result.Pop());

						//Calculate
						value = 0d;
						switch(curr.Arity)
						{
							case 2:
								value = _binaryOperations[curr.Content].Invoke(operands.Pop(), operands.Pop());
								break;
							case 1:
								value = _unaryOperations[curr.Content].Invoke(operands.Pop());
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

			//Result stack has to be reduced to one token of type number
			return result.Count() == 1 ? result.Pop() : double.NaN;
		}


	}
}

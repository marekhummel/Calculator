using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
	static class ExpressionParser
	{




		public static List<Token> Tokenize(string infix)
		{
			var patternNums = @"\d*\.?\d+";
			var patternOps = "[" + string.Join("", ops) + "]";

			var pattern = "((" + patternNums + ")|(" + patternOps + "))";

			var matches = Regex.Matches(infix, pattern);

			var res = new List<Token>();
			foreach(Match m in matches)
			{
				res.Add(new Token(m.ToString()));
			}

			return res;
		}


		public static bool InfixToPostfix(List<Token> infix, out List<Token> postfix)
		{
			postfix = null;

			var stack = new Stack<Token>();
			var res = new List<Token>();

			while(infix.Count > 0)
			{
				var curr = infix[0];

				switch(curr.Type)
				{
					case Token.TokenType.Number:
						res.Add(curr);
						break;

					case Token.TokenType.Function:
						stack.Push(curr);
						break;

					case Token.TokenType.ArgumentSeperator:
						while (true)
						{
							//Wrongly positioned seperator or missing left parenthesis
							if(!stack.Any())
								return false;

							var top = stack.Pop();
		
							if(top.Type == Token.TokenType.LeftParenthesis)
								break;

							res.Add(top);
						}
						break;

					case Token.TokenType.Operator:
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
				if(top.Type == Token.TokenType.LeftParenthesis)
					return false;

				res.Add(top);
			}



			postfix = res;
			return true;
		}



		public static double Evaluate(string infix)
		{
			throw new NotImplementedException();
		}


	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
	class Token
	{


		//Statics
		private static readonly bool _initialized;
		static Token()
		{

			//only execute this method once
			if(_initialized)
				return;

			//Define the valid / known tokens
			_functions = "sin( abs( cos( tan( asin( acos( atan( sqrt( log( ln( exp(".Split(' ');
			_operators = "+ - * / % ^".Split(' ');
			_seperators = "( ) ,".Split(' ');

			ValidTokens = new[] { _functions, _operators, _seperators }.SelectMany(tok => tok).ToArray();


			//Precedences
			_precedences = new Dictionary<string, int>();
			_precedences.Add("+", 1);
			_precedences.Add("-", 1);
			_precedences.Add("*", 2);
			_precedences.Add("/", 2);
			_precedences.Add("%", 2);
			_precedences.Add("^", 3);
			_precedences.Add("(", 0);
			_precedences.Add(")", 0);
			_precedences.Add("sin(", 4);
			_precedences.Add("cos(", 4);
			_precedences.Add("tan(", 4);
			_precedences.Add("asin(", 4);
			_precedences.Add("acos(", 4);
			_precedences.Add("atan(", 4);
			_precedences.Add("sqrt(", 4);
			_precedences.Add("log(", 4);
			_precedences.Add("ln(", 4);
			_precedences.Add("abs(", 4);
			_precedences.Add("exp(", 4);


			//Associativities
			_associativities = new Dictionary<string, AssociativityType>();
			_associativities.Add("+", AssociativityType.Left);
			_associativities.Add("-", AssociativityType.Left);
			_associativities.Add("*", AssociativityType.Left);
			_associativities.Add("/", AssociativityType.Left);
			_associativities.Add("%", AssociativityType.Left);
			_associativities.Add("^", AssociativityType.Right);
			_associativities.Add("(", AssociativityType.None);
			_associativities.Add(")", AssociativityType.None);
			_associativities.Add("sin(", AssociativityType.None);
			_associativities.Add("cos(", AssociativityType.None);
			_associativities.Add("tan(", AssociativityType.None);
			_associativities.Add("asin(", AssociativityType.None);
			_associativities.Add("acos(", AssociativityType.None);
			_associativities.Add("atan(", AssociativityType.None);
			_associativities.Add("sqrt(", AssociativityType.None);
			_associativities.Add("log(", AssociativityType.None);
			_associativities.Add("ln(", AssociativityType.None);
			_associativities.Add("abs(", AssociativityType.None);
			_associativities.Add("exp(", AssociativityType.None);

			//Set initialized flag to true
			_initialized = true;
		}

		private static readonly string[] _functions;
		private static readonly string[] _operators;
		private static readonly string[] _seperators;

		private static readonly Dictionary<string, int> _precedences;
		private static readonly Dictionary<string, AssociativityType> _associativities;

		public static string[] ValidTokens;



		// ==================



		//Constructor
		public Token(string c)
		{
			//Content
			Content = c;


			//Type
			double n;
			if(double.TryParse(c, out n))
				Type = TokenType.Number;
			else if(_operators.Contains(c))
				Type = TokenType.Operator;
			else if(_functions.Contains(c))
				Type = TokenType.Function;
			else if(_seperators.Contains(c))
			{
				switch(c)
				{
					case "(":
						Type = TokenType.LeftParenthesis;
						break;
					case ")":
						Type = TokenType.RightParathesis;
						break;
					case ",":
						Type = TokenType.ArgumentSeperator;
						break;
				}
			}


			//Precendence and associativity
			if (Type == TokenType.Number)
			{
				Precedence = 99;
				Associativity = AssociativityType.None;
			}
			else
			{
				Precedence = _precedences[c];
				Associativity = _associativities[c];
			}
		}

		//Enums
		public enum TokenType
		{
			Number,
			Operator,
			Function,
			ArgumentSeperator,
			LeftParenthesis,
			RightParathesis
		}

		public enum AssociativityType
		{
			Left,
			Right,
			None
		}


		//Properties
		public string Content { get; }
		public TokenType Type { get; }
		public int Precedence { get; private set; }
		public AssociativityType Associativity { get; private set; }

		


		public override string ToString()
		{
			return Content;
		}

	}
}

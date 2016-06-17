using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
	public class Token
	{


		//Statics
		private static readonly bool _initialized;
		static Token()
		{

			//only execute this method once
			if(_initialized)
				return;

			//Define the valid / known tokens
			var polyadics = "gcd( lcm( max( min(".Split(' ');
			_functions = "sin( cos( tan( asin( acos( atan( abs( sqrt( log( ln( exp( fact( ncr( npr( floor( frac(".Split(' ').Concat(polyadics).ToArray();
			_constants = "pi e ans".Split(' ');
			_operators = "+ - * / % ^ ! & |".Split(' ');
			_seperators = "( ) ,".Split(' ');

			ValidTokens = new[] { _functions, _operators, _seperators, _constants }.SelectMany(tok => tok).ToArray();
			PolyadicFunctions = polyadics; 

			//Precedences
			_precedence = new Dictionary<string, int>();
			_precedence.Add("+", 1);
			_precedence.Add("-", 1);
			_precedence.Add("!", 2);
			_precedence.Add("*", 2);
			_precedence.Add("/", 2);
			_precedence.Add("%", 2);
			_precedence.Add("^", 3);
			_precedence.Add("&", 2);
			_precedence.Add("|", 1);	
			_precedence.Add("(", 0);
			_precedence.Add(")", 0);
			_precedence.Add(",", 0);
			_precedence.Add("sin(", 4);
			_precedence.Add("cos(", 4);
			_precedence.Add("tan(", 4);
			_precedence.Add("asin(", 4);
			_precedence.Add("acos(", 4);
			_precedence.Add("atan(", 4);
			_precedence.Add("sqrt(", 4);
			_precedence.Add("log(", 4);
			_precedence.Add("ln(", 4);
			_precedence.Add("abs(", 4);
			_precedence.Add("exp(", 4);
			_precedence.Add("fact(", 4);
			_precedence.Add("ncr(", 4);
			_precedence.Add("npr(", 4);
			_precedence.Add("gcd(", 4);
			_precedence.Add("lcm(", 4);
			_precedence.Add("max(", 4);
			_precedence.Add("min(", 4);
			_precedence.Add("floor(", 4);
			_precedence.Add("frac(", 4);



			//Associativities
			_associativity = new Dictionary<string, AssociativityType>();
			_associativity.Add("+", AssociativityType.Left);
			_associativity.Add("-", AssociativityType.Left);
			_associativity.Add("!", AssociativityType.None);
			_associativity.Add("*", AssociativityType.Left);
			_associativity.Add("/", AssociativityType.Left);
			_associativity.Add("%", AssociativityType.Left);
			_associativity.Add("^", AssociativityType.Right);
			_associativity.Add("&", AssociativityType.Left);
			_associativity.Add("|", AssociativityType.Left);
			_associativity.Add("(", AssociativityType.None);
			_associativity.Add(")", AssociativityType.None);
			_associativity.Add(",", AssociativityType.None);
			_associativity.Add("sin(", AssociativityType.None);
			_associativity.Add("cos(", AssociativityType.None);
			_associativity.Add("tan(", AssociativityType.None);
			_associativity.Add("asin(", AssociativityType.None);
			_associativity.Add("acos(", AssociativityType.None);
			_associativity.Add("atan(", AssociativityType.None);
			_associativity.Add("sqrt(", AssociativityType.None);
			_associativity.Add("log(", AssociativityType.None);
			_associativity.Add("ln(", AssociativityType.None);
			_associativity.Add("abs(", AssociativityType.None);
			_associativity.Add("exp(", AssociativityType.None);
			_associativity.Add("fact(", AssociativityType.None);
			_associativity.Add("ncr(", AssociativityType.None);
			_associativity.Add("npr(", AssociativityType.None);
			_associativity.Add("gcd(", AssociativityType.None);
			_associativity.Add("lcm(", AssociativityType.None);
			_associativity.Add("max(", AssociativityType.None);
			_associativity.Add("min(", AssociativityType.None);
			_associativity.Add("floor(", AssociativityType.None);
			_associativity.Add("frac(", AssociativityType.None);

			//Arities
			_arity = new Dictionary<string, int>();
			_arity.Add("+", 2);
			_arity.Add("-", 2);
			_arity.Add("!", 1);
			_arity.Add("*", 2);
			_arity.Add("/", 2);
			_arity.Add("%", 2);
			_arity.Add("^", 2);
			_arity.Add("&", 2);
			_arity.Add("|", 2);
			_arity.Add("(", -1);
			_arity.Add(")", -1);
			_arity.Add(",", -1);
			_arity.Add("sin(", 1);
			_arity.Add("cos(", 1);
			_arity.Add("tan(", 1);
			_arity.Add("asin(", 1);
			_arity.Add("acos(", 1);
			_arity.Add("atan(", 1);
			_arity.Add("sqrt(", 1);
			_arity.Add("log(", 2);
			_arity.Add("ln(", 1);
			_arity.Add("abs(", 1);
			_arity.Add("exp(", 1);
			_arity.Add("fact(", 1);
			_arity.Add("ncr(", 2);
			_arity.Add("npr(", 2);
			_arity.Add("gcd(", 0);
			_arity.Add("lcm(", 0);
			_arity.Add("max(", 0);
			_arity.Add("min(", 0);
			_arity.Add("floor(", 1);
			_arity.Add("frac(", 1);

			//Set initialized flag to true
			_initialized = true;
		}

		private static readonly string[] _functions;
		private static readonly string[] _constants;
		private static readonly string[] _operators;
		private static readonly string[] _seperators;

		private static readonly Dictionary<string, int> _precedence;
		private static readonly Dictionary<string, AssociativityType> _associativity;
		private static readonly Dictionary<string, int> _arity;

		public static string[] ValidTokens;
		public static string[] PolyadicFunctions;



		// ==================



		//Constructor
		public Token(string c)
		{
			//Content
			c = c.ToLower();
			Content = c;


			//Type
			double n;
			if(double.TryParse(c, out n))
				Type = TokenType.Number;
			else if (_constants.Contains(c))
				Type = TokenType.Constant;
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
			if (Type == TokenType.Number || Type == TokenType.Constant)
			{
				Precedence = 99;
				Associativity = AssociativityType.None;
				Arity = 0;
			}
			else
			{
				Precedence = _precedence[c];
				Associativity = _associativity[c];
				Arity = _arity[c];
			}
		}


		//Enums



		//Properties
		public string Content { get; }
		public TokenType Type { get; }
		public int Precedence { get; private set; }
		public AssociativityType Associativity { get; private set; }
		public int Arity { get; set; }



		public override string ToString()
		{
			return Content;
		}

	}
}

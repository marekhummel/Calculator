using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculator
{
    public class Token
    {


        //Statics
        static Token()
        {
            //Define the valid / known tokens
            string[]? polyadics = "gcd( lcm( max( min(".Split(' ');
            _functions = "sin( cos( tan( asin( acos( atan( abs( sqrt( log( ln( exp( fact( ncr( npr( floor( frac(".Split(' ').Concat(polyadics).ToArray();
            _constants = "pi e ans".Split(' ');
            _operators = "+ - * / % ^ ! & |".Split(' ');
            _seperators = "( ) ,".Split(' ');

            ValidTokens = new[] { _functions, _operators, _seperators, _constants }.SelectMany(tok => tok).ToArray();
            PolyadicFunctions = polyadics;

            //Precedences
            _precedence = new Dictionary<string, int> {
                { "+", 1 },
                { "-", 1 },
                { "!", 2 },
                { "*", 2 },
                { "/", 2 },
                { "%", 2 },
                { "^", 3 },
                { "&", 2 },
                { "|", 1 },
                { "(", 0 },
                { ")", 0 },
                { ",", 0 },
                { "sin(", 4 },
                { "cos(", 4 },
                { "tan(", 4 },
                { "asin(", 4 },
                { "acos(", 4 },
                { "atan(", 4 },
                { "sqrt(", 4 },
                { "log(", 4 },
                { "ln(", 4 },
                { "abs(", 4 },
                { "exp(", 4 },
                { "fact(", 4 },
                { "ncr(", 4 },
                { "npr(", 4 },
                { "gcd(", 4 },
                { "lcm(", 4 },
                { "max(", 4 },
                { "min(", 4 },
                { "floor(", 4 },
                { "frac(", 4 }
            };



            //Associativities
            _associativity = new Dictionary<string, AssociativityType> {
                { "+", AssociativityType.Left },
                { "-", AssociativityType.Left },
                { "!", AssociativityType.None },
                { "*", AssociativityType.Left },
                { "/", AssociativityType.Left },
                { "%", AssociativityType.Left },
                { "^", AssociativityType.Right },
                { "&", AssociativityType.Left },
                { "|", AssociativityType.Left },
                { "(", AssociativityType.None },
                { ")", AssociativityType.None },
                { ",", AssociativityType.None },
                { "sin(", AssociativityType.None },
                { "cos(", AssociativityType.None },
                { "tan(", AssociativityType.None },
                { "asin(", AssociativityType.None },
                { "acos(", AssociativityType.None },
                { "atan(", AssociativityType.None },
                { "sqrt(", AssociativityType.None },
                { "log(", AssociativityType.None },
                { "ln(", AssociativityType.None },
                { "abs(", AssociativityType.None },
                { "exp(", AssociativityType.None },
                { "fact(", AssociativityType.None },
                { "ncr(", AssociativityType.None },
                { "npr(", AssociativityType.None },
                { "gcd(", AssociativityType.None },
                { "lcm(", AssociativityType.None },
                { "max(", AssociativityType.None },
                { "min(", AssociativityType.None },
                { "floor(", AssociativityType.None },
                { "frac(", AssociativityType.None }
            };

            //Arities
            _arity = new Dictionary<string, int> {
                { "+", 2 },
                { "-", 2 },
                { "!", 1 },
                { "*", 2 },
                { "/", 2 },
                { "%", 2 },
                { "^", 2 },
                { "&", 2 },
                { "|", 2 },
                { "(", -1 },
                { ")", -1 },
                { ",", -1 },
                { "sin(", 1 },
                { "cos(", 1 },
                { "tan(", 1 },
                { "asin(", 1 },
                { "acos(", 1 },
                { "atan(", 1 },
                { "sqrt(", 1 },
                { "log(", 2 },
                { "ln(", 1 },
                { "abs(", 1 },
                { "exp(", 1 },
                { "fact(", 1 },
                { "ncr(", 2 },
                { "npr(", 2 },
                { "gcd(", 0 },
                { "lcm(", 0 },
                { "max(", 0 },
                { "min(", 0 },
                { "floor(", 1 },
                { "frac(", 1 }
            };
        }

        private static readonly string[] _functions;
        private static readonly string[] _constants;
        private static readonly string[] _operators;
        private static readonly string[] _seperators;

        private static readonly Dictionary<string, int> _precedence;
        private static readonly Dictionary<string, AssociativityType> _associativity;
        private static readonly Dictionary<string, int> _arity;


        // Public properties
        public static string[] ValidTokens { get; }
        public static string[] PolyadicFunctions { get; }



        // ==================



        //Constructor
        public Token(string c)
        {
            //Content
            c = c.ToLower();
            Content = c;


            //Type
            if (double.TryParse(c, out double _)) {
                Type = TokenType.Number;
            }
            else if (_constants.Contains(c)) {
                Type = TokenType.Constant;
            }
            else if (_operators.Contains(c)) {
                Type = TokenType.Operator;
            }
            else if (_functions.Contains(c)) {
                Type = TokenType.Function;
            }
            else if (_seperators.Contains(c)) {
                switch (c) {
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
            if (Type == TokenType.Number || Type == TokenType.Constant) {
                Precedence = 99;
                Associativity = AssociativityType.None;
                Arity = 0;
            }
            else {
                Precedence = _precedence[c];
                Associativity = _associativity[c];
                Arity = _arity[c];
            }
        }


        //Properties
        public string Content { get; }
        public TokenType Type { get; }
        public int Precedence { get; private set; }
        public AssociativityType Associativity { get; private set; }
        public int Arity { get; set; }



        public override string ToString() => Content;

    }
}

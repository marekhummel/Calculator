﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Calculator
{
    public static class ExpressionParser
    {

        //Define the operations
        static ExpressionParser()
        {
            //Preset some operations
            static int? tryParse(double d) => !int.TryParse(d.ToString(CultureInfo.InvariantCulture), out int i) ? null : i;

            double gcd(double a, double b)
            {
                if (a < b) {
                    return gcd(b, a);
                }

                int? i = tryParse(a);
                int? j = tryParse(b);

                if (i == null || j == null) {
                    return double.NaN;
                }
                return j != 0 ? gcd((double)j, (double)i % (double)j) : (double)i;
            }

            double lcm(double a, double b)
            {
                int? i = tryParse(a);
                int? j = tryParse(b);
                return i == null || j == null ? double.NaN : (double)i * (double)j / gcd((double)i, (double)j);
            }

            double fact(double a)
            {
                int? i = tryParse(a);
                return i == null || i < 0 ? double.NaN : Enumerable.Range(1, (int)i).Aggregate(1d, (x, y) => x * y);
            }

            double and(double a, double b)
            {
                int? i = tryParse(a);
                int? j = tryParse(b);
                return i == null || j == null ? double.NaN : (double)(i & j);
            }

            double or(double a, double b)
            {
                int? i = tryParse(a);
                int? j = tryParse(b);
                return i == null || j == null ? double.NaN : (double)(i | j);
            }



            //Initialze the _operations dictionaries
            _binaryOperations = new Dictionary<string, Func<double, double, double>> {
                { "+", (a, b) => a + b },
                { "-", (a, b) => a - b },
                { "*", (a, b) => a * b },
                { "/", (a, b) => Math.Abs(b) < 10E-10 ? double.NaN : a / b },
                { "%", (a, b) => a % b },
                { "^", Math.Pow },
                { "&", and },
                { "|", or },
                { "log(", (a, b) => Math.Log(b, a) },
                { "ncr(", (a, b) => fact(a) / (fact(b) * fact(a - b)) },
                { "npr(", (a, b) => fact(a) / fact(b) }
            };

            _unaryOperations = new Dictionary<string, Func<double, double>> {
                { "!", a => -a },
                { "sin(", Math.Sin },
                { "cos(", Math.Cos },
                { "tan(", Math.Tan },
                { "asin(", Math.Asin },
                { "acos(", Math.Acos },
                { "atan(", Math.Atan },
                { "abs(", Math.Abs },
                { "sqrt(", Math.Sqrt },
                { "ln(", Math.Log },
                { "exp(", Math.Exp },
                { "fact(", fact },
                { "floor(", Math.Floor },
                { "frac(", a => a - Math.Floor(a) }
            };

            _polyadicFunctions = new Dictionary<string, Func<IEnumerable<double>, double>> {
                { "gcd(", ops => ops.Aggregate((a, b) => gcd(a, b)) },
                { "lcm(", ops => ops.Aggregate((a, b) => lcm(a, b)) },
                { "max(", ops => ops.Aggregate(Math.Max) },
                { "min(", ops => ops.Aggregate(Math.Min) }
            };

            _constants = new Dictionary<string, double> {
                { "pi", Math.PI },
                { "e", Math.E },
                { "ans", 0 }
            };
        }

        private static readonly Dictionary<string, Func<double, double, double>> _binaryOperations;
        private static readonly Dictionary<string, Func<double, double>> _unaryOperations;
        private static readonly Dictionary<string, Func<IEnumerable<double>, double>> _polyadicFunctions;
        private static readonly Dictionary<string, double> _constants;


        //Unit of angular measure
        public enum AngleUnit
        {
            Degrees,
            Radians,
            Grad
        }
        public static AngleUnit UsedAngleUnit { get; set; }




        //Tokenize of the infix string
        public static List<Token> Tokenize(string infix)
        {
            //Define the pattern
            const string patternNums = @"\d*\.?\d+(E[+-]?\d+)?";
            string? patternOps = string.Join("|", Token.ValidTokens.Select(Regex.Escape));
            string? pattern = patternNums + "|" + patternOps;

            //Find matches
            var matches = Regex.Matches(infix, pattern, RegexOptions.IgnoreCase);

            //Convert the matches to tokens
            var tokens = matches.Cast<Match>().Select(m => new Token(m.ToString())).ToList();


            //Additional processing
            //Set arities for polyadic functions
            foreach (var tk in tokens.Where(t => Token.PolyadicFunctions.Contains(t.Content))) {
                int index = tokens.FindIndex(t => t == tk);

                int openparenthesis = 1;
                int argsCount = 0;
                foreach (var t in tokens.Skip(index + 1)) {
                    switch (t.Type) {
                        case TokenType.Function:
                            openparenthesis++;
                            break;
                        case TokenType.RightParathesis:
                            openparenthesis--;
                            break;
                    }

                    if ((t.Type == TokenType.Number || t.Type == TokenType.RightParathesis) && openparenthesis == 1) {
                        argsCount++;
                    }

                    if (openparenthesis == 0) {
                        break;
                    }
                }

                tokens[index].Arity = argsCount;
            }



            //Special handling of implicit multiply operator and unary minus/plus operator
            var pairs = tokens.Take(tokens.Count - 1).Select((tk, i) => new[] { tk, tokens[i + 1] }).ToList();

            //Insert implicit multiply operators	
            int pindex = 1;
            foreach (var p in pairs) {
                Token left = p[0], right = p[1];

                if (left.Type == TokenType.Number && right.Type == TokenType.Constant) {
                    tokens.Insert(pindex, new Token("*"));
                }
                else if (left.Type == TokenType.RightParathesis && right.Type == TokenType.LeftParenthesis) {
                    tokens.Insert(pindex, new Token("*"));
                }

                pindex++;
            }

            //Detect unary minus/plus
            pindex = 0;
            foreach (var p in pairs) {
                Token left = p[0], right = p[1];

                if (pindex == 0 && left.Content == "-") {
                    tokens[0] = new Token("!");
                }
                else if (pindex == 0 && left.Content == "+") {
                    tokens.RemoveAt(0);
                }
                else if (left.Type == TokenType.Operator && right.Type == TokenType.Operator) {
                    if (right.Content == "-") {
                        tokens[pindex + 1] = new Token("!");
                    }
                    else if (right.Content == "+") {
                        tokens.RemoveAt(pindex + 1);
                    }
                }
                else if ((left.Type == TokenType.LeftParenthesis || left.Type == TokenType.ArgumentSeperator) && right.Type == TokenType.Operator) {
                    if (right.Content == "-") {
                        tokens[pindex + 1] = new Token("!");
                    }
                    else if (right.Content == "+") {
                        tokens.RemoveAt(pindex + 1);
                    }
                }



                pindex++;
            }

            return tokens;
        }



        //Infix to postfix
        public static EvalResult.ErrorType ShuntingYardAlgorithm(List<Token> infix, out List<Token> postfix)
        {
            postfix = new List<Token>();

            var tokens = new Queue<Token>(infix);
            var stack = new Stack<Token>();
            var res = new List<Token>();

            while (tokens.Any()) {
                var curr = tokens.Dequeue();

                switch (curr.Type) {
                    case TokenType.Number:
                    case TokenType.Constant:
                        res.Add(curr);
                        break;

                    case TokenType.Function:
                        stack.Push(curr);
                        break;

                    case TokenType.ArgumentSeperator:
                        while (true) {
                            //Wrongly positioned seperator or missing left parenthesis
                            if (!stack.Any()) {
                                return EvalResult.ErrorType.SyntaxError;
                            }

                            if (stack.Peek().Type == TokenType.Function) {
                                break;
                            }

                            res.Add(stack.Pop());
                        }
                        break;

                    case TokenType.Operator:
                        if (curr.Associativity == AssociativityType.Left) {
                            while (stack.Any()) {
                                var top = stack.Peek();
                                if (top.Type == TokenType.Operator && curr.Precedence <= top.Precedence) {
                                    res.Add(stack.Pop());
                                }
                                else {
                                    break;
                                }
                            }
                        }
                        stack.Push(curr);
                        break;

                    case TokenType.LeftParenthesis:
                        stack.Push(curr);
                        break;

                    case TokenType.RightParathesis:
                        while (true) {
                            //Missing left parenthesis
                            if (!stack.Any()) {
                                return EvalResult.ErrorType.SyntaxError;
                            }

                            var top = stack.Pop();

                            if (top.Type == TokenType.LeftParenthesis) {
                                break;
                            }

                            if (top.Type == TokenType.Function) {
                                res.Add(top);
                                break;
                            }

                            res.Add(top);
                        }
                        break;
                }
            }

            //Flush the stack to the result
            while (stack.Count > 0) {
                var top = stack.Pop();

                //More left parentheses than right ones
                if (top.Type == TokenType.LeftParenthesis || top.Type == TokenType.Function) {
                    return EvalResult.ErrorType.SyntaxError;
                }

                res.Add(top);
            }

            postfix = res;
            return EvalResult.ErrorType.Success;
        }



        //Evaluation of a infix string
        public static EvalResult Evaluate(string infix)
        {
            //Tokenize the string
            var tokens = Tokenize(infix);

            //Transform to postfix notation
            var resultSya = ShuntingYardAlgorithm(tokens, out var postfix);

            //Possible invalid infix
            return resultSya != EvalResult.ErrorType.Success ? new EvalResult(resultSya) : Evaluate(postfix);
        }

        //Intern evaluation method
        private static EvalResult Evaluate(IEnumerable<Token> postfix)
        {
            var tokens = new Queue<Token>(postfix);
            var resultStack = new Stack<double>();

            //Iterate through all tokens
            while (tokens.Any()) {
                //Get the current token
                var curr = tokens.Dequeue();

                //Act based on the token type
                double value;
                switch (curr.Type) {
                    case TokenType.Number:
                        resultStack.Push(double.Parse(curr.Content, CultureInfo.InvariantCulture));
                        break;

                    case TokenType.Constant:
                        resultStack.Push(_constants[curr.Content]);
                        break;

                    case TokenType.Operator:
                        if (resultStack.Count < curr.Arity) {
                            return new EvalResult(EvalResult.ErrorType.SyntaxError);
                        }

                        //Calculate
                        value = 0d;
                        switch (curr.Arity) {
                            case 2:
                                double right = resultStack.Pop();
                                double left = resultStack.Pop();
                                value = _binaryOperations[curr.Content].Invoke(left, right);
                                break;
                            case 1:
                                double operand = resultStack.Pop();
                                value = _unaryOperations[curr.Content].Invoke(operand);
                                break;
                        }

                        if (double.IsNaN(value)) {
                            return new EvalResult(EvalResult.ErrorType.MathError);
                        }

                        //Push
                        resultStack.Push(value);
                        break;

                    case TokenType.Function:
                        var operands = new Stack<double>();

                        //Check whether the necessary operands for this operations are given
                        if (resultStack.Count < curr.Arity) {
                            return new EvalResult(EvalResult.ErrorType.ArgumentError);
                        }

                        //Get the operands
                        for (int i = 0; i < curr.Arity; i++) {
                            operands.Push(resultStack.Pop());
                        }

                        //Calculate
                        switch (curr.Arity) {
                            case 2:
                                if (Token.PolyadicFunctions.Contains(curr.Content)) {
                                    value = _polyadicFunctions[curr.Content].Invoke(operands);
                                }
                                else {
                                    value = _binaryOperations[curr.Content].Invoke(operands.Pop(), operands.Pop());
                                }

                                break;
                            case 1:
                                //Convert operand if trignometric function
                                if (new[] { "sin(", "cos(", "tan(", "asin(", "acos(", "atan(" }.Any(func => func == curr.Content)) {
                                    double operand = operands.Pop();
                                    switch (UsedAngleUnit) {
                                        case AngleUnit.Degrees:
                                            operand = operand * Math.PI / 180;
                                            break;
                                        case AngleUnit.Grad:
                                            operand = operand * Math.PI / 200;
                                            break;
                                    }
                                    operands.Push(operand);
                                }

                                if (!_unaryOperations.ContainsKey(curr.Content)) {
                                    return new EvalResult(EvalResult.ErrorType.SyntaxError);
                                }

                                value = _unaryOperations[curr.Content].Invoke(operands.Pop());
                                break;
                            default:
                                value = _polyadicFunctions[curr.Content].Invoke(operands);
                                break;
                        }

                        if (double.IsNaN(value)) {
                            return new EvalResult(EvalResult.ErrorType.MathError);
                        }

                        //Push
                        resultStack.Push(value);
                        break;

                    default:
                        //Invalid token, postfix notation neither supports parenthesis nor argument seperators
                        return new EvalResult(EvalResult.ErrorType.SyntaxError);
                }
            }

            //Result stack has to be reduced to one token
            if (resultStack.Count != 1) {
                _constants["ans"] = double.NaN;
                return new EvalResult(EvalResult.ErrorType.SyntaxError);
            }

            double ret = Math.Round(resultStack.Pop(), 10);
            _constants["ans"] = ret;
            return new EvalResult(ret);
        }





        //Creation of a expression tree based on a infix string
        public static TreeViewItem? CreateExpressionTree(string infix)
        {
            //Tokenize the string
            var tokens = Tokenize(infix);

            //Transform to postfix notation
            var success = ShuntingYardAlgorithm(tokens, out var postfix);

            //Possible invalid infix
            return success == EvalResult.ErrorType.Success ? CreateExpressionTree(postfix) : null;
        }

        //Intern exptree method
        private static TreeViewItem? CreateExpressionTree(IEnumerable<Token> postfix)
        {
            var tokens = new Queue<Token>(postfix);
            var result = new Stack<TreeViewItem>();

            //Iterate through all tokens
            while (tokens.Any()) {
                //Get the current token
                var curr = tokens.Dequeue();

                //Act based on the token type
                var newitem = new TreeViewItem { Header = curr.Content, IsExpanded = true };
                switch (curr.Type) {
                    case TokenType.Number:
                    case TokenType.Constant:
                        result.Push(newitem);
                        break;

                    case TokenType.Operator:

                        if (result.Count < curr.Arity) {
                            return null;
                        }

                        //Add children
                        switch (curr.Arity) {
                            case 2:
                                _ = newitem.Items.Add(result.Pop());
                                newitem.Items.Insert(0, result.Pop());
                                result.Push(newitem);

                                break;
                            case 1:
                                _ = newitem.Items.Add(result.Pop());
                                result.Push(newitem);

                                break;
                        }

                        break;

                    case TokenType.Function:
                        var operands = new Stack<TreeViewItem>();

                        //Check whether the necessary operands for this operations are given
                        if (result.Count < curr.Arity) {
                            return null;
                        }

                        //Get the operands
                        for (int i = 0; i < curr.Arity; i++) {
                            operands.Push(result.Pop());
                        }

                        //Add the children
                        switch (curr.Arity) {
                            case 2:
                                if (Token.PolyadicFunctions.Contains(curr.Content)) {
                                    operands.ToList().ForEach(o => newitem.Items.Add(o));
                                }
                                else {
                                    _ = newitem.Items.Add(operands.Pop());
                                    _ = newitem.Items.Add(operands.Pop());
                                }
                                break;
                            case 1:
                                _ = newitem.Items.Add(operands.Pop());
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
            return result.Count == 1 ? result.Pop() : null;

        }

    }
}

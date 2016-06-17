using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Calculator
{
	public struct EvalResult
	{

		public EvalResult(double val)
		{
			Value = val;
			Error = EvalResult.ErrorType.Success;
		}

		public EvalResult(ErrorType type)
		{
			Value = double.NaN;
			Error = type;
		}

		public enum ErrorType
		{
			Success,
			SyntaxError,
			MathError,
			ArgumentError
		}

		public double Value { get; }
		public ErrorType Error { get; }


		public override string ToString()
		{
			return Error == ErrorType.Success ? Value.ToString("G", CultureInfo.InvariantCulture) : Error.ToString();
		}
	}
}

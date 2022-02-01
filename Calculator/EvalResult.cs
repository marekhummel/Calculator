using System.Globalization;

namespace Calculator
{
    public struct EvalResult
    {

        public EvalResult(double val)
        {
            Value = val;
            Error = ErrorType.Success;
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
            => Error == ErrorType.Success ? Value.ToString("G", CultureInfo.InvariantCulture) : Error.ToString();
    }
}

namespace Calculator
{
    public enum TokenType
    {
        Number,
        Constant,
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
}

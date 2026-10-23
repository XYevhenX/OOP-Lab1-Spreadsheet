namespace SpreadsheetApp.Parsing
{
    public enum TokenType
    {
        Number, CellReference,
        Equal, Less, Greater, LessOrEqual, GreaterOrEqual, NotEqual,
        Not, Or, And, Eqv,
        LParen, RParen, EOF
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }
}
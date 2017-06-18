namespace lox
{
    class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object Literal;
        public readonly int Line;

        public override string ToString() => $"{Type} {Lexeme} {Literal}";

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }
    }
}
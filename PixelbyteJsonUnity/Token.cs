namespace Pixelbyte.JsonUnity
{
    class Token
    {
        public TokenType Kind { get; private set; }
        public string Lexeme { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, int line, int column, string lexeme = null)
        {
            Kind = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}:{2}] {3}", Kind, Line, Column, Lexeme);
        }
    }
}

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
            if (!string.IsNullOrEmpty(Lexeme))
                return string.Format("[{0}:{1}] {2} = {3}", Line, Column, Kind, Lexeme);
            else
                return string.Format("[{0}:{1}] {2} ", Line, Column, Kind);
        }
    }
}

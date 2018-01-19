namespace Pixelbyte.JsonUnity
{
    class Token
    {
        public TokenType Kind { get; private set; }
        public string Lexeme { get; private set; }
        /// <summary>
        /// Some lexemes have a literal value. This holds those
        /// </summary>
        public object Literal { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, int line, int column, string lexeme = null, object literal = null)
        {
            Kind = type;
            Column = column;
            Literal = literal;
            Lexeme = lexeme;
            Line = line;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Lexeme))
                return string.Format("[{0}:{1}] {2} = {3}", Line, Column, Kind.Actual(), Lexeme);
            else
                return string.Format("[{0}:{1}] {2} ", Line, Column, Kind.Actual());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixelbyte.JsonUnity
{
    public class JSONTokenizer
    {
        //Matches a properly-formatted json number (except taht it allows multiple '.')
        static Regex jsonNumMatcher = new Regex(@"-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?$", RegexOptions.Compiled);
        //Matches a 4-digit HEX number
        static Regex hexNumber = new Regex(@"[0-9a-fA-F]{4}", RegexOptions.Compiled);

        static readonly char[] WHITESPACE = new char[] { '\t', ' ', '\r', '\n' };
        static readonly char[] NONVALUECHARS = new char[] { ',', ':', '\"', '{', '}', '[', ']', '\n', '\t', ' ', '\r' };
        //static readonly char[] NUMBERCHARS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', 'e', 'E', '-', '+' };

        static char[] STRING_ESCAPES = new char[] { '"', '/', '\\', 'b', 'r', 'n', 'f', 't' };
        static char[] STRING_ESCAPE_CODES = new char[] { '"', '/', '\\', '\b', '\r', '\n', '\f', '\t' };
        static char[] FLOATING_POINT_CHARS = new char[] { '.', 'e', 'E' };

        //The json string we want to parse
        string json;

        //How far we are into the json string
        int index = 0;

        //What column we are currently in
        int column = 0;

        //What line of the string we are curently on
        int line = 0;

        //This is where we'll stor a multi-character token
        StringBuilder sb = new StringBuilder();
        List<string> errors;

        public List<Token> tokens;

        public bool Successful { get; private set; }

        public List<String> Errors { get { return errors; } }

        /// <summary>
        /// Gets all the errors. One per line.
        /// </summary>
        public string AllErrors { get { return String.Join(Environment.NewLine, Errors.ToArray()); } }

        public JSONTokenizer() { tokens = new List<Token>(); errors = new List<string>(); }

        public void Tokenize(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException("json string cannot be null or empty!");
            try
            {
                this.json = json;
                errors.Clear();
                Successful = true;
                index = 0;
                column = 1; //Start in column 1, not 0
                line = 1; //Start on line 1
                sb.Length = 0;
                GetTokens();
                this.json = String.Empty;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }

        void GetTokens()
        {
            EatWhiteSpace();
            while (!EOF())
            {
                int currentLine = line;
                int currentColumn = column;
                char c = Peek();
                switch (c)
                {
                    //An Array beginning or end?
                    case '[':
                        NextChar();
                        tokens.Add(new Token(TokenType.OpenBracket, currentLine, currentColumn));
                        break;
                    case ']':
                        NextChar();
                        tokens.Add(new Token(TokenType.CloseBracket, currentLine, currentColumn));
                        break;
                    //An object beginning or end?
                    case '{':
                        NextChar();
                        tokens.Add(new Token(TokenType.OpenCurly, currentLine, currentColumn));
                        break;
                    case '}':
                        NextChar();
                        tokens.Add(new Token(TokenType.CloseCurly, currentLine, currentColumn));
                        break;
                    case ':':
                        NextChar();
                        tokens.Add(new Token(TokenType.Colon, currentLine, currentColumn));
                        break;
                    case ',':
                        NextChar();
                        tokens.Add(new Token(TokenType.Comma, currentLine, currentColumn));
                        break;
                    case '"':
                        var str = ReadString();
                        if (!string.IsNullOrEmpty(str))
                            tokens.Add(new Token(TokenType.String, currentLine, currentColumn, str, str));
                        //LogError("Illegal null or empty string", currentLine, currentColumn);
                        break;
                    default:
                        var val = ReadValue();
                        if (val == "false")
                            tokens.Add(new Token(TokenType.False, currentLine, currentColumn, val, false));
                        else if (val == "true")
                            tokens.Add(new Token(TokenType.True, currentLine, currentColumn, val, true));
                        else if (val == "null")
                            tokens.Add(new Token(TokenType.Null, currentLine, currentColumn, val, null));
                        else if (!string.IsNullOrEmpty(val) && (char.IsDigit(val[0]) || val[0] == '-'))
                        {
                            if (val.CountChar('.') > 1 || !jsonNumMatcher.IsMatch(val))
                            {
                                LogError("Malformed number: " + val, currentLine, currentColumn);
                                break;
                            }

                            //Is it an integer?
                            if (val.IndexOfAny(FLOATING_POINT_CHARS) == -1)
                            {
                                //Store the value in the largest C# type
                                if (val[0] == '-')
                                {
                                    Int64 value;
                                    if (!Int64.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out value))
                                    {
                                        LogError("Unable to parse number from string " + val);
                                    }
                                    tokens.Add(new Token(TokenType.Number, currentLine, currentColumn, val, value));
                                }
                                else
                                {
                                    UInt64 value;
                                    if (!UInt64.TryParse(val, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out value))
                                    {
                                        LogError("Unable to parse number from string " + val);
                                    }
                                    tokens.Add(new Token(TokenType.Number, currentLine, currentColumn, val, value));
                                }
                            }
                            //Floating point number? Ok then.
                            else
                            {
                                //Store the value in the largest C# type
                                Double value;
                                if (!Double.TryParse(val, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out value))
                                {
                                    LogError("Unable to parse number from string " + val);
                                }
                                tokens.Add(new Token(TokenType.Number, currentLine, currentColumn, val, value));
                            }
                        }
                        else
                        {
                            LogError(string.Format("Unexpected unquoted string: {0}", val), currentLine, currentColumn);
                            //If there is a quote at the end of this, just eat it
                            if (!EOF() && Peek() == '"') NextChar();
                            //LogError("Supported unquoted strings: true, false, null");
                        }
                        break;
                }
                EatWhiteSpace();
            }
        }

        bool EOF() { return index >= json.Length; }
        char Peek() { return json[index]; }

        char NextChar()
        {
            column++;
            if (json[index] == '\n')
            {
                column = 1;
                line++;
            }
            return json[index++];
        }

        /// <summary>
        /// Returns true if there are at least the specified number of characters left in the json string
        /// num specified must be > 0
        /// </summary>
        bool CharactersLeft(int num) { return index + (num - 1) < json.Length; }

        bool PeekMatch(char c, int lookahead = 0)
        {
            var newIndex = index + lookahead;
            if (newIndex < json.Length) return json[newIndex] == c;
            return false;
        }

        void EatWhiteSpace() { while (!EOF() && WHITESPACE.Contains(Peek())) NextChar(); }

        bool IsNonValueCharacter(char c) { return NONVALUECHARS.Contains(c); }

        string ReadValue()
        {
            sb.Length = 0;
            while (!EOF() && !IsNonValueCharacter(Peek()))
                sb.Append(NextChar());
            return sb.ToString();
        }

        private string ReadString()
        {
            char c = '\0';
            sb.Length = 0;
            //Eat the starting "
            NextChar();

            while (!EOF())
            {
                c = NextChar();

                if (c == '\\')
                {
                    bool escapeMatch = false;
                    //Check for all valid escape characters
                    for (int i = 0; i < STRING_ESCAPES.Length; i++)
                    {
                        if (PeekMatch(STRING_ESCAPES[i]))
                        {
                            sb.Append(STRING_ESCAPE_CODES[i]);
                            escapeMatch = true;
                            break;
                        }
                    }

                    if (!escapeMatch)
                    {
                        if (Peek() == 'u')
                        {
                            NextChar();
                            //Check and make sure there are 4 characters for the code
                            //and make sure that the last of the 4 characters in NOT a "
                            if (!CharactersLeft(4) || PeekMatch('"', 3))
                            {
                                LogError("Unicode escape code must be followed by 4 hex digits!", line, column);
                                return null;
                            }
                            else
                            {
                                var hex = new StringBuilder();
                                for (int i = 0; i < 4; i++)
                                {
                                    hex.Append(NextChar());
                                }

                                if (!hexNumber.IsMatch(hex.ToString()))
                                {
                                    LogError("Invalid Unicode escape code: " + hex.ToString(), line, column - 6);
                                }
                                else
                                {
                                    //Valid!
                                    sb.Append((char)Convert.ToInt32(hex.ToString(), 16));
                                }
                            }
                        }
                        else
                        {
                            LogError("Illegal escape specifier: " + c);
                            return null;
                        }
                    }
                }
                //A newline ends the deal too
                else if (c == '"' || c == '\r' || c == '\n')
                    break;
                else
                {
                    sb.Append(c);
                }
            }

            //Check for correctly-terminated string
            if (c == '"')
            {
                return sb.ToString();
            }
            else
            {
                LogError("Unterminated string!", line, column - sb.Length);
                return null;
            }
        }

        void LogError(string text, int line = -1, int col = -1)
        {
            if (line > -1)
                errors.Add(string.Format("Tokenizer [{0}:{1}] {2}", line, column, text));
            else
                errors.Add(text);
            Successful = false;
        }
    }
}

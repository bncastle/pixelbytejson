using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JSONObject
    {
        public string name;
        public List<JSONPair> pairs;

        public JSONObject(string name = null)
        {
            this.name = name;
            pairs = new List<JSONPair>();
        }

        public bool Add(JSONPair pair)
        {
            //TODO: Check for duplicates?
            pairs.Add(pair);
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append(":");
            sb.AppendLine();
            for (int i = 0; i < pairs.Count; i++)
            {
                sb.Append(pairs[i].ToString());
                if (i < pairs.Count - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class JSONPair
    {
        public string name;
        public object value;

        public JSONPair(string name, object val)
        {
            this.name = name;
            value = val;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", name, value.ToString());
        }
    }

    /// <summary>
    /// Parses the given JSON string
    /// reference: https://www.json.org/
    /// </summary>
    public class JSONParser
    {
        static void Main()
        {
            //Pull in the json text
            string testJson = string.Empty;
            using (var sr = new StreamReader(@"..\..\TestClass.json"))
            {
                testJson = sr.ReadToEnd();
            }

            var jparser = Parse(testJson);

            //Console.WriteLine(Environment.CurrentDirectory);
        }

        JSONTokenizer tokenizer;
        List<string> errors;

        public bool IsTokenizerError { get { return tokenizer.IsError; } }
        public List<string> TokenizerErrors { get { return tokenizer.errors; } }
        public List<string> ParserErrors { get { return errors; } }
        public bool IsParserError { get; private set; }

        int tokenIndex = 0;

        /// <summary>
        /// Peeks at the next availabel token without consuming it
        /// </summary>
        Token PeekToken
        {
            get
            {
                if (tokenizer.tokens.Count == 0 || tokenIndex >= tokenizer.tokens.Count) return null;
                return tokenizer.tokens[tokenIndex];
            }
        }

        /// <summary>
        /// Retrieves the previous token or null if there is not one
        /// </summary>
        Token PreviousToken
        {
            get
            {
                if (tokenizer.tokens.Count == 0 || tokenIndex < 2) return null;
                return tokenizer.tokens[tokenIndex - 2];
            }
        }

        TokenType PreviousTokenType
        {
            get
            {
                if (PreviousToken == null) return TokenType.None;
                return PreviousToken.Kind;
            }
        }

        private JSONParser(JSONTokenizer tok)
        {
            tokenizer = tok;
            IsParserError = false;
            errors = new List<string>();
        }

        public static JSONParser Parse(string json)
        {
            JSONTokenizer tok = new JSONTokenizer();
            JSONParser jp = new JSONParser(tok);

            //1st tokenize
            tok.Tokenize(json);
            if (!tok.IsError)
            {
                //foreach (var t in tok.tokens)
                //    Console.WriteLine(t.ToString());
                //Now parse
                jp.Parse();

                if (jp.IsParserError)
                {
                    foreach (var err in jp.errors)
                    {
                        Console.WriteLine(err);
                    }
                }
            }

            return jp;
        }

        /// <summary>
        /// Gets the next available token and advances to the next one
        /// </summary>
        Token NextToken()
        {
            if (tokenizer.tokens.Count == 0 || tokenIndex >= tokenizer.tokens.Count) return null;
            var tok = tokenizer.tokens[tokenIndex];
            tokenIndex++;
            return tok;
        }

        JSONPair ParsePair()
        {
            if(PeekToken == null || PeekToken.Kind != TokenType.String)
            {
                LogError("Expected a string", PeekToken, false);
                return null;
            }
            var pairName = NextToken().Lexeme;
            if(PeekToken.Kind != TokenType.Colon)
            {
                LogError("Expected a ':'", PeekToken, false);
                return null;
            }

            //Eat the :
            NextToken();

            if (PeekToken.Kind.Contains(TokenType.Value))
            {
                var token = NextToken();
                return new JSONPair(pairName, token.Lexeme);
            }
            else if(PeekToken.Kind == TokenType.OpenCurly)
            {
                return new JSONPair(pairName, ParseObject());
            }
            else if(PeekToken.Kind == TokenType.OpenBracket)
            {
                return new JSONPair(pairName, ParseArray());
            }
            else
            {
                LogError("Unexpected token:", PeekToken);
                return null;
            }
            
        }

        JSONObject ParseObject()
        {
            JSONObject obj = new JSONObject();

            //Eat the OpenCurly Object
            NextToken();

            while (PeekToken != null)
            {
                if(PeekToken.Kind != TokenType.String)
                {
                    LogError("Expected a string!", PeekToken, false);
                    return null;
                }

                var pair = ParsePair();
                if (pair == null) break;
                else obj.Add(pair);

                if (PeekToken == null || PeekToken.Kind == TokenType.CloseCurly) break;

                if (PeekToken.Kind != TokenType.Comma)
                {
                    LogError("Expected a ,", PeekToken, false);
                }
                
                NextToken();
            }

            if (PeekToken == null)
            {
                LogError("Expected '}'");
                return null;
            }
            else
                //Eat the closing '}'
                NextToken();
            return obj;
        }

        List<object> ParseArray()
        {
            //eat the opening '['
            NextToken();

            List<object> array = new List<object>();
            while(PeekToken != null)
            {
                if (PeekToken.Kind.Contains(TokenType.Value))
                {
                    array.Add(PeekToken.Lexeme);
                }
                else if(PeekToken.Kind == TokenType.OpenBracket)
                {
                    array.Add(ParseArray());
                }
                else if(PeekToken.Kind == TokenType.OpenCurly)
                {
                    array.Add(ParseObject());
                }
                else if(PeekToken.Kind == TokenType.Colon)
                {
                    LogError("Unexpected ':'", PeekToken, false);
                }

                if (PeekToken == null || PeekToken.Kind == TokenType.CloseBracket) break;

                NextToken();

                if (PeekToken.Kind != TokenType.Comma)
                {
                    LogError("Expected a ,", PeekToken, false);
                }
                
                NextToken();
            }

            if (PeekToken == null)
            {
                LogError("Expected ']'");
                return null;
            }
            else
                //Eat the closing ']'
                NextToken();
            return array;
        }

        void Parse()
        {
            JSONObject obj;
            List<object> array;
            if (PeekToken.Kind == TokenType.OpenCurly)
                obj = ParseObject();
            else if (PeekToken.Kind == TokenType.OpenBracket)
                array = ParseArray();
            Console.WriteLine("ok");
        }

        void LogError(string text, Token tok = null, bool showtoken = true)
        {
            if (tok != null)
            {
                if (showtoken)
                    errors.Add(string.Format("Error [{0}:{1}] Token: {2} {3}", tok.Line, tok.Column, tok.Kind.Actual(), text));
                else
                    errors.Add(string.Format("Error [{0}:{1}] {2}", tok.Line, tok.Column, text));
            }
            else
                errors.Add(text);
            IsParserError = true;
        }
    }
}

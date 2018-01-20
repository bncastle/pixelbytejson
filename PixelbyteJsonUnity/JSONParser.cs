using System;
using System.Collections.Generic;
using System.IO;

namespace Pixelbyte.JsonUnity
{
    /// <summary>
    /// Parses the given JSON string
    /// reference: https://www.json.org/
    /// </summary>
    public class JSONParser
    {
        JSONTokenizer tokenizer;
        List<string> errors;

        //This Parsed result, if no errors, will either be
        //a JsonObject, or an array
        public JSONObject rootObject;
        public List<object> rootArray;

        public JSONTokenizer Tokenizer { get { return tokenizer; } }

        public List<string> Errors { get { return errors; } }
        /// <summary>
        /// Gets all the errors. One per line.
        /// </summary>
        public string AllErrors { get { return String.Join(Environment.NewLine, Errors.ToArray()); } }

        public bool Successful { get; private set; }

        int tokenIndex = 0;

        /// <summary>
        /// Peeks at the next available token without consuming it
        /// </summary>
        Token PeekToken
        {
            get
            {
                if (tokenizer.tokens.Count == 0 || tokenIndex >= tokenizer.tokens.Count) return null;
                return tokenizer.tokens[tokenIndex];
            }
        }

        private JSONParser(JSONTokenizer tok)
        {
            tokenizer = tok;
            Successful = true;
            errors = new List<string>();
        }

        public static JSONParser ParseFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();
            //Pull in the json text
            string json;
            using (var sr = new StreamReader(filename))
            {
                json = sr.ReadToEnd();
            }
            return Parse(json);
        }

        public static JSONParser Parse(string json)
        {
            JSONTokenizer tok = new JSONTokenizer();
            JSONParser jp = new JSONParser(tok);

            tok.Tokenize(json);
            //if (tok.Successful)
            {
                try
                {
                    jp.Parse();
                }
                catch (Exception e)
                {
                    jp.LogError(e.Message);
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

        Token PreviousToken()
        {
            if (tokenizer.tokens.Count == 0 || tokenIndex < 2) return null;
            var tok = tokenizer.tokens[tokenIndex - 2];
            tokenIndex++;
            return tok;
        }

        JSONPair ParsePair()
        {
            if (PeekToken == null || PeekToken.Kind != TokenType.String)
            {
                LogError("Expected a string", PeekToken, false);
                return null;
            }
            var pairName = NextToken().Lexeme;

            if (PeekToken == null)
            {
                LogError("Expected a ':'", PreviousToken(), false);
                return null;
            }
            else if(PeekToken.Kind != TokenType.Colon)
            {
                LogError("Expected a ':'", PeekToken, false);
                return null;
            }

            //Eat the :
            NextToken();

            if (PeekToken == null)
            {
                LogError("Expected a value", PreviousToken(), false);
                return null;
            }

            if (PeekToken.Kind.Contains(TokenType.Value))
            {
                var token = NextToken();
                return new JSONPair(pairName, token.Literal);
            }
            else if (PeekToken.Kind == TokenType.OpenCurly)
            {
                return new JSONPair(pairName, ParseObject());
            }
            else if (PeekToken.Kind == TokenType.OpenBracket)
            {
                throw new Exception();
                //TODO: 
                // return new JSONPair(pairName, ParseArray());
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
                //It could be an empty object
                if (PeekToken.Kind == TokenType.CloseCurly) break;

                //Otherwise, we must have a string to indicate a value of some sort
                if (PeekToken.Kind != TokenType.String)
                {
                    LogError("Expected a string!", PeekToken, false);
                    return null;
                }

                var pair = ParsePair();
                if (pair == null) break;
                else obj.Add(pair.name, pair.value);

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
            while (PeekToken != null)
            {
                if (PeekToken.Kind.Contains(TokenType.Value))
                {
                    array.Add(PeekToken.Lexeme);
                }
                else if (PeekToken.Kind == TokenType.OpenBracket)
                {
                    array.Add(ParseArray());
                }
                else if (PeekToken.Kind == TokenType.OpenCurly)
                {
                    array.Add(ParseObject());
                }
                else if (PeekToken.Kind == TokenType.Colon)
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
            if (PeekToken == null)
                LogError("No valid JSON tokens found!");
            else if (PeekToken.Kind == TokenType.OpenCurly)
                rootObject = ParseObject();
            else if (PeekToken.Kind == TokenType.OpenBracket)
                rootArray = ParseArray();
        }

        void LogError(string text, Token tok = null, bool showtoken = true)
        {
            if (tok != null)
            {
                if (showtoken)
                    errors.Add(string.Format("Parser [{0}:{1}] Token: {2}: {3}", tok.Line, tok.Column, tok.Kind.Actual(), text));
                else
                    errors.Add(string.Format("Parser [{0}:{1}]: {2}", tok.Line, tok.Column, text));
            }
            else
                errors.Add("Parser: " +  text);
            Successful = false;
        }
    }
}
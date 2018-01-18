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
        public JSONObject(string name)
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

        public List<JSONObject> objects;

        JSONTokenizer tokenizer;
        List<string> errors;


        public bool IsTokenizerError { get { return tokenizer.IsError; } }
        public List<string> TokenizerErrors { get { return tokenizer.errors; } }
        public List<string> ParserErrors { get { return errors; } }

        public bool IsParserError { get; private set; }

        private JSONParser(JSONTokenizer tok)
        {
            tokenizer = tok;
            IsParserError = false;
            errors = new List<string>();
            objects = new List<JSONObject>();
        }

        public static JSONParser Parse(string json)
        {
            JSONTokenizer tok = new JSONTokenizer();
            JSONParser jp = new JSONParser(tok);

            //1st tokenize
            tok.Tokenize(json);
            if (!tok.IsError)
            {
                foreach (var t in tok.tokens)
                    Console.WriteLine(t.ToString());
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

        void Parse()
        {
            JSONObject jsonObj = null;
            JSONPair jsonPair = null;
            bool inArray = false;

            int arrayDepth = 0;
            int objectDepth = 0;
            TokenType expectNext = TokenType.None;

            if (tokenizer.tokens.Count == 0)
            {
                LogError("No JSON found!");
                return;
            }
            else
            {
                //Ensure that the JSON is valid
                var firstTok = tokenizer.tokens[0];
                if (firstTok.Kind != TokenType.OpenBracket && firstTok.Kind != TokenType.OpenCurly)
                {
                    LogError("JSON must start with an array or an object!", firstTok);
                    return;
                }
            }

            for (int i = 0; i < tokenizer.tokens.Count; i++)
            {
                var token = tokenizer.tokens[i];
                switch (token.Kind)
                {
                    case TokenType.OpenCurly:
                        objectDepth++;
                        break;
                    case TokenType.ClosedCurly:
                        objectDepth--;
                        if (objectDepth < 0)
                        {
                            LogError("Mismatched closing '}'", token);
                            return;
                        }
                        break;
                    case TokenType.OpenBracket:
                        arrayDepth++;
                        inArray = true;
                        break;
                    case TokenType.CloseBracket:
                        arrayDepth--;
                        if (arrayDepth == 0) inArray = false;
                        if (arrayDepth < 0)
                        {
                            LogError("Mismatched closing ']'", token);
                            return;
                        }
                        break;
                    case TokenType.Colon:
                        break;
                    case TokenType.Comma:
                        break;
                    case TokenType.String:
                        break;
                    case TokenType.Number:
                        break;
                    case TokenType.True:
                        break;
                    case TokenType.False:
                        break;
                    case TokenType.Null:
                        break;
                    case TokenType.None:
                        break;
                    default:
                        break;
                }
            }

            //After Parsing is complete, both the object and array depth should be 0
            if (objectDepth > 1)
                LogError(string.Format("Missing {0} '}'!", objectDepth));
            else if (objectDepth == 1)
                LogError(string.Format("Missing a '}'!", objectDepth));

            if (arrayDepth > 1)
                LogError(string.Format("Missing {0} ']'!", arrayDepth));
            else if (arrayDepth == 1)
                LogError(string.Format("Missing a ']'!", arrayDepth));
        }

        void LogError(string text, Token tok = null)
        {
            if (tok != null)
                errors.Add(string.Format("Error [{0}:{1}] Token: {2} {3}", tok.Line, tok.Column, tok.Kind, text));
            else
                errors.Add(text);
            IsParserError = true;
        }
    }
}

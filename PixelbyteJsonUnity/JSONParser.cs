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

    /// <summary>
    /// Parses the given JSON string
    /// reference: https://www.json.org/
    /// </summary>
    public class JSONParser
    {
        const byte EMPTY = 0;
        const byte INARRAY = 1;
        const byte INOBJECT = 2;

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

        //Tells us if we are currently in an object or an array
        Stack<byte> depthStack;
        /// <summary>
        /// Tells us if the next expected token should be a value
        /// </summary>
        bool shouldBeValue;

        public bool IsTokenizerError { get { return tokenizer.IsError; } }
        public List<string> TokenizerErrors { get { return tokenizer.errors; } }
        public List<string> ParserErrors { get { return errors; } }
        public bool IsParserError { get; private set; }

        int tokenIndex = 0;

        /// <summary>
        /// Gets the next available token and advances to the next one
        /// </summary>
        Token NextToken
        {
            get
            {
                if (tokenizer.tokens.Count == 0 || tokenIndex >= tokenizer.tokens.Count) return null;
                var tok = tokenizer.tokens[tokenIndex];
                tokenIndex++;
                return tok;
            }
        }

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
                if (tokenizer.tokens.Count == 0 || tokenIndex == 0) return null;
                return tokenizer.tokens[tokenIndex];
            }
        }

        bool InArray { get { return depthStack.Count > 0 && depthStack.Peek() == INARRAY; } }
        bool InObject { get { return depthStack.Count > 0 && depthStack.Peek() == INOBJECT; } }

        private JSONParser(JSONTokenizer tok)
        {
            tokenizer = tok;
            IsParserError = false;
            errors = new List<string>();
            objects = new List<JSONObject>();
            depthStack = new Stack<byte>(16);
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

        void DepthPushObject() { depthStack.Push(INOBJECT); }
        void DepthPushArray() { depthStack.Push(INARRAY); }
        byte DepthPop() { if (depthStack.Count == 0) return EMPTY; else return depthStack.Pop(); }

        //object ParseValue()
        //{
        //    if()
        //}

        void Parse()
        {
            JSONObject jsonObj = null;
            JSONPair jsonPair = null;
            depthStack.Clear();

            TokenType expectNext = TokenType.OpenCurly | TokenType.OpenBracket;

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

            var prevKind = TokenType.None;
            for (int i = 0; i < tokenizer.tokens.Count; i++)
            {
                var token = tokenizer.tokens[i];
                var kind = token.Kind;

                //Console.WriteLine(token.ToString());

                if (kind.IsExpected(expectNext))
                {
                    switch (kind)
                    {
                        case TokenType.None:
                            break;
                        case TokenType.OpenCurly:
                            DepthPushObject();
                            expectNext = TokenType.String;
                            break;
                        case TokenType.ClosedCurly:
                            if (!InObject)
                            {
                                LogError("Mismatched closing '}'", token);
                                return;
                            }
                            DepthPop();
                            expectNext = TokenType.None;
                            break;
                        case TokenType.OpenBracket:
                            DepthPushArray();
                            expectNext = TokenType.Value;
                            break;
                        case TokenType.CloseBracket:
                            if (!InArray)
                            {
                                LogError("Mismatched closing ']'", token);
                                return;
                            }
                            DepthPop();
                            expectNext = TokenType.Comma;

                            //Also if we're in an object or an array, we could expect the close of those as well
                            if (InObject) expectNext |= TokenType.ClosedCurly;
                            else if (InArray) expectNext |= TokenType.CloseBracket;

                            break;
                        case TokenType.Colon:
                            //Any value can proceeed a colon
                            expectNext = TokenType.Value | TokenType.OpenBracket | TokenType.OpenCurly;
                            break;
                        case TokenType.Comma:
                            expectNext = TokenType.Value;
                            if (InArray)
                                expectNext |= TokenType.OpenCurly | TokenType.OpenBracket;
                            break;
                        case TokenType.String:
                            if (InArray)
                                expectNext = TokenType.Comma | TokenType.CloseBracket;
                            else if (InObject)
                            {
                                if (prevKind == TokenType.Colon)
                                    expectNext = TokenType.Comma | TokenType.ClosedCurly;
                                else
                                    expectNext = TokenType.Comma | TokenType.Colon | TokenType.OpenBracket | TokenType.OpenCurly | TokenType.ClosedCurly;
                            }
                            else
                                LogError("Unexpected vaue!", token, false);
                            break;
                        case TokenType.Number:
                        case TokenType.True:
                        case TokenType.False:
                        case TokenType.Null:
                            expectNext = TokenType.Comma;
                            if (InArray)
                                expectNext |= TokenType.CloseBracket;
                            else if (InObject)
                                expectNext |= TokenType.ClosedCurly;

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    LogError("Expected: " + expectNext.ListActual(), token, false);
                    break;
                }

                prevKind = kind;
            }

            if (IsParserError) return;

            //After Parsing is complete, both the object and array depth should be 0
            if (InObject)
            {
                var lastToken = tokenizer.tokens[tokenizer.tokens.Count - 1];
                LogError(" Missing '}'!", lastToken, false);
            }
            else if (InArray)
            {
                var lastToken = tokenizer.tokens[tokenizer.tokens.Count - 1];
                LogError("Missing ']'!", lastToken, false);
            }
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

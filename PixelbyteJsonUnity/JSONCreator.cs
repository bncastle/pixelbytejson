using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    public class JSONCreator
    {
        StringBuilder builder;
        bool prettyPrint;
        bool startOfLine = true;
        int indentLevel;

        public JSONCreator(bool prettyPrint)
        {
            builder = new StringBuilder();
            this.prettyPrint = prettyPrint;
        }

        public void LineBreak()
        {
            if (!prettyPrint) return;
            builder.Append('\n');
            startOfLine = true;

        }

        void Indent()
        {
            if (!startOfLine) return;
            startOfLine = false;
            for (int i = 0; i < indentLevel; i++)
                builder.Append('\t');
        }

        void DeIndent()
        {
            if (!startOfLine) return;
            builder.Length = builder.Length - indentLevel;
            indentLevel--;
        }

        public void BeginObject(bool newline = true) { Indent(); builder.Append('{'); indentLevel++; if (newline) LineBreak(); }
        public void EndObject()
        {
            indentLevel--;
            ////There will also be a space after the comma so we include that too
            if (builder[builder.Length - 3] == ',')
            {
                builder.Length = builder.Length - 3;
                LineBreak();
            }
            Indent();

            builder.Append("}");
        }

        public void BeginArray() { builder.Append('['); indentLevel++; }
        public void EndArray()
        {
            indentLevel--;
            ////There will also be a space after the comma so we include that too
            if (builder[builder.Length - 3] == ',')
            {
                builder.Length = builder.Length - 3;
                LineBreak();
            }
            Indent();

            builder.Append("]");
        }

        public void Colon() { builder.Append(" : "); }
        public void Comma() { builder.Append(", "); }
        public void Null() { builder.Append("null"); }
        public void Bool(bool flag) { builder.Append(flag ? "true" : "false"); }

        public void Value(object value)
        {
            if (value == null) Null();
            else if (value is bool) Bool((bool)value);
            else if (value is string) String((string)value);
            else if (value is DateTime) String(value.ToString());
            else if (value.GetType().IsEnum) String(value.ToString());
            else if (value is char) String(value.ToString());
            else if (value.GetType().IsNumeric()) Number(value);
            else throw new NotImplementedException("Type: " + value.ToString() + " not implemented");
        }

        //Tells which types this builder supports natively
        public bool ValueSupported(object value)
        {
            return (value == null || value is bool || value is string || value is DateTime || value.GetType().IsEnum || value is char || value.GetType().IsNumeric());
        }


        public void Number(object number)
        {
            if (!number.GetType().IsNumeric())
                throw new ArgumentException("Expected a number!");
            var text = number.ToString();
            if (number.GetType().IsFloatingPoint() && text.IndexOf('.') == -1) text += ".0";

            builder.Append(text);
        }

        public void String(string text)
        {
            Indent();
            //Quote the string and also look for any escape characters  since we'll need to escape them again
            builder.Append('"');
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    default: //Check for a unicode char code
                        int val = Convert.ToInt32(text[i]);
                        //Is it a printable character?
                        if (val >= 32 && val <= 126)
                            builder.Append(text[i]);
                        else //Unicode escape
                        {
                            builder.Append("\\u");
                            //Convert the value to hex
                            builder.Append(Convert.ToString(val, 16).PadLeft(4, '0'));
                        }
                        break;
                }
            }
            builder.Append('"');
        }

        public void List(IList list)
        {
            if (list.Count == 0) builder.Append("[]");
            else
            {
                foreach (var item in list)
                {
                    BeginArray();
                    //
                    EndArray();
                }
            }
        }

        public void Pair(string name, string value)
        {
            String(name);
            builder.Append(" : ");

            if (string.IsNullOrEmpty(value))
                Null();
            else
                builder.Append(value);
            builder.Append(", ");
            LineBreak();
        }

        public override string ToString() { return builder.ToString(); }
    }
}

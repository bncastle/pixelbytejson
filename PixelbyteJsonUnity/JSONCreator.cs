using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    class JSONCreator
    {
        StringBuilder builder;
        bool prettyPrint;
        int indentLevel;

        public JSONCreator(bool prettyPrint)
        {
            builder = new StringBuilder();
            this.prettyPrint = prettyPrint;
        }

        void LineBreak()
        {
            if (!prettyPrint) return;
            builder.Append('\n');
            for (int i = 0; i < indentLevel; i++)
                builder.Append('\t');
        }

        void Indent()
        {
            indentLevel++;
            LineBreak();
        }

        void DeIndent()
        {
            builder.Length = builder.Length - indentLevel;
            indentLevel--;
        }

        public void BeginObject() { builder.Append('{'); Indent(); }
        public void EndObject()
        {
            DeIndent();
            if (builder[builder.Length - 2] == ',')
                builder.Length = builder.Length - 2;
            builder.Append(" }");
        }

        public void BeginArray() { builder.Append('['); Indent(); }
        public void EndArray() { DeIndent(); builder.Append(']'); }

        public void Colon() { builder.Append(" : "); }
        public void Null() { builder.Append("null"); }
        public void Bool(bool flag) { builder.Append(flag ? "true" : "false"); }

        public void Number(object number, DecimalPlacesAttribute formatter)
        {
            if (!number.GetType().IsNumeric())
                throw new ArgumentException("Expected a number!");

            if (number.GetType().IsInteger())
                builder.Append(number);
            //else if (formatter != null)
            //    builder.Append(formatter.Convert(number));
            else
                builder.Append(number);
        }

        public void String(string text)
        {
            //Quote the string and also look for any escape characters 
            //since we'll need to escape them again
            builder.Append('"');
            builder.Append(text);
            builder.Append('"');
        }

        public void Pair(string name, object value)
        {
            String(name);
            builder.Append(" : ");
            builder.Append(value.ToString());
            builder.Append(", ");
            LineBreak();
        }

        public override string ToString() { return builder.ToString(); }
    }
}

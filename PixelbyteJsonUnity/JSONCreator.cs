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
            builder.Append('\n');
            for (int i = 0; i < indentLevel; i++)
                builder.Append('\t');
        }

        public void BeginObject() { builder.Append('{'); indentLevel++; if (prettyPrint) LineBreak(); }
        public void EndObject() { builder.Append('{'); indentLevel--; }

        public void BeginArray() { builder.Append('['); }
        public void EndArray() { builder.Append(']'); }
        public void Null() { builder.Append("null"); }
        public void Bool(bool flag) { builder.Append(flag ? "true" : "false"); }
        public void Number(object number) { }

        public override string ToString() { return builder.ToString(); }
    }
}

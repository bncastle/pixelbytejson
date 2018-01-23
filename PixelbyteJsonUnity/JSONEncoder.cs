using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    /// <summary>
    /// Helps with building the JSON string when we serialize a class
    /// </summary>
    public class JSONEncoder
    {
        public delegate void EncodeCallback(object obj, JSONEncoder encoder);

        //Contains all supported JSON encoders
        static Dictionary<Type, EncodeCallback> typeEncoders;
        static EncodeCallback defaultTypeEncoder;

        StringBuilder builder;
        bool prettyPrint;
        bool startOfLine = true;
        int indentLevel;

        static JSONEncoder() { typeEncoders = new Dictionary<Type, EncodeCallback>(); AddDefaults(); }

        #region Static Encoder Methods

        public static void SetTypeEncoder(Type type, EncodeCallback encodeFunc) { typeEncoders[type] = encodeFunc; }
        public static void RemoveTypeEncoder(Type type) { typeEncoders.Remove(type); }
        public static void ClearTypeEncoders() { typeEncoders.Clear(); }
        public EncodeCallback GetTypeEncoder(Type type) { EncodeCallback callback = null; typeEncoders.TryGetValue(type, out callback); return callback; }
        public EncodeCallback GetTypeEncoderOrDefault(Type type) { EncodeCallback callback = GetTypeEncoder(type); if (callback == null) callback = defaultTypeEncoder; return callback; }

        #endregion

        public JSONEncoder(bool prettyPrint)
        {
            builder = new StringBuilder();
            this.prettyPrint = prettyPrint;

        }

        #region Encode methods

        public static string Encode(object obj, bool prettyPrint = true)
        {
            JSONEncoder creator = new JSONEncoder(prettyPrint);
            Encode(obj, creator);
            return creator.ToString();
        }

        static string Encode(object obj, JSONEncoder creator)
        {
            //Object to serialize can't be null [TODO: Or can it?]
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            //See if the object implements the Serialization callbacks interface
            var callbacks = obj as ISerializeCallbacks;
            var serializationControl = obj as ISerializationControl;

            if (callbacks != null) callbacks.PreSerialization();

            var encodeMethod = creator.GetTypeEncoderOrDefault(type);
            encodeMethod(obj, creator);

            if (callbacks != null) callbacks.PostSerialization();
            return creator.ToString();
        }
        #endregion

        #region JSON text output methods

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
            if (builder.Length > 3 && builder[builder.Length - 3] == ',')
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

        public void EncodePair(string name, object value)
        {
            String(name);
            Colon();
            EncodeValue(value);
        }

        public void EncodeValue(object value)
        {
            if (ValueSupported(value))
                Value(value);
            else
            {
                var type = value.GetType();

                EncodeCallback encode = null;
                //Try a dictionary first
                if (type.HasInterface(typeof(IDictionary)))
                    encode = GetTypeEncoder(typeof(IDictionary));
                else if (type.HasInterface(typeof(IEnumerable)))
                    encode = GetTypeEncoder(typeof(IEnumerable));
                else
                    encode = GetTypeEncoder(value.GetType());
                if (encode != null)
                    encode(value, this);
                else
                    Encode(value, this);
            }
        }
        #endregion

        public override string ToString() { return builder.ToString(); }

        static void AddDefaults()
        {
            defaultTypeEncoder = ((obj, builder) =>
            {
                Type type = obj.GetType();
                builder.BeginObject();

                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    //If the field is private or protected we need to check and see if it has an attribute that allows us to include it
                    //Or if the field should be excluded, then skip it
                    if (((field.IsPrivate || field.IsFamily) && !field.HasAttribute<JsonIncludeAttribute>())
                        || field.HasAttribute<JsonExcludeAttribute>())
                        continue;

                    var value = field.GetValue(obj);
                    builder.EncodePair(field.Name, value);

                    //Format for another name value pair
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndObject();
            });

            SetTypeEncoder(typeof(IEnumerable), (obj, builder) =>
            {
                builder.BeginArray();
                builder.LineBreak();
                foreach (var item in (IEnumerable)obj)
                {
                    builder.EncodeValue(item);
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndArray();
            });

            SetTypeEncoder(typeof(IDictionary), (obj, builder) =>
            {
                builder.BeginObject();
                var table = obj as IDictionary;
                foreach (var key in table.Keys)
                {
                    builder.EncodePair(key.ToString(), table[key]);
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndObject();
            });
        }
    }
}

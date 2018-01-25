using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixelbyte.Json
{
    /// <summary>
    /// Helps with building the JSON string when we serialize a class
    /// </summary>
    public class JSONEncoder
    {
        //The presence of this string as a key in a Json object indicates the 
        //type of object that it represents if it is present. Otherwise, we just fit
        //the object to the type of the field. This field is necessary when the field is some 
        //abstract class but the actual value is a subclass of it.
        public const string TypeNameString = "@type";

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

        public JSONEncoder(bool prettyPrint, bool storeTypeInformation)
        {
            builder = new StringBuilder();
            this.prettyPrint = prettyPrint;
        }

        #region Encode methods

        public static string Encode(object obj, bool storeTypeInfo = false, bool prettyPrint = true)
        {
            JSONEncoder creator = new JSONEncoder(prettyPrint, storeTypeInfo);
            creator.Encode(obj);
            return creator.ToString();
        }

        void EncodeViaJsonEncodingControl(IJsonEncodeControl control)
        {
            EncodeInfo encodeData = new EncodeInfo();
            control.GetSerializedData(encodeData);
            if (encodeData.Count == 0)
                throw new Exception(string.Format("Object of Type {0} implements IJsonEncodingControl but returned no Encoding Data!", control.GetType().Name));

            BeginObject();
            foreach (KeyValuePair<string, object> item in encodeData)
            {
                EncodePair(item.Key, item.Value);
                Comma();
                LineBreak();
            }
            EndObject();
        }

        string Encode(object obj)
        {
            //Object to serialize can't be null [TODO: Or can it?]
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type type = obj.GetType();

            //See if the object implements the Serialization callbacks interface
            var callbacks = obj as IJsonEncodeCallbacks;
            var serializationControl = obj as IJsonEncodeControl;

            if (callbacks != null) callbacks.OnPreJsonEncode();

            //If the object implements serialization control, then use it instead
            if (serializationControl != null)
            {
                EncodeViaJsonEncodingControl(serializationControl);
            }
            else
            {
                var encodeMethod = GetTypeEncoderOrDefault(type);
                encodeMethod(obj, this);
            }

            if (callbacks != null) callbacks.OnPostJsonEncode();
            return ToString();
        }
        #endregion

        #region JSON text output methods

        public void LineBreak()
        {
            if (!prettyPrint) return;

            //Remove any whitespace from the end of this line
            EatEndingWhitespace();

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
            if (builder.Length - indentLevel > 0)
                builder.Length = builder.Length - indentLevel;
            indentLevel--;
        }

        public void BeginObject(bool newline = true) { Indent(); builder.Append('{'); indentLevel++; if (newline) LineBreak(); }
        public void EndObject()
        {
            indentLevel--;
            //Get rid of any commas or spaces at the end here
            if (EatFromEnd(','))
            {
                LineBreak();
            }
            Indent();

            builder.Append("}");
        }

        void EatEndingWhitespace()
        {
            while (builder.Length > 0 && char.IsWhiteSpace(builder[builder.Length - 1])) builder.Length--;
        }

        bool EatFromEnd(char c)
        {
            int index = builder.Length - 1;

            while (index > 0 && char.IsWhiteSpace(builder[index]) && builder[index] != c)
                index--;

            //Be sure to remove the character stored in c also
            if (index < builder.Length - 1)
            {
                builder.Length = index;
                return true;
            }
            else
                return false;
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

        public void Colon() { builder.Append(": "); }
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
                    Encode(value);
            }
        }

        public void WriteTypeInfoIfAttributePresent(Type type)
        {
            //If this type is a class and any of its baseClasses have
            //a JsonTypeHint attribute, then write the type out
            if (!type.IsClass || !type.HasAttribute<JsonTypeHint>(true)) return;

            EncodePair("@type", type.FullName);
            Comma();
            LineBreak();
        }

        #endregion

        static void AddDefaults()
        {
            defaultTypeEncoder = ((obj, builder) =>
            {
                Type type = obj.GetType();
                builder.BeginObject();

                builder.WriteTypeInfoIfAttributePresent(type);

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

        public override string ToString() { return builder.ToString(); }
    }
}

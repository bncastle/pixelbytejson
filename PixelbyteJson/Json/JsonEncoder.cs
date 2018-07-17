using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Pixelbyte.Json
{
    /// <summary>
    /// Helps with building the JSON string when we serialize a class
    /// </summary>
    public class JsonEncoder
    {
        public const BindingFlags DEFAULT_JSON_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        //The presence of this string as a key in a Json object indicates the 
        //type of object that it represents. If it isn't in the JSON data, we try to fit
        //the object to the type of the field. Having type information is necessary when the field is some 
        //abstract class but the actual value is a subclass of it.
        internal const string TypeNameString = "@type";

        //Signature for all EncodeCallback methods
        public delegate void EncodeMethod(object obj, JsonEncoder encoder);

        //Contains all supported JSON encoders
        static TypeComparer typeComparer;
        static Dictionary<Type, EncodeMethod> typeEncoders;
        static EncodeMethod defaultTypeEncoder;

        StringBuilder builder;
        //format the JSON output with newlines and tabs
        bool prettyPrint;

        /// <summary>
        /// If true, then any enums are encoded as their actual string value
        /// False encodes enums as ints
        /// </summary>
        bool enumsAsStrings;

        bool startOfLine = true;
        int indentLevel;

        static JsonEncoder()
        {
            typeComparer = new TypeComparer();
            typeEncoders = new Dictionary<Type, EncodeMethod>(typeComparer);
            AddDefaults();
        }

        #region Static Encoder Methods
        public static void SetTypeEncoder(Type type, EncodeMethod encodeFunc) { typeEncoders[type] = encodeFunc; }
        public static void SetDefaultEncoder(EncodeMethod defaultEncoder) { defaultTypeEncoder = defaultEncoder; }
        public static void RemoveTypeEncoder(Type type) { typeEncoders.Remove(type); }
        public static void ClearTypeEncoders() { typeEncoders.Clear(); }
        public EncodeMethod GetTypeEncoder(Type type) { EncodeMethod callback = null; typeEncoders.TryGetValue(type, out callback); return callback; }
        public EncodeMethod GetTypeEncoderOrDefault(Type type)
        {
            EncodeMethod callback = GetTypeEncoder(type);
            if (callback == null)
            {
                if (type.HasInterface(typeof(IEnumerable)))
                    callback = GetTypeEncoder(typeof(IEnumerable));
                else if (type.HasInterface(typeof(IDictionary)))
                    callback = GetTypeEncoder(typeof(IDictionary));

                if (callback == null)
                    callback = defaultTypeEncoder;
            }
            return callback;
        }

        #endregion

        JsonEncoder(bool enumsAsStrings, bool prettyPrint)
        {
            builder = new StringBuilder();
            this.prettyPrint = prettyPrint;
            this.enumsAsStrings = enumsAsStrings;
        }

        #region Encode methods

        public static string Encode(object obj, bool prettyPrint = true, bool enumsAsStrings = false)
        {
            JsonEncoder creator = new JsonEncoder(enumsAsStrings, prettyPrint);
            creator.Encode(obj);
            return creator.ToString();
        }

        void EncodeViaJsonEncodingControl(IJsonEncodeControl control)
        {
            EncodeInfo encodeData = new EncodeInfo();
            control.GetSerializedData(encodeData);
            if (encodeData.Count == 0)
                throw new Exception(string.Format("Object of Type {0} implements {1} but returned no EncodeInfo data!", control.GetType().FriendlyName(), typeof(IJsonEncodeControl).Name));

            BeginObject();
            //Include type information
            WriteTypeInfoIfAttributePresent(control.GetType());

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
            var preEncodeCallback = type.FindMethodWith<JsonPreEncodeAttribute>(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var encodedCallback = type.FindMethodWith<JsonEncodedAttribute>( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            var serializationControl = obj as IJsonEncodeControl;

            preEncodeCallback?.Invoke(obj, null);

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

            encodedCallback?.Invoke(obj, null);
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

            //Be sure to remove the character stored in c also but ONLY if it is c!
            if (index < builder.Length - 1 && builder[index] == c)
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
            if (EatFromEnd(','))
            {
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
            else if (value.GetType().IsEnum)
            {
                if (enumsAsStrings)
                    String(value.ToString());
                else
                {
                    //Get the underlying type of the Enum (it can be int, uint, ushort, byte, etc)
                    var enumType = Enum.GetUnderlyingType(value.GetType());
                    var convertedType = Convert.ChangeType(value, enumType);
                    Number(convertedType);
                }
            }
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

                EncodeMethod encode = null;
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
            if (!type.IsClass || type.GetCustomAttribute<JsonTypeHintAttribute>(true) == null) return;

            EncodePair(TypeNameString, type.FullName);
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

                obj.EnumerateFields(DEFAULT_JSON_BINDING_FLAGS, (targetObj, field, jsonName) =>
                {
                    var value = field.GetValue(targetObj);
                    builder.EncodePair(jsonName, value);

                    //Format for another name value pair
                    builder.Indent();
                    builder.Comma();
                    builder.LineBreak();
                });

                builder.EndObject();
            });

            SetTypeEncoder(typeof(IEnumerable), (obj, builder) =>
            {
                builder.BeginArray();
                builder.LineBreak();
                foreach (var item in (IEnumerable)obj)
                {
                    builder.Indent();
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
                    builder.Indent();
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

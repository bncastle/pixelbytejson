using System.Reflection;

namespace Pixelbyte.JsonUnity
{
    public class SerializationProxy
    {
        public SerializationProxy() { }

        public void GetObjectData(object obj, SerializationData data)
        {
            var fi = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var fieldInfo in fi)
            {
                //If the field is private or protected we need to check and see if it has an attribute that allows us to include it
                //Or if the data already has that field in it
                if (((fieldInfo.IsPrivate || fieldInfo.IsFamily) && !fieldInfo.HasAttribute<JsonIncludeAttribute>())
                    || data.HasKey(fieldInfo.Name))
                    continue;

                object value = fieldInfo.GetValue(obj);
                data[fieldInfo.Name] = value.ToString();
            }
        }
    }
}

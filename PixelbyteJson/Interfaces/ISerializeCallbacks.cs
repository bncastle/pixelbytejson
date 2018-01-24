namespace Pixelbyte.Json
{
    public interface ISerializeCallbacks
    {
        void PreSerialization();
        void PostSerialization();
    }
}

namespace Pixelbyte.JsonUnity
{
    public interface ISerializeCallbacks
    {
        void PreSerialization();
        void PostSerialization();
    }
}
